namespace NesAsm.Emulator.Tests
{
    public class NESEmulatorTests
    {
        [Fact]
        public void TestARegister()
        {
            var emulator = new NESEmulator();

            Assert.Equal(0, emulator.A);

            emulator.LDA(28);
            Assert.Equal(28, emulator.A);

            emulator.INC();
            Assert.Equal(29, emulator.A);

            emulator.LDA(255);
            Assert.Equal(255, emulator.A);

            emulator.DEC();
            Assert.Equal(254, emulator.A);
        }

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

        [Fact]
        public void TestYRegister()
        {
            var emulator = new NESEmulator();

            Assert.Equal(0, emulator.Y);

            emulator.LDY(12);
            Assert.Equal(12, emulator.Y);

            emulator.INY();
            Assert.Equal(13, emulator.Y);

            emulator.LDY(41);
            Assert.Equal(41, emulator.Y);

            emulator.DEY();
            Assert.Equal(40, emulator.Y);
        }
    }
}