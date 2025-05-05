using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.PPUExamples;

public class GameLoopExemple
{
    private static int i;

    public static void Reset()
    {
        // Init
        
        // Background palette
        SetBackgroundPaletteColors(0, 0x_01, 0x_15, 0x_2A, 0X_23);

        while (true)
        {
            MainLoop();
            WaitForVBlank();
        }
    }

    public static void MainLoop()
    {
        i++;
        if (i > 1000)
        {
            i = 0;
        }
    }

    public static void Nmi()
    {
        // Update Background palette
        SetBackgroundPaletteColors(0, 0x_02, 0x_15, 0x_2A, 0X_23);
    }
}
