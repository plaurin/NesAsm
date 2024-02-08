using NesAsm.Emulator;

namespace NesAsm.Example;

public class Controller : ScriptBase
{
    public Controller(NESEmulator emulator) : base(emulator)
    {
    }

    private static readonly ushort JOYPAD1 = 0x4016;
    private static readonly ushort JOYPAD2 = 0x4017;

    public static readonly byte BUTTON_A = 0b_1000_0000;
    public static readonly byte BUTTON_B = 0b_0100_0000;
    public static readonly byte BUTTON_SELECT = 0b_0010_0000;
    public static readonly byte BUTTON_START = 0b_0001_0000;
    public static readonly byte BUTTON_UP = 0b_0000_1000;
    public static readonly byte BUTTON_DOWN = 0b_0000_0100;
    public static readonly byte BUTTON_LEFT = 0b_0000_0010;
    public static readonly byte BUTTON_RIGHT = 0b_0000_0001;

    /// <summary>
    /// 0x20 States for controller 1 down buttons
    /// </summary>
    public static readonly byte Down1 = 0x20;

    /// <summary>
    /// 0x21 States for controller 1 just pressed buttons (TODO)
    /// </summary>
    public static readonly byte Pressed1 = 0x21;

    public void ReadControllerOne()
    {
        // Initialize the output memory
        LDAi(1);
        STA(Down1);

        // Send the latch pulse down to the 4021
        STA(JOYPAD1);
        LDAi(0);
        STA(JOYPAD1);

        // Read the buttons from the data line

        read_loop:

        LDA(JOYPAD1);
        LSR();
        ROL(Down1);
        if (BCC()) goto read_loop;

        // Temp copy
        LDA(Down1);
        STA(Pressed1);
    }
}
