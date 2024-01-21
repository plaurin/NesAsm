using NesAsm.Emulator;

namespace NesAsm.Example;

public class ReadController : ScriptBase
{
    public ReadController(NESEmulator emulator) : base(emulator)
    {
    }

    public void ReadControllerOne()
    {
        // Initialize the output memory
        LDAi(1);
        STA(0x20);

        // Send the latch pulse down to the 4021
        STA(0x4016);
        LDAi(0);
        STA(0x4016);

        // Read the buttons from the data line

        read_loop:

        LDA(0x4016);
        LSR();
        ROL(0x20);
        if (BCC()) goto read_loop;
    }
}
