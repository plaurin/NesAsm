using ApprovalTests.Reporters;

namespace NesAsm.Analyzers.Tests.CharTests;

[UseReporter(typeof(VisualStudioReporter))]
public class CharClassVisitorTests
{
    [Fact]
    public void TestImportCharScript()
    {
        TestScript("ImportCharDefinition.cs");
    }

    [Fact]
    public void TestInvalidSizeCharScript()
    {
        TestScript("InvalidSize.cs");
    }

    [Fact]
    public void TestMoreThanFourPalettesCharScript()
    {
        TestScript("MoreThanFourPalettes.cs");
    }

    [Fact]
    public void TestMoreThanThreeNonDefaultColorCharScript()
    {
        TestScript("MoreThanThreeNonDefaultColor.cs");
    }

    [Fact]
    public void TestColorMismatchCharScript()
    {
        TestScript("ColorMismatch.cs");
    }

    private static void TestScript(params string[] filenames)
    {
        var generator = new CharGenerator();

        var sourcePaths = filenames.Select(filename => $@"..\..\..\CharTests\TestFiles\{filename}").ToArray();

        GeneratorTestUtilities.TestGenerator(generator, sourcePaths);
    }
}