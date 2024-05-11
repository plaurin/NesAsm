using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

[Script]
internal class ParametersScript : FileBasedReference
{
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

    public void InvalidParamType(long l)
    {
        LDAa(l);
    }

    public void InvalidOpCodeUsingParameter(byte a)
    {
        STA(a);
    }

    // Only to force NA0004 in InvalidParamType
    private void LDAa(long l)
    {
    }
}
