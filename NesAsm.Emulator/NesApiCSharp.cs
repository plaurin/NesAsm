namespace NesAsm.Emulator;

public class NesApiCSharp
{
    public static void RunGame(Action gameEntryPoint)
    {
        gameEntryPoint.Invoke();
    }
}