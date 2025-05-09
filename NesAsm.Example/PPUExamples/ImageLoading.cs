using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.PPUExamples;

public class ImageLoading
{
    public static void Reset()
    {
        // Init;
        LoadImage(@"PPUExamples\Image1.png");


        // Nametable
        byte tileIndex = 1;
        for (int j = 0; j < 30; j++)
            for (int i = 0; i < 32; i++)
            {
                SetNametableTile(0, i, j, tileIndex++);
                if (tileIndex > 3) tileIndex = 1;
            }

        // Attribute table
        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 15; j++)
            {
                SetAttributeTablePalette(0, i, j, 3);
            }

        SetAttributeTablePalette(0, 13, 14, 1);
        SetAttributeTablePalette(0, 14, 14, 2);

        // Sprites
        SetNametableTile(0, 1, 1, 0);

        SetSpriteData(0, 20, 20, 1, 0, false, false, false);
        SetSpriteData(1, 8, 8, 2, 1, false, false, false);
        SetSpriteData(2, 60, 60, 3, 2, false, false, false);
        SetSpriteData(3, 80, 80, 4, 3, false, false, false);

        while (true)
        {
            MainLoop();
            WaitForVBlank();
        }
    }

    private static void MainLoop()
    {
    }

    public static void Nmi()
    {
    }
}