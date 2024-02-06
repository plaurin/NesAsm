using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Example;

[HeaderSegment()]
[VectorsSegment()]
[StartupSegment()]
public class Game1C : ScriptBase
{
    public Game1C(NESEmulator emulator) : base(emulator)
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

        for (X = 0; X < 32; X++)
        {
            LDA(Palettes, X);

            // Write palette data to PPUDATA $2007
            STA(PPUDATA);
        }

        // Transfert sprite data into $200-$2ff memory range

        for (X = 0; X < 48; X++)
        {
            LDA(ControllerSprites, X);
            STA(0x200, X);
        }

        // Main game loop
        while (true)
        {
            Call<ReadController>(s => s.ReadControllerOne());

            UpdateController();

            MoveFace();

            // Wait for VBlank
            LDA(0x30);

            WaitForVBlank:
            CMP(0x30);
            if (BEQ()) goto WaitForVBlank;
        }
    }

    public void UpdateController()
    {
        // ## Update button palette
        // Init palette to zero
        LDXi(0);
        STX(0x20A);
        STX(0x20E);
        STX(0x212);
        STX(0x216);
        STX(0x21A);
        STX(0x21E);
        STX(0x222);
        STX(0x226);

        LDXi(1);

        // Check Button Right
        LDA(0x21);
        if ((A & 0b_0000_0001) != 0)
        {
            STX(0x20A);
        }

        // Check Button Left
        LDA(0x21);
        if ((A & 0b_0000_0010) != 0)
        {
            STX(0x20E);
        }

        // Check Button Down
        LDA(0x21);
        if ((A & 0b_0000_0100) != 0)
        {
            STX(0x212);
        }

        // Check Button Up
        LDA(0x21);
        if ((A & 0b_0000_1000) != 0)
        {
            STX(0x216);
        }

        // Check Button Start
        LDA(0x21);
        if ((A & 0b_0001_0000) != 0)
        {
            STX(0x21A);
        }

        // Check Button Select
        LDA(0x21);
        if ((A & 0b_0010_0000) != 0)
        {
            STX(0x21E);
        }

        // Check Button B
        LDA(0x21);
        if ((A & 0b_0100_0000) != 0)
        {
            STX(0x222);
        }

        // Check Button A
        LDA(0x21);
        if ((A & 0b_1000_0000) != 0)
        {
            STX(0x226);
        }
    }

    public void MoveFace()
    {
        // Move right
        LDA(0x21);
        if ((A & 0b_0000_0001) != 0)
        {
            INC(0x22B);
        }

        // Move right
        LDA(0x21);
        if ((A & 0b_0000_0010) != 0)
        {
            DEC(0x22B);
        }

        // Move down
        LDA(0x21);
        if ((A & 0b_0000_0100) != 0)
        {
            INC(0x228);
        }

        // Move up
        LDA(0x21);
        if ((A & 0b_0000_1000) != 0)
        {
            DEC(0x228);
        }
    }

    public void Nmi()
    {
        // Transfer Sprites via OAM
        LDAi(0x00);
        // 0x2003 = OAM_ADDR
        STA(0x2003);

        LDAi(0x02);
        // 0x4014 = OAM_DMA
        STA(0x4014);

        // Increment frame counter
        INC(0x30);
    }

    public void Reset()
    {
        SEI();
        CLD();
        
        LDXi(0b_0100_0000);
        STX(0x4017);

        LDXi(0xff);
        TXS();

        LDXi(0b_0001_0000);
        STX(0x2000);
        STX(0x2001);
        STX(0x4010);

        BIT(0x2002);

        vblankWait1:
        BIT(0x2002);
        if (BPL()) goto vblankWait1;

        // Clear Memory - TODO Generate a loop that can go all the way from exactly 0 to 255 and don't stop before
        LDAi(0x00);
        for (X = 0; X < 255; X++)
        {
            STA(0x0000, X);
            STA(0x0100, X);
            STA(0x0200, X);
            STA(0x0300, X);
            STA(0x0400, X);
            STA(0x0500, X);
            STA(0x0600, X);
            STA(0x0700, X);
        }

        vblankWait2:
        BIT(0x2002);
        if (BPL()) goto vblankWait2;

        ResetPalettes();

        // To Main

        // Enable Sprites & Background
        LDAi(0b_0001_1000); 
        STA(PPUMASK);

        // Enable NMI
        LDAi(0b_1000_0000);
        STA(PPUCTRL);

        Jump<Game1C>(s => s.Main());
    }

    public void ResetPalettes()
    {
        BIT(0x2002);
        
        LDAi(0x3f);
        STA(0x2006);
        LDAi(0x00);
        STA(0x2006);

        LDAi(0x0F);
        LDXi(0x20);

        paletteLoadLoop:

        STA(0x2007);
        DEX();
        if (BNE()) goto paletteLoadLoop;
    }

    [RomData]
    private byte[] ControllerSprites = [
        // Empty sprites
        0, 0, 0, 0,
        0, 0, 0, 0,

        // Right
        30,  // Y position - Top
        1,   // Tile 1
        0,   // Palette 4
        50,  // X position - Left

        // Left
        30,  // Y position - Top
        2,   // Tile 2
        0,   // Palette 4
        30,  // X position - Left

        // Down
        40,  // Y position - Top
        3,   // Tile 3
        0,   // Palette 4
        40,  // X position - Left

        // Up
        20,  // Y position - Top
        4,   // Tile 4
        0,   // Palette 4
        40,  // X position - Left

        // Start
        30,  // Y position - Top
        5,   // Tile 5
        0,   // Palette 4
        70,  // X position - Left

        // Select
        30,  // Y position - Top
        6,   // Tile 6
        0,   // Palette 4
        60,  // X position - Left

        // B
        30,  // Y position - Top
        7,   // Tile 7
        0,   // Palette 4
        80,  // X position - Left

        // A
        30,  // Y position - Top
        8,   // Tile 8
        1,   // Palette 4
        90,  // X position - Left

        // Face
        80,  // Y position - Top
        9,   // Tile 9
        3,   // Palette 6
        80,  // X position - Left

        // Heart
        80,  // Y position - Top
        10,  // Tile 10
        2,   // Palette 5
        100, // X position - Left
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
        0x29,
        0x00,
        0x00,
        0x0F,
        0x16,
        0x00,
        0x00,
        0x0F,
        0x12,
        0x00,
        0x00,
    ];

    [CharData]
    private byte[] Characters = [
        // First tile is empty
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,

        // Right
        0b_00001000,
        0b_00001100,
        0b_00001110,
        0b_11111111,
        0b_11111111,
        0b_00001110,
        0b_00001100,
        0b_00001000,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

        // Left
        0b_00010000,
        0b_00110000,
        0b_01110000,
        0b_11111111,
        0b_11111111,
        0b_01110000,
        0b_00110000,
        0b_00010000,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

        // Down
        0b_00011000,
        0b_00011000,
        0b_00011000,
        0b_00011000,
        0b_11111111,
        0b_01111110,
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

        // Up
        0b_00011000,
        0b_00111100,
        0b_01111110,
        0b_11111111,
        0b_00011000,
        0b_00011000,
        0b_00011000,
        0b_00011000,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

        // Start
        0b_00000000,
        0b_11100000,
        0b_10001110,
        0b_11000100,
        0b_01100100,
        0b_00100100,
        0b_11100100,
        0b_00000000,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

        // Select
        0b_00000000,
        0b_11100000,
        0b_10001110,
        0b_11001000,
        0b_01101110,
        0b_00101000,
        0b_11101110,
        0b_00000000,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

        // B
        0b_11111000,
        0b_11111100,
        0b_11000110,
        0b_11111100,
        0b_11111100,
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

        // A
        0b_00111100,
        0b_00111100,
        0b_11100110,
        0b_11000011,
        0b_11111111,
        0b_11111111,
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

        // Face
        0b_00111100,
        0b_01111110,
        0b_11011011,
        0b_11011011,
        0b_11111111,
        0b_11000011,
        0b_01111110,
        0b_00111100,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

        // Heart
        0b_00000000,
        0b_01100110,
        0b_01111110,
        0b_11111111,
        0b_11111111,
        0b_01111110,
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
    ];
}