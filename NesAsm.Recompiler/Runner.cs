using NesAsm.Emulator;

namespace NesAsm.Recompiler;

public class Runner
{
    private readonly PPUInstance _ppu;
    private Dictionary<ushort, Sub> _subs;

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

    public void Run()
    {
        Cpu.Init();

        _subs = new Dictionary<ushort, Sub>();
        var subStack = new Stack<Sub>();
        var jumpTable = new Dictionary<ushort, ushort>();
        var jumpITable = new Dictionary<ushort, HashSet<ushort>>();
        // dataTable and dataITable

        //var addresses = new HashSet<ushort>();
        //var sub = new Sub(Cpu, Cpu.PC, addresses);
        //subs.Add(sub.Address, sub);

        (HashSet<ushort> addresses, Sub sub) TryGetSub(ushort address)
        {
            if (_subs.TryGetValue(address, out var s))
                return (s.Addresses, s);

            var addresses = new HashSet<ushort>();
            s = new Sub(Cpu, Cpu.PC, addresses);
            _subs.Add(s.Address, s);
            return (addresses, s);
        }

        (var addresses, var sub) = TryGetSub(Cpu.PC);

        while (_ppu.Frame <= 5)
        {
            var address = Cpu.PC;
            addresses.Add(address);

            var ins = Cpu.RunNextInstruction();

            switch (ins.Opcode)
            {
                case 0x20: // JSR
                case 0x4C: // JMP Absolute
                    if (address != Cpu.PC)
                    {
                        jumpTable.TryAdd(address, Cpu.PC);
                        subStack.Push(sub);

                        (addresses, sub) = TryGetSub(Cpu.PC);
                    }

                    //addresses = [];
                    //sub = new Sub(Cpu, Cpu.PC, addresses);
                    //subs.Add(sub.Address, sub);
                    // TODO for Indirect
                    break;
                case 0x6C: // JMP Indirect
                    break;
                case 0x60: // "RTS"
                case 0x40: // "RTI"
                    sub = subStack.Pop();
                    addresses = sub.Addresses;
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