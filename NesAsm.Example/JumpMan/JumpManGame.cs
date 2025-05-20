using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.JumpMan;

public class JumpManGame
{
    const int SkyTile = 0x24;
    const int BrickTile = 0x45;
    const int GroundTile = 0xB4;
    const int QuestionTile = 0x53;

    public static void Reset()
    {
        // Background palette
        SetBackgroundPaletteColors(0, 0x_22, 0x_29, 0x_1A, 0X_0F);
        SetBackgroundPaletteColors(1, 0x_0F, 0x_36, 0x_17, 0x_0F);
        SetBackgroundPaletteColors(2, 0x_0F, 0x_30, 0x_21, 0x_0F);
        SetBackgroundPaletteColors(3, 0x_0F, 0x_27, 0x_17, 0x_0F);
        
        // Init;
        LoadImage(@"JumpMan\JumpMan.png", hasTileSeparator: false, useExistingPalettes: true);

        // Full sky
        for (int j = 0; j < 30; j++)
            for (int i = 0; i < 32; i++)
            {
                SetNametableTile(0, i, j, SkyTile);
            }

        SetNametableTile(0, 0, 26, GroundTile);
        SetNametableTile(0, 1, 26, GroundTile+1);
        SetNametableTile(0, 0, 27, GroundTile+2);
        SetNametableTile(0, 1, 27, GroundTile+3);
        SetAttributeTablePalette(0, 0, 13, 3);


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