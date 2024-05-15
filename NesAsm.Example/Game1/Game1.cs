using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Example.Game1;

[HeaderSegment()]
[VectorsSegment()]
[StartupSegment()]
[FileInclude<Controller>("..")]
public class Game1 : NesScript
{
    [NoReturnProc]
    public static void Main()
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

        // Transfert sprite data into $200-$2ff memory range
        LDXi(0x00);

        spriteLoop:

        LDA(ControllerSprites, X);
        STA(0x200, X);
        INX();

        CPXi(48);
        if (BNE()) goto spriteLoop;

        // Main game loop
        endless_loop:

        Controller.ReadControllerOne();

        UpdateController();

        MoveFace();

        // Wait for VBlank
        LDA(0x30);

        WaitForVBlank:
        CMP(0x30);
        if (BEQ()) goto WaitForVBlank;

        goto endless_loop;
    }

    public static void UpdateController()
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
        CheckRight:
        LDA(0x21);
        ANDi(0b_0000_0001);
        if (BEQ()) goto CheckLeft;
        STX(0x20A);

        // Check Button Left
        CheckLeft:
        LDA(0x21);
        ANDi(0b_0000_0010);
        if (BEQ()) goto CheckDown;
        STX(0x20E);

        // Check Button Down
        CheckDown:
        LDA(0x21);
        ANDi(0b_0000_0100);
        if (BEQ()) goto CheckUp;
        STX(0x212);

        // Check Button Up
        CheckUp:
        LDA(0x21);
        ANDi(0b_0000_1000);
        if (BEQ()) goto CheckStart;
        STX(0x216);

        // Check Button Start
        CheckStart:
        LDA(0x21);
        ANDi(0b_0001_0000);
        if (BEQ()) goto CheckSelect;
        STX(0x21A);

        // Check Button Select
        CheckSelect:
        LDA(0x21);
        ANDi(0b_0010_0000);
        if (BEQ()) goto CheckB;
        STX(0x21E);

        // Check Button B
        CheckB:
        LDA(0x21);
        ANDi(0b_0100_0000);
        if (BEQ()) goto CheckA;
        STX(0x222);

        // Check Button A
        CheckA:
        LDA(0x21);
        ANDi(0b_1000_0000);
        if (BEQ()) goto EndCheck;
        STX(0x226);

        EndCheck:
        // Hack because it is not possible to finish a method with a label?!?
        LDAi(0);
    }

    public static void MoveFace()
    {
        // Move right
        LDA(0x21);
        ANDi(0b_0000_0001);
        if (BEQ()) goto MoveLeft;
        INC(0x22B);

        MoveLeft:
        LDA(0x21);
        ANDi(0b_0000_0010);
        if (BEQ()) goto MoveDown;
        DEC(0x22B);

        MoveDown:
        LDA(0x21);
        ANDi(0b_0000_0100);
        if (BEQ()) goto MoveUp;
        INC(0x228);

        MoveUp:
        LDA(0x21);
        ANDi(0b_0000_1000);
        if (BEQ()) goto End;
        DEC(0x228);

        End:
        LDA(0);
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

        clearMemory:

        LDAi(0x00);

        STA(0x0000, X);
        STA(0x0100, X);
        STA(0x0200, X);
        STA(0x0300, X);
        STA(0x0400, X);
        STA(0x0500, X);
        STA(0x0600, X);
        STA(0x0700, X);

        INX();
        if (BNE()) goto clearMemory;

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

        Main();
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
    private static byte[] ControllerSprites = [
        // Empty sprites
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

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
    private static byte[] Palettes = [
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
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,

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