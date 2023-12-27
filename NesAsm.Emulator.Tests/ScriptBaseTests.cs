namespace NesAsm.Emulator.Tests;

public class ScriptBaseTests
{
    [Fact]
    public void TestBasicScriptInheritance()
    {
        var emulator = new NESEmulator();
        var script = new ScriptOne(emulator);

        script.Main();

        Assert.Equal(123, emulator.A);
        Assert.Equal(132, emulator.X);
        Assert.Equal(144, emulator.Y);
    }

    private class ScriptOne(NESEmulator emulator) : ScriptBase(emulator)
    {
        public void Main()
        {
            LDAi(123);
            LDXi(133);
            DEX();
            LDYi(143);
            INY();
        }
    }
}
