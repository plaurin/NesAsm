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
                draw.Invoke();

                nmi?.Invoke();
            });

            reset.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static Task RunGame(CancellationToken? cancellationToken, Action draw, Action<CancellationToken?> reset, Action? nmi = null, Action? interrupt = null)
    {
        try
        {
            // Set PPU callback for nmi
            PPUApiCSharp.SetNmiCallback(() =>
            {
                draw.Invoke();

                nmi?.Invoke();
            });

            reset.Invoke(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return Task.CompletedTask;
    }
}