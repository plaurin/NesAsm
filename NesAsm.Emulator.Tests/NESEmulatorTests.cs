using static NesAsm.Emulator.ZeroPageAddress;

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

    [Fact]
    public void TestZeroPageLoadAndStoreInstructions()
    {
        var zpa = _01;
        Assert.Equal(1, zpa.Value);

        _emulator.LDA(42);
        _emulator.STA(_01);
        Assert.Equal(42, _emulator.Memory[1]);

        _emulator.LDX(35);
        _emulator.STX(_02);
        Assert.Equal(35, _emulator.Memory[2]);

        _emulator.LDY(68);
        _emulator.STY(_03);
        Assert.Equal(68, _emulator.Memory[3]);

        _emulator.LDA(_03);
        Assert.Equal(68, _emulator.A);

        _emulator.LDX(_01);
        Assert.Equal(42, _emulator.X);

        _emulator.LDY(_02);
        Assert.Equal(35, _emulator.Y);
    }

    [Fact]
    public void TestAbsoluteModeLoadAndStoreInstructions()
    {
        _emulator.LDA(76);
        _emulator.STA(0x1000);
        Assert.Equal(76, _emulator.Memory[0x1000]);

        _emulator.LDX(25);
        _emulator.STX(0x2000);
        Assert.Equal(25, _emulator.Memory[0x2000]);

        _emulator.LDY(94);
        _emulator.STY(0x3000);
        Assert.Equal(94, _emulator.Memory[0x3000]);

        _emulator.LDA(0x3000);
        Assert.Equal(94, _emulator.A);

        _emulator.LDX(0x1000);
        Assert.Equal(76, _emulator.X);

        _emulator.LDY(0x2000);
        Assert.Equal(25, _emulator.Y);
    }
}