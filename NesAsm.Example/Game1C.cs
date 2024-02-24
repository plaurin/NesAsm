using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Example;

[HeaderSegment()]
[VectorsSegment()]
[StartupSegment()]
[FileInclude<Controller>]
[FileInclude<PPU>]
public class Game1C : ScriptBase
{
    public Game1C(NESEmulator emulator) : base(emulator)
    {
    }

    private const ushort RightButtonPalette = 0x20A;
    private const ushort LeftButtonPalette = 0x20E;
    private const ushort DownButtonPalette = 0x212;
    private const ushort UpButtonPalette = 0x216;
    private const ushort StartButtonPalette = 0x21A;
    private const ushort SelectButtonPalette = 0x21E;
    private const ushort BButtonPalette = 0x222;
    private const ushort AButtonPalette = 0x226;

    private const ushort FaceX = 0x22B;
    private const ushort FaceY = 0x228;

    public void Main()
    {
        // Read the PPUSTATUS register $2002 PPU_STATUS
        LDA(PPU.PPU_STATUS);

        // Store the address $3f01 in the PPUADDR $2006 (begining of the background palette 0)
        LDAi(0x3F);
        // 0x2006 PPU_ADDR
        STA(PPU.PPU_ADDR);
        LDAi(0x00);
        // 0x2006 PPU_ADDR
        STA(PPU.PPU_ADDR);

        for (X = 0; X < 32; X++)
        {
            LDA(Palettes, X);

            // Write palette data to PPUDATA $2007 PPU_DATA
            STA(PPU.PPU_DATA);
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
            Call<Controller>(s => s.ReadControllerOne());

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
        STX(RightButtonPalette);
        STX(LeftButtonPalette);
        STX(DownButtonPalette);
        STX(UpButtonPalette);
        STX(StartButtonPalette);
        STX(SelectButtonPalette);
        STX(BButtonPalette);
        STX(AButtonPalette);

        LDXi(1);

        // Check Button Right
        LDA(Controller.Down1);
        if ((A & Controller.BUTTON_RIGHT) != 0)
        {
            STX(RightButtonPalette);
        }

        // Check Button Left
        LDA(Controller.Down1);
        if ((A & Controller.BUTTON_LEFT) != 0)
        {
            STX(LeftButtonPalette);
        }

        // Check Button Down
        LDA(Controller.Down1);
        if ((A & Controller.BUTTON_DOWN) != 0)
        {
            STX(DownButtonPalette);
        }

        // Check Button Up
        LDA(Controller.Down1);
        if ((A & Controller.BUTTON_UP) != 0)
        {
            STX(UpButtonPalette);
        }

        // Check Button Start
        LDA(Controller.Down1);
        if ((A & Controller.BUTTON_START) != 0)
        {
            STX(StartButtonPalette);
        }

        // Check Button Select
        LDA(Controller.Down1);
        if ((A & Controller.BUTTON_SELECT) != 0)
        {
            STX(SelectButtonPalette);
        }

        // Check Button B
        LDA(Controller.Down1);
        if ((A & Controller.BUTTON_B) != 0)
        {
            STX(BButtonPalette);
        }

        // Check Button A
        LDA(Controller.Down1);
        if ((A & Controller.BUTTON_A) != 0)
        {
            STX(AButtonPalette);
        }
    }

    public void MoveFace()
    {
        // Move right
        LDA(Controller.Down1);
        if ((A & Controller.BUTTON_RIGHT) != 0)
        {
            INC(FaceX);
        }

        // Move left
        LDA(Controller.Down1);
        if ((A & Controller.BUTTON_LEFT) != 0)
        {
            DEC(FaceX);
        }

        // Move down
        LDA(Controller.Down1);
        if ((A & Controller.BUTTON_DOWN) != 0)
        {
            INC(FaceY);
        }

        // Move up
        LDA(Controller.Down1);
        if ((A & Controller.BUTTON_UP) != 0)
        {
            DEC(FaceY);
        }
    }

    public void Nmi()
    {
        // Transfer Sprites via OAM
        LDAi(0x00);
        // 0x2003 = OAM_ADDR
        STA(PPU.OAM_ADDR);

        LDAi(0x02);
        // 0x4014 = OAM_DMA
        STA(PPU.OAM_DMA);

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
        // 0x2000 PPU_CTRL
        STX(PPU.PPU_CTRL);
        // 0x2001 PPU_MASK
        STX(PPU.PPU_MASK);
        STX(0x4010);

        // 0x2002 PPU_STATUS
        BIT(PPU.PPU_STATUS);

        vblankWait1:
        // 0x2002 PPU_STATUS
        BIT(PPU.PPU_STATUS);
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
        // 0x2002 PPU_STATUS
        BIT(PPU.PPU_STATUS);
        if (BPL()) goto vblankWait2;

        ResetPalettes();

        // To Main

        // Enable Sprites & Background
        LDAi(0b_0001_1000);
        // 0x2002 PPU_STATUS
        STA(PPU.PPU_MASK);

        // Enable NMI
        LDAi(0b_1000_0000);
        // 0x2000 PPU_CTRL
        STA(PPU.PPU_CTRL);

        Jump<Game1C>(s => s.Main());
    }

    public void ResetPalettes()
    {
        // 0x2002 PPU_STATUS
        BIT(PPU.PPU_STATUS);
        
        LDAi(0x3f);
        // 0x2006 PPU_ADDR
        STA(PPU.PPU_ADDR);
        LDAi(0x00);
        // 0x2006 PPU_ADDR
        STA(PPU.PPU_ADDR);

        LDAi(0x0F);
        LDXi(0x20);

        paletteLoadLoop:

        // 0x2007 PPU_DATA
        STA(PPU.PPU_DATA);
        DEX();
        if (BNE()) goto paletteLoadLoop;
    }

    [RomData]
    private readonly byte[] ControllerSprites = [
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
    private readonly byte[] Palettes = [
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
    private readonly byte[] Characters = [
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
