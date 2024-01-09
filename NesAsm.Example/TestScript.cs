using NesAsm.Emulator;

namespace NesAsm.Example;

public class TestScript : ScriptBase
{
    public TestScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Start()
    {
        LDAi(10);
        LDXi(0xFF);
    }

    public void Stop(byte l)
    {
    }
}
