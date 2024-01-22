using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Example;

[PostFileInclude("wrapper-no-nmi.s")]
public class HiloWorld : ScriptBase
{
    public HiloWorld(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
        // Read the PPUSTATUS register $2002
        LDA(PPUSTATUS);

        // Store the address $3f01 in the PPUADDR $2006 (begining of the background palette 0)
        LDAi(0x3F);
        STA(PPUADDR);
        LDAi(0x00);
        STA(PPUADDR);

        loop:

        LDA(Palettes, X);

        // Write palette data to PPUDATA $2007
        STA(PPUDATA);
        INX();

        CPXi(0x20);

        if (BNE()) goto loop;
    }

    public void Nmi()
    {
        LDXi(0x00);
        STX(0x2003);

        spriteLoop:

        LDA(HiloWorldSprites, X);
        STA(0x2004);
        INX();

        CPXi(44);
        if (BNE()) goto spriteLoop;
    }

    [RomData]
    private byte[] HiloWorldSprites = [
        // Empty sprites
        0, 0, 0, 0,
        0, 0, 0, 0,

        // H
        30,  // Y position - Top
        1,   // Tile 1
        0,   // Palette 4
        30,  // X position - Left

        // I
        30,  // Y position - Top
        2,   // Tile 2
        0,   // Palette 4
        39,  // X position - Left

        // L
        30,  // Y position - Top
        3,   // Tile 3
        0,   // Palette 4
        48,  // X position - Left

        // O
        30,  // Y position - Top
        4,   // Tile 4
        0,   // Palette 4
        57,  // X position - Left

        // W
        40,  // Y position - Top
        5,   // Tile 5
        0,   // Palette 4
        75,  // X position - Left

        // O
        40,  // Y position - Top
        4,   // Tile 4
        0,   // Palette 4
        84,  // X position - Left

        // R
        40,  // Y position - Top
        6,   // Tile 6
        0,   // Palette 4
        93,  // X position - Left

        // L
        40,  // Y position - Top
        3,   // Tile 3
        0,   // Palette 4
        102,  // X position - Left

        // D
        40,  // Y position - Top
        7,   // Tile 7
        0,   // Palette 4
        111,  // X position - Left
    ];

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
        // First tile is empty
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,

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