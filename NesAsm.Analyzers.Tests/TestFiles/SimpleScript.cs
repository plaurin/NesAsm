using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.TestFiles;

[PostFileInclude("wrapper.s")]
internal class SimpleScript : ScriptBase
{
    public SimpleScript(NESEmulator emulator) : base(emulator)
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
