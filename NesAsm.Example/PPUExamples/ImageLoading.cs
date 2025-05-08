using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.PPUExamples;

public class ImageLoading
{
    public static void Reset()
    {
        // Init;
        LoadImage(@"PPUExamples\Image1.png");

        SetSpriteData(0, 20, 20, 1, 0, false, false, false);
        SetSpriteData(1, 40, 40, 2, 1, false, false, false);
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