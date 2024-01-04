namespace NesAsm.Analyzers.Tests;

public class UtilitiesTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("0", "0")]
    [InlineData("123", "123")]
    [InlineData("65535", "65535")]
    [InlineData("0x00", "$00")]
    [InlineData("0x1", "$1")]
    [InlineData("0xFF", "$FF")]
    [InlineData("0xA00", "$A00")]
    [InlineData("0xFFFF", "$FFFF")]
    [InlineData("0b1", "%1")]
    [InlineData("0b_101", "%101")]
    [InlineData("0b_10101010", "%10101010")]
    [InlineData("0b_1_0_1_0_1_0_1_0", "%10101010")]
    public void TestParsingNumeric(string input, string expectedOutput)
    {
        var result = Utilities.ConvertOperandToNumericText(input);
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("65536")]
    [InlineData("-1")]
    [InlineData("0xA0000")]
    [InlineData("0b010101010")]
    public void TestOutOfBoundValueShouldThrow(string invalidInput)
    {
        Assert.Throws<InvalidOperationException>(() => Utilities.ConvertOperandToNumericText(invalidInput));
    }

}
