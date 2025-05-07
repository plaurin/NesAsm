using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.PPUExamples;

public class ImageLoading
{
    public static void Reset()
    {
        // Init;
        LoadImage(@"PPUExamples\Image1.png");

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