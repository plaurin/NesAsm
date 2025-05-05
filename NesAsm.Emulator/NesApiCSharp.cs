namespace NesAsm.Emulator;

public class NesApiCSharp
{
    public static void RunOnce(Action draw, Action gameEntryPoint)
    {
        gameEntryPoint.Invoke();

        while (true)
        {
            draw.Invoke();
        }
    }

    public static void RunGame(Action draw, Action reset, Action? nmi = null, Action? interrupt = null)
    {
        // Set PPU callback for nmi
        PPUApiCSharp.SetNmiCallback(() =>
        {
            nmi?.Invoke();

            draw.Invoke();
        });

        reset.Invoke();
    }
}