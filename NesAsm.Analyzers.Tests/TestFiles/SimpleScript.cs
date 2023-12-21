using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;
internal class SimpleScript : ScriptBase
{
    public SimpleScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
        LDA(25);
        INC();
    }
}
