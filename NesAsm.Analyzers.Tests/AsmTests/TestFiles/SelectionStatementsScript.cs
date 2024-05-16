using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

[PostFileInclude("wrapper.s")]
internal class SelectionStatementsScript : NesScript
{
    public static void Start()
    {
        LDXi(12);
        LDYi(12);

        if (X == 12)
        {
            LDAi(1);
        }

        if (Y > 15)
        {
            LDAi(2);
        }

        if ((A & 0b_0000_0001) != 0)
        {
            LDAi(3);
        }
    }

    public static void InvalidIf()
    {
    }

}
