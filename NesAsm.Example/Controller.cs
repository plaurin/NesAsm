using NesAsm.Emulator;

namespace NesAsm.Example;

public class Controller : ScriptBase
{
    public Controller(NESEmulator emulator) : base(emulator)
    {
    }

    private readonly ushort JOYPAD1 = 0x4016;
    private readonly ushort JOYPAD2 = 0x4017;

    public readonly byte BUTTON_A = 0b_1000_0000;
    public readonly byte BUTTON_B = 0b_0100_0000;
    public readonly byte BUTTON_SELECT = 0b_0010_0000;
    public readonly byte BUTTON_START = 0b_0001_0000;
    public readonly byte BUTTON_UP = 0b_0000_1000;
    public readonly byte BUTTON_DOWN = 0b_0000_0100;
    public readonly byte BUTTON_LEFT = 0b_0000_0010;
    public readonly byte BUTTON_RIGHT = 0b_0000_0001;

    public readonly byte Down = 0x20;
    public readonly byte Pressed = 0x21;

    public void ReadControllerOne()
    {
        // Initialize the output memory
        LDAi(1);
        STA(Down);

        // Send the latch pulse down to the 4021
        STA(JOYPAD1);
        LDAi(0);
        STA(JOYPAD1);

        // Read the buttons from the data line

        read_loop:

        LDA(JOYPAD1);
        LSR();
        ROL(Down);
        if (BCC()) goto read_loop;

        // Temp copy
        LDA(Down);
        STA(Pressed);
    }
}
