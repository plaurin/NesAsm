namespace NesAsm.Emulator.Tests;

public class ScriptBaseTests
{
    [Fact]
    public void TestBasicScriptInheritance()
    {
        var emulator = new NESEmulator();
        var script = new ScriptOne(emulator);

        script.Main();

        Assert.Equal(124, emulator.A);
        Assert.Equal(132, emulator.X);
        Assert.Equal(144, emulator.Y);
    }

    private class ScriptOne(NESEmulator emulator) : ScriptBase(emulator)
    {
        public void Main()
        {
            LDA(123);
            INC();
            LDX(133);
            DEX();
            LDY(143);
            INY();
        }
    }
}
