namespace NesAsm.Emulator.Tests;

public class PPUTests
{
    [Fact]
    public Task Test()
    {
        PPUApiCSharp.SetBackgroundPaletteColors(0, 1, 2, 3, 4);
        PPUApiCSharp.SetBackgroundPaletteColors(1, 11, 12, 13, 14);
        PPUApiCSharp.SetBackgroundPaletteColors(2, 21, 22, 23, 24);
        PPUApiCSharp.SetBackgroundPaletteColors(3, 31, 32, 33, 34);

        PPUApiCSharp.SetPatternTablePixel(0, 0, 0, 1);
        PPUApiCSharp.SetPatternTablePixel(0, 1, 1, 2);
        PPUApiCSharp.SetPatternTablePixel(0, 2, 2, 3);

        var screen = PPU.DrawScreen();
        var stream = new MemoryStream(screen);

        return Verify(stream);
    }
}
