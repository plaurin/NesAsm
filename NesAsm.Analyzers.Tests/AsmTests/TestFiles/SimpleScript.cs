using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

[Script]
[PostFileInclude("wrapper.s")]
internal class SimpleScript
{
    public SimpleScript()
    {
    }

    public void Main()
    {
        // Start by loading the value 25 into the X register
        LDXi(25);

        // Increment the value of the X registrer
        INX();
    }

    public void InvalidParsing()
    {
        LDAa(0b_101010101010);
        
        LDAa(22);
    }

    // Only to force NA0005 in InvalidParsing
    private void LDAa(long l)
    {
    }
}
