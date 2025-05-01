using PPU3 = NesAsm.Emulator.PPU;

namespace NesAsm.Example.PPUExamples;

public class BackgroundExemple
{
    public static void PaletteExemple()
    {
        // Background palette
        PPU3.SetBackgroundPaletteColors(0, 0x_01, 0x_15, 0x_2A, 0X_23);
        PPU3.SetBackgroundPaletteColors(1, 0x_11, 0x_12, 0x_13, 0x_14);
        PPU3.SetBackgroundPaletteColors(2, 0x_21, 0x_22, 0x_23, 0x_24);
        PPU3.SetBackgroundPaletteColors(3, 0x_31, 0x_32, 0x_33, 0x_34);

        // Tile 0
        PPU3.SetPatternTablePixel(0, 0, 0, 1);
        PPU3.SetPatternTablePixel(0, 1, 1, 2);
        PPU3.SetPatternTablePixel(0, 2, 2, 3);

        // Tile 1
        PPU3.SetPatternTablePixel(0, 10, 0, 3);
        PPU3.SetPatternTablePixel(0, 11, 0, 2);
        PPU3.SetPatternTablePixel(0, 12, 0, 1);

        // Tile 2
        PPU3.SetPatternTablePixel(0, 20, 0, 2);
        PPU3.SetPatternTablePixel(0, 21, 2, 2);
        PPU3.SetPatternTablePixel(0, 22, 4, 2);

        // Tile 3
        PPU3.SetPatternTablePixel(0, 26, 0, 3);
        PPU3.SetPatternTablePixel(0, 27, 3, 2);
        PPU3.SetPatternTablePixel(0, 28, 6, 3);

        // Nametable
        PPU3.SetNametableTile(0, 1, 1, 1);
        PPU3.SetNametableTile(0, 2, 2, 2);
        PPU3.SetNametableTile(0, 3, 3, 3);

        // Attribute table
        PPU3.SetAttributeTablePalette(0, 3, 0, 1);
        PPU3.SetAttributeTablePalette(0, 4, 4, 2);
        PPU3.SetAttributeTablePalette(0, 0, 4, 3);

    }
}
