using NesAsm.Emulator;

namespace NesAsm.Recompiler;

public class Runner
{
    private readonly CPU _cpu;
    private readonly PPUInstance _ppu;

    public Runner(Cart cart)
    {
        _ppu = new PPUInstance();
        var memory = new NesMemory(cart, _ppu);
        _cpu = new CPU(memory);
    }

    public void Run(IReadOnlyCollection<Subroutine> subroutines)
    {
        var resetSub = subroutines.FirstOrDefault(s => s.Address == _cpu.Cart.ResetAddress);
        var nmiSub = subroutines.FirstOrDefault(s => s.Address == _cpu.Cart.NmiAddress);

        if (resetSub != null)
        {
            RunSub(resetSub);
        }

        if (nmiSub != null)
        {
            RunSub(nmiSub);
        }

        _cpu.Init();

        while(true)
        {
            _cpu.RunNextInstruction();
            _ppu.RunToCycle(_cpu.Cycles);
        }
    }

    private void RunSub(Subroutine sub)
    {

    }
}