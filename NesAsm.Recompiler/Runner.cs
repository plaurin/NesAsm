using NesAsm.Emulator;

namespace NesAsm.Recompiler;

public class Runner
{
    private readonly PPUInstance _ppu;
    private readonly NesMemory _memory;
    private readonly HashSet<ushort> _instructionAddresses = [];
    private readonly Dictionary<ushort, Sub> _subs = [];
    private readonly Dictionary<ushort, ushort>  _jumpTable = [];
    private readonly Dictionary<ushort, HashSet<ushort>> _jumpITable = [];
    private readonly List<string> _callstack = [];

    public Runner(Cart cart)
    {
        _ppu = new PPUInstance();
        _memory = new NesMemory(cart, _ppu);
        Cpu = new CPU(_memory);
        _memory.SetCPU(Cpu);
    }

    public CPU Cpu { get; init; }

    public IEnumerable<Subroutine> Subroutines => RomMemoryRegions.OfType<Subroutine>();


    public IEnumerable<MemoryRegion> RomMemoryRegions
    {
        get
        {
            var result = new List<MemoryRegion>();
            var subs = new List<Subroutine>();

            var instructions = new List<Instruction>();

            // Instructions
            var nextAddress = 0x8000;
            foreach (var address in _instructionAddresses.OrderBy(a => a))
            {
                if (address == 0x8657) { }
                if (address != nextAddress)
                {
                    if (instructions.Any(i => i.IsBranch && i.Argument == address))
                    {
                        while (nextAddress < address)
                        {
                            // fill gap
                            var parsedIns = Parser.GetInstruction(Cpu.Cart.PrgRom, (ushort)nextAddress, executed: false);
                            instructions.Add(parsedIns);
                            nextAddress = parsedIns.Address + parsedIns.Bytes;
                        }
                        // assert nextAddress == address
                    }
                    else
                    {
                        // new sub
                        subs.Add(new Subroutine(instructions));
                        instructions = [];
                    }
                }

                var instruction = Parser.GetInstruction(Cpu.Cart.PrgRom, address, executed: true);
                nextAddress = instruction.Address + instruction.Bytes;
                instructions.Add(instruction);

                if (instruction.IsReturn || _jumpTable.Any(j => j.Value == nextAddress) || _jumpITable.Any(j => j.Value.Any(t => t == nextAddress)))
                {
                    subs.Add(new Subroutine(instructions));
                    instructions = [];
                }
            }

            // Data
            var dataRegions = new List<DataRegion>();
            foreach (var readAccess in GetMemoryAccess(_memory.Reads, isRead: true, isWrite: false, subroutinesOverride: subs).Where(m => m.IsROMAccess))
            {
                dataRegions.Add(new DataRegion(readAccess.TargetAddress, readAccess.Size ?? 1));
            }

            result.AddRange(subs.OfType<MemoryRegion>().Where(s => s.Size > 0).Concat(dataRegions).OrderBy(a => a.Address));

            // Unknown regions
            var unknownRegions = new List<UnknownRegion>();
            nextAddress = 0x8000;
            foreach (var memoryRegion in result)
            {
                if (memoryRegion.Address != nextAddress)
                {
                    unknownRegions.Add(new UnknownRegion((ushort)nextAddress, memoryRegion.Address - nextAddress));
                }

                nextAddress = memoryRegion.Address + memoryRegion.Size;
            }

            if (nextAddress < 0xFFF9)
            {
                unknownRegions.Add(new UnknownRegion((ushort)nextAddress, 0xFFF9 - nextAddress + 1));
            }

            result.AddRange(unknownRegions);

            return result.OrderBy(s => s.Address);
        }
    }

    public IEnumerable<MemoryAccessRecord> Reads() => GetMemoryAccess(_memory.Reads, isRead: true, isWrite: false);

    public IEnumerable<MemoryAccessRecord> Writes() => GetMemoryAccess(_memory.Writes, isRead: false, isWrite: true);

