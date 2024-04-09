using ApprovalTests;
using ApprovalTests.Reporters;
using BigGustave;
using System.Text;

namespace NesAsm.Analyzers.Tests.PngTests;

[UseReporter(typeof(VisualStudioReporter))]
public class PngUtilTests
{
    [Fact]
    public void TestReadPng()
    {
        var image = Png.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\CharTests\TestFiles\ImportChar.png"));
        TestImage(image);
    }

    [Fact]
    public void TestRead4TilesPngEvenWidth()
    {
        TestImage("4Tiles.png");
    }

    [Fact]
    public void TestRead4TilesOddWidth()
    {
        TestImage("4TilesNoEndSeparator.png");
    }

    private static void TestImage(string imageFileName)
    {
        var image = Png.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..\..\..\PngTests\TestFiles\{imageFileName}"));
        TestImage(image);
    }

    private static void TestImage(Png image)
    {
        var sb = new StringBuilder();

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Pixel pixel = image.GetPixel(x, y);

                sb.Append($"{pixel.R:X2}{pixel.G:X2}{pixel.B:X2} ");
            }

            sb.AppendLine();
        }

        Approvals.Verify(sb.ToString());
    }
}
