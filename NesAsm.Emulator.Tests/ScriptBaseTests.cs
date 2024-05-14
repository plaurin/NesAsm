namespace NesAsm.Emulator.Tests;
using static NesAsm.Emulator.NESEmulatorStatic;

public class ScriptBaseTests
{
    [Fact]
    public void TestBasicScriptInheritance()
    {
        ScriptOne.MainProc();

        Assert.Equal(123, NESEmulatorStatic.A);
        Assert.Equal(132, NESEmulatorStatic.X);
        Assert.Equal(144, NESEmulatorStatic.Y);
    }

    private static class ScriptOne
    {
        public static void MainProc()
        {
            LDAi(123);
            LDXi(133);
            DEX();
            LDYi(143);
            INY();
        }
    }
}