    private IEnumerable<MemoryAccessRecord> GetMemoryAccess(Dictionary<ushort, HashSet<ushort>> records, bool isRead, bool isWrite, IReadOnlyCollection<Subroutine>? subroutinesOverride = null)
    {
        foreach (var record in records.OrderBy(r => r.Key))
        {
            var sourceAddress = record.Key;
            var sub = (subroutinesOverride ?? Subroutines).First(s => s.IsInSub(sourceAddress));
            var ins = Parser.GetInstruction(Cpu.Cart.PrgRom, sourceAddress);
            if (ins.Address == 0x85C8) { }

            IEnumerable<ushort> direct = null!;
            IEnumerable<ushort> indirect = null!;
            int? size = null!;

            if (ins.Mode == AddressingMode.IndirectY || ins.Mode == AddressingMode.IndirectX || ins.Mode == AddressingMode.Indirect)
            {
                direct = record.Value.Take(2);
                indirect = record.Value.Skip(2);
            }
            else if (ins.Mode == AddressingMode.ZeroPageX || ins.Mode == AddressingMode.ZeroPageY || ins.Mode == AddressingMode.AbsoluteX || ins.Mode == AddressingMode.AbsoluteY)
            {
                indirect = record.Value;
            }
            else
            {
                if (record.Value.Count == 1)
                    direct = record.Value.Take(1);
                else
                {
                    indirect = record.Value;
                }
            }

            if (direct?.Any() == true)
            {
                foreach (var targetAddress in direct.OrderBy(a => a))
                {
                    yield return new MemoryAccessRecord(sub, ins,
                        RomAddress: sourceAddress,
                        TargetAddress: targetAddress,
                        IsRead: isRead,
                        IsWrite: isWrite,
                        IsJump: false,
                        IsBranch: false,
                        IsDirectAccess: true);
                }
            }

            if (indirect?.Any() == true)
            {
                size = indirect.OrderBy(a => a).Last() - indirect.OrderBy(a => a).First() + 1;
                if (indirect.Count() * 10 < size) size = null;

                if (size.HasValue)
                {
                    yield return new MemoryAccessRecord(sub, ins,
                        RomAddress: sourceAddress,
                        TargetAddress: indirect.OrderBy(a => a).First(),
                        IsRead: isRead,
                        IsWrite: isWrite,
                        IsJump: false,
                        IsBranch: false,
                        IsDirectAccess: false,
                        Size: size);
                }
                else
                {
                    foreach (var targetAddress in indirect.OrderBy(a => a))
                    {
                        yield return new MemoryAccessRecord(sub, ins,
                            RomAddress: sourceAddress,
                            TargetAddress: targetAddress,
                            IsRead: isRead,
                            IsWrite: isWrite,
                            IsJump: false,
                            IsBranch: false,
                            IsDirectAccess: false);
                    }
                }
            }
        }
    }

    public IEnumerable<MemoryAccessRecord> DirectJumpTableMemoryAccess()
    {
        foreach (var jumpAddress in _jumpTable)
        {
            var sub = Subroutines.First(s => s.IsInSub(jumpAddress.Key));
            var ins = Parser.GetInstruction(Cpu.Cart.PrgRom, jumpAddress.Key);
            if (ins.Address == 0x85C8) { }

            yield return new MemoryAccessRecord(sub, ins,
                RomAddress: jumpAddress.Key,
                TargetAddress: jumpAddress.Value,
                IsRead: false,
                IsWrite: false,
                IsJump: true,
                IsBranch: false,
                IsDirectAccess: true);
        }
    }

    public IEnumerable<MemoryAccessRecord> IndirectJumpTableMemoryAccess()
    {
        foreach (var jumpAddress in _jumpITable)
        {
            var sub = Subroutines.Single(s => s.IsInSub(jumpAddress.Key));
            var ins = Parser.GetInstruction(Cpu.Cart.PrgRom, jumpAddress.Key);
            foreach (var targetAddress in jumpAddress.Value.OrderBy(a => a))
            {
                yield return new MemoryAccessRecord(sub, ins,
                    RomAddress: jumpAddress.Key,
                    TargetAddress: targetAddress,
                    IsRead: false,
                    IsWrite: false,
                    IsJump: true,
                    IsBranch: false,
                    IsDirectAccess: false);
            }
        }
    }

    public IEnumerable<string> Callstacks => _callstack;

