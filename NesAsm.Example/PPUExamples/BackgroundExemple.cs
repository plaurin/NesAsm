using PPU3 = NesAsm.Emulator.PPU;

namespace NesAsm.Example.PPUExamples;

public class BackgroundExemple
{
    public static void PaletteExemple()
    {
        PPU3.SetBackgroundPaletteColors(0, 1, 2, 3, 4);
        PPU3.SetBackgroundPaletteColors(1, 11, 12, 13, 14);
        PPU3.SetBackgroundPaletteColors(2, 21, 22, 23, 24);
        PPU3.SetBackgroundPaletteColors(3, 31, 32, 33, 34);

        PPU3.SetPatternTablePixel(0, 0, 0, 1);
        PPU3.SetPatternTablePixel(0, 1, 1, 2);
        PPU3.SetPatternTablePixel(0, 2, 2, 3);
    }
}
