using BigGustave;

namespace NesAsm.Analyzers.Tests;

public class PngUtilTests
{
    [Fact]
    public void TestReadPng()
    {
        using (var stream = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\TestFiles\ImportChar.png")))
        {
            Png image = Png.Open(stream);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Pixel pixel = image.GetPixel(x, y);

                    if (pixel.R != 0)
                    {
                        Console.WriteLine($"({x}, {y}): {pixel}");
                    }
                }
            }
        }
    }
}