    public void Run()
    {
        Cpu.Init();

        // TODO dataTable and dataITable
        ushort startSegmentAddress = Cpu.PC;

        var returnToSubs = new Dictionary<ushort, Sub>();

        (HashSet<ushort> addresses, Sub sub) TryGetSub(ushort address, bool useReturnsToo = false)
        {
            if (useReturnsToo && returnToSubs.TryGetValue(address, out var s))
                return (s.Addresses, s);

            if (_subs.TryGetValue(address, out s))
                return (s.Addresses, s);

            var addresses = new HashSet<ushort>();
            s = new Sub(Cpu, Cpu.PC, addresses);
            _subs.Add(s.Address, s);

            return (addresses, s);
        }

        void WriteCallstack(int indentation, ushort endSegmentAddress, string mnemonic, ushort targetAddress) =>
            _callstack.Add($"{"".PadLeft(indentation)}run ${startSegmentAddress:X4}-${endSegmentAddress:X4} => {mnemonic} to {Labels.GetLabelAndMemoryAddress(targetAddress)}");

        (var addresses, var sub) = TryGetSub(Cpu.PC);

        while (_ppu.Frame <= 250)
        {
            var address = Cpu.PC;
            if (address == 0x8052) { }
            if (address >= 0x6000 && address <= 0xFFFF)
                addresses.Add(address);

            var ins = Cpu.RunNextInstruction();
            _instructionAddresses.Add(address);

            switch (ins.Opcode)
            {
                case 0x20: // JSR
                case 0x4C: // JMP Absolute
                case 0x6C: // JMP Indirect
                    if (address != Cpu.PC)
                    {
                        _jumpTable.TryAdd(address, Cpu.PC);
                        returnToSubs[(ushort)(address + 3)] = sub;

                        (addresses, sub) = TryGetSub(Cpu.PC);

                        WriteCallstack(255 - Cpu.SP - 2 + (ins.Mnemonic == "JMP" ? 2 : 0), address, ins.Mnemonic, Cpu.PC);
                        startSegmentAddress = Cpu.PC;
                    }
                    if (ins.Opcode == 0x6C)
                    {
                        if (!_jumpITable.TryGetValue(address, out var hashset))
                        {
                            hashset = [];
                            _jumpITable.TryAdd(address, hashset);
                        }
                        hashset.Add(Cpu.PC);
                    }
                    break;
                case 0x60: // "RTS"
                    (addresses, sub) = TryGetSub(Cpu.PC, useReturnsToo: true);

                    WriteCallstack(255 - Cpu.SP + 2, address, ins.Mnemonic, Cpu.PC);
                    startSegmentAddress = Cpu.PC;
                    //Jump(address, ins);
                    break;
                case 0x40: // "RTI"
                    (addresses, sub) = TryGetSub(Cpu.PC);

                    WriteCallstack(255 - Cpu.SP + 2, address, ins.Mnemonic, Cpu.PC);
                    startSegmentAddress = Cpu.PC;
                    //Jump(address, ins);
                    break;
            }

            _ppu.RunToCycle(Cpu.Cycles);

            if (_ppu.NmiRequested)
            {
                _ppu.NmiRequested = false;
                Cpu.RunNmi();

                (addresses, sub) = TryGetSub(Cpu.PC);
                startSegmentAddress = Cpu.PC;

                _callstack.Add("");
                _callstack.Add($"  * Total [F:{_ppu.Frame - 1,3}]: {_subs.Count,3} Subs, {_jumpTable.Count,3} jumps, {_jumpITable.Sum(j => j.Value.Count),3} indirect jumps *");
                _callstack.Add($"--- Nmi Frame {_ppu.Frame} ---");
                _callstack.Add("");
            }
        }
    }
}

public record Sub(CPU Cpu, ushort Address, HashSet<ushort> Addresses)
{
    public IEnumerable<Emulator.Instructions.Instruction> Instructions => Addresses.OrderBy(a => a).Select(a => Cpu.GetInstructionAt(Address));

    public override string ToString()
    {
        return $"${Address:X4} to ${Addresses.Max():X4} (Instructions: {Instructions.Count()})";
    }
}