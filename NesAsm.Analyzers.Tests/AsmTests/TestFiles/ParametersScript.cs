using NesAsm.Emulator;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class ParametersScript : NesScript
{
    public static void Start()
    {
        ProcB(25, 1000, true);
        ProcB(0x20, 0x2000, true);
    }

    public static void ProcB(byte a, ushort b, bool c)
    {
        LDAa(a);
        STA(0x40);

        //LDXa(b);
        //STA(0x41);

        LDYa(c);
        STA(0x42);
    }

    public static void InvalidParamType(long l)
    {
        Proc(l);
    }

    public static void InvalidOpCodeUsingParameter(byte a)
    {
        STA(a);
    }

    // Maybe not used anymore
    // Only to force NA0004 in InvalidParamType
    private static void Proc(long l)
    {
    }
}
