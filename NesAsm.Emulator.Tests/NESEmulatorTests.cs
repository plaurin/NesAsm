namespace NesAsm.Emulator.Tests
{
    public class NESEmulatorTests
    {
        [Fact]
        public void TestXRegister()
        {
            var emulator = new NESEmulator();

            Assert.Equal(0, emulator.X);

            emulator.LDX(5);
            Assert.Equal(5, emulator.X);

            emulator.INX();
            Assert.Equal(6, emulator.X);

            emulator.LDX(20);
            Assert.Equal(20, emulator.X);

            emulator.DEX();
            Assert.Equal(19, emulator.X);
        }
    }
}