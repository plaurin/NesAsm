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
        .Select(s => new Subroutine(s.Value.Addresses.Select(a => Parser.GetInstruction(Cpu.Cart.PrgRom, a)).ToList()))
        .OrderBy(s => s.Address);

    public IEnumerable<string> Callstacks => _callstack;

    public void Run()
    {
        Cpu.Init();

        // TODO dataTable and dataITable
        ushort startSegmentAddress = Cpu.PC;

        var returnToSubs = new Dictionary<ushort, Sub>();

        (HashSet<ushort> addresses, Sub sub) TryGetSub(ushort address)
        {
            if (returnToSubs.TryGetValue(address, out var s))
                return (s.Addresses, s);

            if (_subs.TryGetValue(address, out s))
                return (s.Addresses, s);

            var addresses = new HashSet<ushort>();
            s = new Sub(Cpu, Cpu.PC, addresses);
            _subs.Add(s.Address, s);

            return (addresses, s);
        }

        void WriteCallstack(int indentation, ushort endSegmentAddress, string mnemonic, ushort targetAddress) =>
            _callstack.Add($"{"".PadLeft(indentation)}run ${startSegmentAddress:X4}-${endSegmentAddress:X4} => {mnemonic} to ${targetAddress:X4}");

        (var addresses, var sub) = TryGetSub(Cpu.PC);

        while (_ppu.Frame <= 5)
        {
            var address = Cpu.PC;
            if (address == 0x8E28) { }
            addresses.Add(address);

            var ins = Cpu.RunNextInstruction();

            switch (ins.Opcode)
            {
                case 0x20: // JSR
                case 0x4C: // JMP Absolute
                    if (address != Cpu.PC)
                    {
                        _jumpTable.TryAdd(address, Cpu.PC);
                        returnToSubs[(ushort)(address + 3)] = sub;

                        (addresses, sub) = TryGetSub(Cpu.PC);

                        WriteCallstack(255 - Cpu.SP - 2 + (ins.Mnemonic == "JMP" ? 2 : 0), address, ins.Mnemonic, Cpu.PC);
                        startSegmentAddress = Cpu.PC;
                    }
                    break;
                case 0x6C: // JMP Indirect
                    break;
                case 0x60: // "RTS"
                case 0x40: // "RTI"
                    (addresses, sub) = TryGetSub(Cpu.PC);

                    WriteCallstack(255 - Cpu.SP + 2, address, ins.Mnemonic, Cpu.PC);
                    startSegmentAddress = Cpu.PC;
                    break;
            }

            _ppu.RunToCycle(Cpu.Cycles);
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