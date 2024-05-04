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

    [Theory]
    [InlineData("C:\\Users\\pasca\\Dev\\GitHub\\NesAsm\\NesAsm.Example", "toto.cs", "Output", "C:\\Users\\pasca\\Dev\\GitHub\\NesAsm\\NesAsm.Example\\Output")]
    [InlineData("C:\\Dev", "toto.cs", "Output", "C:\\Dev\\Output")]
    [InlineData("C:\\Dev", "sub1\\toto.cs", "Output", "C:\\Dev\\Output\\sub1")]

    // No output path
    [InlineData("C:\\Dev", "toto.cs", null, "C:\\Dev")]

    // Relative node path
    [InlineData("C:\\Dev\\Sub1\\Sub2\\Sub3", "..\\..\\toto.cs", "Output", "C:\\Dev\\Sub1\\Output")]
    [InlineData("C:\\Dev\\Sub1\\Sub2\\Sub3", "..\\..\\toto.cs", null, "C:\\Dev\\Sub1")]
    [InlineData("C:\\Dev\\A\\B\\C", "..\\..\\sub1\\sub2\\toto.cs", "Output", "C:\\Dev\\A\\Output\\sub1\\sub2")]
    [InlineData("C:\\Dev\\A\\B\\C", "..\\..\\sub1\\sub2\\toto.cs", null, "C:\\Dev\\A\\sub1\\sub2")]

    // Rooted node path
    [InlineData("C:\\Dev", "C:\\some1\\some2\\toto.cs", null, "C:\\some1\\some2")]
    [InlineData("C:\\Dev", "C:\\some1\\some2\\toto.cs", "out1\\out2", "C:\\some1\\some2\\out1\\out2")]
    public void TestGetOutputFolder(string currentFolder, string nodeFilePath, string outputPath, string expectedResult)
    {
        var result = Utilities.GetOutputFolder(currentFolder, nodeFilePath, outputPath);
        Assert.Equal(expectedResult, result);
    }
}
