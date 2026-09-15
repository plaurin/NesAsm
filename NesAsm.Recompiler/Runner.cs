using NesAsm.Emulator;

namespace NesAsm.Recompiler;

public class Runner
{
    private readonly CPU _cpu;

    public Runner(Cart cart)
    {
        var memory = new NesMemory(cart);
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
        }
    }

    private void RunSub(Subroutine sub)
    {

    }
}