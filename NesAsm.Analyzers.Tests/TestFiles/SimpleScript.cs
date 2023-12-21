using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;
internal class SimpleScript : ScriptBase
{
    public SimpleScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
        // Start by loading the value 25 into the Accumulator register
        LDA(25);

        // Increment the value of the Accumulator registrer
        INC();
    }
}
