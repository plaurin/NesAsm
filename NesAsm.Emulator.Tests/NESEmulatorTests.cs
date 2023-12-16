namespace NesAsm.Emulator.Tests;

public class NESEmulatorTests
{
    private readonly NESEmulator _emulator;

    public NESEmulatorTests()
    {
        _emulator = new NESEmulator();
    }

    [Fact]
    public void TestARegister()
    {

        Assert.Equal(0, _emulator.A);

        _emulator.LDA(28);
        Assert.Equal(28, _emulator.A);

        _emulator.INC();
        Assert.Equal(29, _emulator.A);

        _emulator.LDA(255);
        Assert.Equal(255, _emulator.A);

        _emulator.DEC();
        Assert.Equal(254, _emulator.A);
    }

    [Fact]
    public void TestXRegister()
    {
        Assert.Equal(0, _emulator.X);

        _emulator.LDX(5);
        Assert.Equal(5, _emulator.X);

        _emulator.INX();
        Assert.Equal(6, _emulator.X);

        _emulator.LDX(20);
        Assert.Equal(20, _emulator.X);

        _emulator.DEX();
        Assert.Equal(19, _emulator.X);
    }

    [Fact]
    public void TestYRegister()
    {
        Assert.Equal(0, _emulator.Y);

        _emulator.LDY(12);
        Assert.Equal(12, _emulator.Y);

        _emulator.INY();
        Assert.Equal(13, _emulator.Y);

        _emulator.LDY(41);
        Assert.Equal(41, _emulator.Y);

        _emulator.DEY();
        Assert.Equal(40, _emulator.Y);
    }

    [Fact]
    public void TestTransfertOpCodes()
    {
        _emulator.LDA(10);
        _emulator.LDX(20);
        _emulator.LDY(30);

        Assert.Equal(10, _emulator.A);
        Assert.Equal(20, _emulator.X);
        Assert.Equal(30, _emulator.Y);

        _emulator.TAX();
        Assert.Equal(10, _emulator.A);
        Assert.Equal(10, _emulator.X);
        Assert.Equal(30, _emulator.Y);

        _emulator.TYA();
        Assert.Equal(30, _emulator.A);
        Assert.Equal(10, _emulator.X);
        Assert.Equal(30, _emulator.Y);

        _emulator.TXA();
        Assert.Equal(10, _emulator.A);
        Assert.Equal(10, _emulator.X);
        Assert.Equal(30, _emulator.Y);

        _emulator.TAY();
        Assert.Equal(10, _emulator.A);
        Assert.Equal(10, _emulator.X);
        Assert.Equal(10, _emulator.Y);
    }
}