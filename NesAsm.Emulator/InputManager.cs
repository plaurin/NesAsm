namespace NesAsm.Emulator;

public static class InputManager
{
    private static readonly bool[] IsPress = new bool[8];
    private static readonly bool[] WasPress = new bool[8];
    private static readonly bool[] IsJustPress = new bool[8];
    private static readonly bool[] IsJustRelease = new bool[8];
    private static readonly int[] PressLength = new int[8];
    private static readonly int[] ReleaseLength = new int[8];

    private static readonly bool[] DoubleTapStart = new bool[8];
    private static readonly bool[] DoubleTap = new bool[8];
    private static readonly bool[] IsJustDoubleTap = new bool[8];
    private static int MaxFirstPress = 16;
    private static int MaxReleaseLength = 16;

    public static bool Left { get => IsPress[0]; }
    public static bool Right { get => IsPress[1]; }
    public static bool Up { get => IsPress[2]; }
    public static bool Down { get => IsPress[3]; }
    public static bool B { get => IsPress[4]; }
    public static bool A { get => IsPress[5]; }
    public static bool Select { get => IsPress[6]; }
    public static bool Start { get => IsPress[7]; }

    public static bool LeftJustPress { get => IsJustPress[0]; }
    public static bool RightJustPress { get => IsJustPress[1]; }
    public static bool UpJustPress { get => IsJustPress[2]; }
    public static bool DownJustPress { get => IsJustPress[3]; }
    public static bool BJustPress { get => IsJustPress[4]; }
    public static bool AJustPress { get => IsJustPress[5]; }
    public static bool SelectJustPress { get => IsJustPress[6]; }
    public static bool StartJustPress { get => IsJustPress[7]; }

    public static bool LeftJustRelease { get => IsJustRelease[0]; }
    public static bool RightJustRelease { get => IsJustRelease[1]; }
    public static bool UpJustRelease { get => IsJustRelease[2]; }
    public static bool DownJustRelease { get => IsJustRelease[3]; }
    public static bool BJustRelease { get => IsJustRelease[4]; }
    public static bool AJustRelease { get => IsJustRelease[5]; }
    public static bool SelectJustRelease { get => IsJustRelease[6]; }
    public static bool StartJustRelease { get => IsJustRelease[7]; }

    public static int LeftPressLength { get => PressLength[0]; }
    public static int RightPressLength { get => PressLength[1]; }
    public static int UpPressLength { get => PressLength[2]; }
    public static int DownPressLength { get => PressLength[3]; }
    public static int BPressLength { get => PressLength[4]; }
    public static int APressLength { get => PressLength[5]; }
    public static int SelectPressLength { get => PressLength[6]; }
    public static int StartPressLength { get => PressLength[7]; }

    public static int LeftReleaseLength { get => ReleaseLength[0]; }
    public static int RightReleaseLength { get => ReleaseLength[1]; }
    public static int UpReleaseLength { get => ReleaseLength[2]; }
    public static int DownReleaseLength { get => ReleaseLength[3]; }
    public static int BReleaseLength { get => ReleaseLength[4]; }
    public static int AReleaseLength { get => ReleaseLength[5]; }
    public static int SelectReleaseLength { get => ReleaseLength[6]; }
    public static int StartReleaseLength { get => ReleaseLength[7]; }

    public static bool LeftDoubleTap { get => DoubleTap[0]; }
    public static bool RightDoubleTap { get => DoubleTap[1]; }
    public static bool UpDoubleTap { get => DoubleTap[2]; }
    public static bool DownDoubleTap { get => DoubleTap[3]; }
    public static bool BDoubleTap { get => DoubleTap[4]; }
    public static bool ADoubleTap { get => DoubleTap[5]; }
    public static bool SelectDoubleTap { get => DoubleTap[6]; }
    public static bool StartDoubleTap { get => DoubleTap[7]; }

    public static bool LeftJustDoubleTap { get => IsJustDoubleTap[0]; }
    public static bool RightJustDoubleTap { get => IsJustDoubleTap[1]; }
    public static bool UpJustDoubleTap { get => IsJustDoubleTap[2]; }
    public static bool DownJustDoubleTap { get => IsJustDoubleTap[3]; }
    public static bool BJustDoubleTap { get => IsJustDoubleTap[4]; }
    public static bool AJustDoubleTap { get => IsJustDoubleTap[5]; }
    public static bool SelectJustDoubleTap { get => IsJustDoubleTap[6]; }
    public static bool StartJustDoubleTap { get => IsJustDoubleTap[7]; }

    public static void SetState(int index, bool state) => IsPress[index] = state;
    public static void SetDoubleTapLengths(int maxFirstPress, int maxReleaseLength)
    {
        MaxFirstPress = maxFirstPress;
        MaxReleaseLength = maxReleaseLength;
    }

    public static void FrameUpdate()
    {
        for (int i = 0; i < 8; i++)
        {
            IsJustPress[i] = !WasPress[i] && IsPress[i];
            IsJustRelease[i] = WasPress[i] && !IsPress[i];
            IsJustDoubleTap[i] = false;

            if (DoubleTapStart[i])
            {
                if (ReleaseLength[i] > MaxReleaseLength) DoubleTapStart[i] = false;
                if (IsJustPress[i]) { DoubleTap[i] = true; IsJustDoubleTap[i] = true; DoubleTapStart[i] = false; }
            }
            if (IsJustRelease[i] && PressLength[i] <= MaxFirstPress) { DoubleTapStart[i] = true; }
            if (DoubleTap[i] && !IsPress[i]) DoubleTap[i] = false;

            if (IsPress[i]) { PressLength[i]++; ReleaseLength[i] = 0; }
            if (!IsPress[i]) { PressLength[i] = 0; ReleaseLength[i]++; }

            WasPress[i] = IsPress[i];
        }
    }
}
