using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Example.Game1;

[HeaderSegment()]
[VectorsSegment()]
[StartupSegment()]
[FileInclude<Controller>("..")]
[FileInclude<PPU>("..")]
[FileInclude<Game1Char>]
public class Game1C : NesScript
{
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

    private const ushort FrameCounter = 0x30;

    [NoReturnProc]
    public static void Main()
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
            LDA(Game1Char.BackgroundPalettes, X);

            // Write palette data to PPUDATA $2007 PPU_DATA
            STA(PPU.PPU_DATA);
        }

        // Transfert sprite data into $200-$2ff memory range

        for (X = 0; X < 48; X++)
        {
            LDA(EmptySprites1, X);
            STA(0x200, X);
        }

        // Main game loop
        while (true)
        {
            Controller.ReadControllerOne();

            UpdateController();

            MoveFace();

            // Wait for VBlank
            LDA(FrameCounter);

            WaitForVBlank:
            CMP(FrameCounter);
            if (BEQ()) goto WaitForVBlank;
        }
    }

    public static void UpdateController()
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

    public static void MoveFace()
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
        INC(FrameCounter);
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

        Game1C.Main();
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
    private static readonly byte[] EmptySprites1 = GenerateSpriteData(0, 0, 0, 0);

    [RomData]
    private readonly byte[] EmptySprites2 = GenerateSpriteData(0, 0, 0, 0);

    [RomData]
    private readonly byte[] RightSprite = GenerateSpriteData(x: 50, y: 30, tileIndex: 1, paletteIndex: 0);

    [RomData]
    private readonly byte[] LeftSprite = GenerateSpriteData(x: 30, y: 30, tileIndex: 2, paletteIndex: 0);

    [RomData]
    private readonly byte[] DownSprite = GenerateSpriteData(x: 40, y: 40, tileIndex: 3, paletteIndex: 0);

    [RomData]
    private readonly byte[] UpSprite = GenerateSpriteData(x: 40, y: 20, tileIndex: 4, paletteIndex: 0);

    [RomData]
    private readonly byte[] StartSprite = GenerateSpriteData(x: 70, y: 30, tileIndex: 5, paletteIndex: 0);

    [RomData]
    private readonly byte[] SelectSprite = GenerateSpriteData(x: 60, y: 30, tileIndex: 6, paletteIndex: 0);

    [RomData]
    private readonly byte[] BSprite = GenerateSpriteData(x: 80, y: 30, tileIndex: 7, paletteIndex: 0);

    [RomData]
    private readonly byte[] ASprite = GenerateSpriteData(x: 90, y: 30, tileIndex: 8, paletteIndex: 0);

    [RomData]
    private readonly byte[] FaceSprite = GenerateSpriteData(x: 80, y: 80, tileIndex: 9, paletteIndex: 2);

    [RomData]
    private readonly byte[] HeartSprite = GenerateSpriteData(x: 100, y: 80, tileIndex: 10, paletteIndex: 3);
}
