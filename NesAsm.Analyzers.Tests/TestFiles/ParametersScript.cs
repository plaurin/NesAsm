using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class ParametersScript : ScriptBase
{
    public ParametersScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
        ProcB(25, 1000, true);
    }

    public void ProcB(byte a, ushort b, bool c)
    {
        LDAa(a);
        STA(0x40);

        //LDXa(b);
        //STA(0x41);

        LDYa(c);
        STA(0x42);
    }
}
