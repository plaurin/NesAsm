using NesAsm.Emulator;

namespace NesAsm.Example;

[PostFileInclude("wrapper.s")]
public class HiloWorld : ScriptBase
{
    public HiloWorld(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
        // Read the PPUSTATUS register $2002
        LDA(0x2002);

        // Store the address $3f01 in the PPUADDR $2006 (begining of the background palette 0)
        LDAi(0x3F);
        STA(0x2006);
        LDAi(0x00);
        STA(0x2006);

        loop:

        // TODO lda palettes, x
        LDA(Palettes, X);

        // Write palette data to PPUDATA $2007
        STA(0x2007);
        INX();

        CPXi(0x20);

        if (BNE()) goto loop;
    }

    [RomData]
    private byte[] Palettes = [
        // Background palettes
        0x0F,
        0x20,
        0x21,
        0x22,
        0x0F,
        0x00,
        0x00,
        0x00,
        0x0F,
        0x00,
        0x00,
        0x00,
        0x0F,
        0x00,
        0x00,
        0x00,

        // Sprite palettes
        0x0F,
        0x20,
        0x27,
        0x31,
        0x0F,
        0x00,
        0x00,
        0x00,
        0x0F,
        0x00,
        0x00,
        0x00,
        0x0F,
        0x00,
        0x00,
        0x00,
    ];

    [CharData]
    private byte[] Characters = [
        // H
        0b_11000011,
        0b_11000011,
        0b_11000011,
        0b_11111111,
        0b_11111111,
        0b_11000011,
        0b_11000011,
        0b_11000011,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

        // I
        0b_11111111,
        0b_11111111,
        0b_00011000,
        0b_00011000,
        0b_00011000,
        0b_00011000,
        0b_11111111,
        0b_11111111,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

        // L
        0b_11000000,
        0b_11000000,
        0b_11000000,
        0b_11000000,
        0b_11000000,
        0b_11000000,
        0b_11111111,
        0b_11111111,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

        // O
        0b_00011000,
        0b_00111100,
        0b_01100110,
        0b_11000011,
        0b_11000011,
        0b_01100110,
        0b_00111100,
        0b_00011000,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

        // W
        0b_11000011,
        0b_11000011,
        0b_11000011,
        0b_11000011,
        0b_01100110,
        0b_01111110,
        0b_01100110,
        0b_01100110,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

        // R
        0b_11111000,
        0b_11111110,
        0b_11000011,
        0b_11001111,
        0b_11111000,
        0b_11001100,
        0b_11000110,
        0b_11000011,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

        // D
        0b_11111000,
        0b_11111100,
        0b_11000110,
        0b_11000011,
        0b_11000011,
        0b_11000110,
        0b_11111100,
        0b_11111000,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
    ];
}