namespace NesAsm.Emulator;

public class NesApiCSharp
{
    public static void RunOnce(Action draw, Action gameEntryPoint)
    {
        try
        {
            gameEntryPoint.Invoke();

            while (true)
            {
                draw.Invoke();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static void RunGame(Action draw, Action reset, Action? nmi = null, Action? interrupt = null)
    {
        try
        {
            // Set PPU callback for nmi
            PPUApiCSharp.SetNmiCallback(() =>
            {
                nmi?.Invoke();

                draw.Invoke();
            });

            reset.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}