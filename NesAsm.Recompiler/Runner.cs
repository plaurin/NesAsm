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

    public IEnumerable<Subroutine> Subroutines => _subs
        .Select(s =>
        {
            var instructions = new List<Instruction>();
            int nextAddress = s.Key;
            foreach (var addr in s.Value.Addresses.OrderBy(a => a))
            {
                while (nextAddress < addr)
                {
                    // fill gap
                    var parsedIns = Parser.GetInstruction(Cpu.Cart.PrgRom, (ushort)nextAddress, false);
                    instructions.Add(parsedIns);
                    nextAddress = parsedIns.Address + parsedIns.Bytes;
                }

                var ins = Parser.GetInstruction(Cpu.Cart.PrgRom, addr, true);
                instructions.Add(ins);
                nextAddress = ins.Address + ins.Bytes;
            } 

            return new Subroutine(instructions);
        })
        .Where(s => s.Instructions.Count > 0)
        .OrderBy(s => s.Address);

    public IEnumerable<MemoryRegion> MemoryRegions
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

            result.AddRange(subs);
            return result.Where(s => s.Size > 0);
        }
    }

    public IEnumerable<MemoryAccessRecord> Reads() => GetMemoryAccess(_memory.Reads, isRead: true, isWrite: false);

    public IEnumerable<MemoryAccessRecord> Writes() => GetMemoryAccess(_memory.Writes, isRead: false, isWrite: true);

    private IEnumerable<MemoryAccessRecord> GetMemoryAccess(Dictionary<ushort, HashSet<ushort>> records, bool isRead, bool isWrite)
    {
        foreach (var record in records.OrderBy(r => r.Key))
        {
            var sourceAddress = record.Key;
            var sub = Subroutines.First(s => s.IsInSub(sourceAddress));
            var ins = Parser.GetInstruction(Cpu.Cart.PrgRom, sourceAddress);

            if (record.Value.Count == 1)
                yield return new MemoryAccessRecord(sub, ins,
                    RomAddress: sourceAddress,
                    TargetAddress: record.Value.Single(),
                    IsRead: isRead,
                    IsWrite: isWrite,
                    IsJump: false,
                    IsBranch: false,
                    IsDirectAccess: true);
            else
            {
                var first = record.Value.OrderBy(a => a).First();
                var last = record.Value.OrderBy(a => a).Last();
                yield return new MemoryAccessRecord(sub, ins,
                    RomAddress: sourceAddress,
                    TargetAddress: first,
                    IsRead: isRead,
                    IsWrite: isWrite,
                    IsJump: false,
                    IsBranch: false,
                    IsDirectAccess: false,
                    Size: last - first - 1);
            }
        }
    }

    public IEnumerable<MemoryAccessRecord> DirectJumpTableMemoryAccess()
    {
        foreach (var jumpAddress in _jumpTable)
        {
            var sub = Subroutines.First(s => s.IsInSub(jumpAddress.Key));
            var ins = Parser.GetInstruction(Cpu.Cart.PrgRom, jumpAddress.Key);
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