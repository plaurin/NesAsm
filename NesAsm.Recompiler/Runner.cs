using NesAsm.Emulator;

namespace NesAsm.Recompiler;

public class Runner
{
    private readonly CPU _cpu;

    public Runner()
    {
        var memory = new NesMemory();
        _cpu = new CPU(memory);
    }

    public void Run(IReadOnlyCollection<Subroutine> subroutines, Cart cart)
    {
        var resetSub = subroutines.FirstOrDefault(s => s.Address == cart.ResetAddress);
        var nmiSub = subroutines.FirstOrDefault(s => s.Address == cart.NmiAddress);

        if (resetSub != null)
        {
            RunSub(resetSub);
        }

        if (nmiSub != null)
        {
            RunSub(nmiSub);
        }
    }

    private void RunSub(Subroutine sub)
    {

    }
}