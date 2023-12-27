using NesAsm.Emulator;

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
}
