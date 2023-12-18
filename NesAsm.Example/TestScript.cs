using NesAsm.Emulator;

namespace NesAsm.Example;

public class TestScript : ScriptBase
{
    public TestScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Start()
    {
        LDA(10);
    }
}
