using NesAsm.Emulator;

namespace NesAsm.Recompiler;

public class Runner
{
    private readonly PPUInstance _ppu;
    private readonly Dictionary<ushort, Sub> _subs = [];
    private readonly Dictionary<ushort, ushort>  _jumpTable = [];
    private readonly Dictionary<ushort, HashSet<ushort>> _jumpITable = [];
    private readonly List<string> _callstack = [];

    public Runner(Cart cart)
    {
        _ppu = new PPUInstance();
        var memory = new NesMemory(cart, _ppu);
        Cpu = new CPU(memory);
    }

    public CPU Cpu { get; init; }

    public IEnumerable<Subroutine> Subroutines => _subs
        .Select(s => new Subroutine(s.Value.Addresses.OrderBy(a => a).Select(a => Parser.GetInstruction(Cpu.Cart.PrgRom, a)).ToList()))
        .OrderBy(s => s.Address);

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

        (HashSet<ushort> addresses, Sub sub) Jump(ushort address, Emulator.Instructions.Instruction ins)
        {
            WriteCallstack(255 - Cpu.SP + 2, address, ins.Mnemonic, Cpu.PC);
            startSegmentAddress = Cpu.PC;

            return TryGetSub(Cpu.PC);
        }

        (var addresses, var sub) = TryGetSub(Cpu.PC);

        while (_ppu.Frame <= 15)
        {
            var address = Cpu.PC;
            if (address == 0x8052) { }
            addresses.Add(address);

            var ins = Cpu.RunNextInstruction();

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