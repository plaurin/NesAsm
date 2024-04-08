using ApprovalTests;
using ApprovalTests.Reporters;
using BigGustave;
using System.Text;

namespace NesAsm.Analyzers.Tests;

[UseReporter(typeof(VisualStudioReporter))]
public class PngUtilTests
{
    [Fact]
    public void TestReadPng()
    {
        var image = Png.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestFiles\ImportChar.png"));
        TestImage(image);
    }

    [Fact]
    public void TestRead4TilesPngEvenWidth()
    {
        var image = Png.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestFiles\Png\4Tiles.png"));
        TestImage(image);
    }

    [Fact]
    public void TestRead4TilesOddWidth()
    {
        var image = Png.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestFiles\Png\4TilesNoEndSeparator.png"));
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
