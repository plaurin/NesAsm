using NesAsm.Emulator;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Example;

public class Controller : NesScript
{
    private const ushort JOYPAD1 = 0x4016;
    private const ushort JOYPAD2 = 0x4017;

    public const byte BUTTON_A = 0b_1000_0000;
    public const byte BUTTON_B = 0b_0100_0000;
    public const byte BUTTON_SELECT = 0b_0010_0000;
    public const byte BUTTON_START = 0b_0001_0000;
    public const byte BUTTON_UP = 0b_0000_1000;
    public const byte BUTTON_DOWN = 0b_0000_0100;
    public const byte BUTTON_LEFT = 0b_0000_0010;
    public const byte BUTTON_RIGHT = 0b_0000_0001;

    /// <summary>
    /// 0x20 States for controller 1 down buttons
    /// </summary>
    public const byte Down1 = 0x20;

    /// <summary>
    /// 0x21 States for controller 1 just pressed buttons (TODO)
    /// </summary>
    public const byte Pressed1 = 0x21;

    public static void ReadControllerOne()
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
