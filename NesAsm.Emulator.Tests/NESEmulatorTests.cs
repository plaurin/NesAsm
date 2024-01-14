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

        _emulator.LDAi(28);
        Assert.Equal(28, _emulator.A);

        _emulator.LDAi(255);
        Assert.Equal(255, _emulator.A);
    }

    [Fact]
    public void TestXRegister()
    {
        Assert.Equal(0, _emulator.X);

        _emulator.LDXi(5);
        Assert.Equal(5, _emulator.X);

        _emulator.INX();
        Assert.Equal(6, _emulator.X);

        _emulator.LDXi(20);
        Assert.Equal(20, _emulator.X);

        _emulator.DEX();
        Assert.Equal(19, _emulator.X);
    }

    [Fact]
    public void TestYRegister()
    {
        Assert.Equal(0, _emulator.Y);

        _emulator.LDYi(12);
        Assert.Equal(12, _emulator.Y);

        _emulator.INY();
        Assert.Equal(13, _emulator.Y);

        _emulator.LDYi(41);
        Assert.Equal(41, _emulator.Y);

        _emulator.DEY();
        Assert.Equal(40, _emulator.Y);
    }

    [Fact]
    public void TestTransfertOpCodes()
    {
        _emulator.LDAi(10);
        _emulator.LDXi(20);
        _emulator.LDYi(30);

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
        _emulator.LDAi(42);
        _emulator.STA(01);
        Assert.Equal(42, _emulator.Memory[1]);

        _emulator.LDXi(35);
        _emulator.STX(02);
        Assert.Equal(35, _emulator.Memory[2]);

        _emulator.LDYi(68);
        _emulator.STY(03);
        Assert.Equal(68, _emulator.Memory[3]);

        _emulator.LDA(03);
        Assert.Equal(68, _emulator.A);

        _emulator.LDX(01);
        Assert.Equal(42, _emulator.X);

        _emulator.LDY(02);
        Assert.Equal(35, _emulator.Y);
    }

    [Fact]
    public void TestAbsoluteModeLoadAndStoreInstructions()
    {
        _emulator.LDAi(76);
        _emulator.STA(0x1000);
        Assert.Equal(76, _emulator.Memory[0x1000]);

        _emulator.LDXi(25);
        _emulator.STX(0x2000);
        Assert.Equal(25, _emulator.Memory[0x2000]);

        _emulator.LDYi(94);
        _emulator.STY(0x3000);
        Assert.Equal(94, _emulator.Memory[0x3000]);

        _emulator.LDA(0x3000);
        Assert.Equal(94, _emulator.A);

        _emulator.LDX(0x1000);
        Assert.Equal(76, _emulator.X);

        _emulator.LDY(0x2000);
        Assert.Equal(25, _emulator.Y);
    }

    [Theory]
    [InlineData("A", 10, 5, true, false, false)]
    [InlineData("A", 10, 15, false, false, true)]
    [InlineData("A", 10, 10, true, true, false)]
    [InlineData("X", 20, 5, true, false, false)]
    [InlineData("X", 20, 50, false, false, true)]
    [InlineData("X", 20, 20, true, true, false)]
    [InlineData("Y", 30, 29, true, false, false)]
    [InlineData("Y", 30, 31, false, false, true)]
    [InlineData("Y", 30, 30, true, true, false)]
    public void TestImmediateCompareInstructions(string register, byte registerValue, byte compareValue, bool expectedCarry, bool expectedZero, bool expectedNegative)
    {
        switch (register)
        {
            case "A": _emulator.LDAi(registerValue); break;
            case "X": _emulator.LDXi(registerValue); break;
            case "Y": _emulator.LDYi(registerValue); break;
        };

        switch (register)
        {
            case "A": _emulator.CMPi(compareValue); break;
            case "X": _emulator.CPXi(compareValue); break;
            case "Y": _emulator.CPYi(compareValue); break;
        };
        
        Assert.Equal(expectedCarry, _emulator.Carry);
        Assert.Equal(expectedZero, _emulator.Zero);
        Assert.Equal(expectedNegative, _emulator.Negative);
    }

    [Theory]
    [InlineData("A", 10, 5, true, false, false)]
    [InlineData("A", 10, 15, false, false, true)]
    [InlineData("A", 10, 10, true, true, false)]
    [InlineData("X", 20, 5, true, false, false)]
    [InlineData("X", 20, 50, false, false, true)]
    [InlineData("X", 20, 20, true, true, false)]
    [InlineData("Y", 30, 29, true, false, false)]
    [InlineData("Y", 30, 31, false, false, true)]
    [InlineData("Y", 30, 30, true, true, false)]
    public void TestMemoryCompareInstructions(string register, byte registerValue, byte compareValue, bool expectedCarry, bool expectedZero, bool expectedNegative)
    {
        _emulator.LDAi(compareValue);
        _emulator.STA(28);
        _emulator.LDAi(0);

        switch (register)
        {
            case "A": _emulator.LDAi(registerValue); break;
            case "X": _emulator.LDXi(registerValue); break;
            case "Y": _emulator.LDYi(registerValue); break;
        };

        switch (register)
        {
            case "A": _emulator.CMP(28); break;
            case "X": _emulator.CPX(28); break;
            case "Y": _emulator.CPY(28); break;
        };
        
        Assert.Equal(expectedCarry, _emulator.Carry);
        Assert.Equal(expectedZero, _emulator.Zero);
        Assert.Equal(expectedNegative, _emulator.Negative);
    }

    [Fact]
    public void TestZeroFlagBranchInstructions()
    {
        _emulator.LDAi(10);
        _emulator.CMPi(10);

        if (_emulator.BEQ()) goto skip;

        _emulator.LDAi(99);

        skip:
        Assert.Equal(10, _emulator.A);

        _emulator.CMPi(15);

        if (_emulator.BNE()) goto skip2;

        _emulator.LDAi(98);

        skip2:
        Assert.Equal(10, _emulator.A);
    }

    private readonly byte[] _absoluteIndexedData = [0, 1, 2, 3, 4, 5];
    [Fact]
    public void TestAbsoluteIndexedLoadInstructions()
    {
        _emulator.LDXi(3);
        _emulator.LDA(_absoluteIndexedData, AddressingRegister.X);
        Assert.Equal(3, _emulator.A);

        _emulator.LDYi(1);
        _emulator.LDA(_absoluteIndexedData, AddressingRegister.Y);
        Assert.Equal(1, _emulator.A);

        _emulator.LDXi(0);
        _emulator.LDA(_absoluteIndexedData, AddressingRegister.X);
        Assert.Equal(0, _emulator.A);

        _emulator.LDYi(5);
        _emulator.LDA(_absoluteIndexedData, AddressingRegister.Y);
        Assert.Equal(5, _emulator.A);
    }
}