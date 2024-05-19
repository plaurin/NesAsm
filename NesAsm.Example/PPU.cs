using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Example;

public class PPU : NesScript
{
    // TODO Unscoped constants
    public const ushort PPU_CTRL = 0x2000;
    public const ushort PPU_MASK = 0x2001;
    public const ushort PPU_STATUS = 0x2002;

    public const ushort OAM_ADDR = 0x2003;
    public const ushort OAM_DATA = 0x2004;
    public const ushort OAM_DMA = 0x4014;

    public const ushort PPU_SCROLL = 0x2005;
    public const ushort PPU_ADDR = 0x2006;
    public const ushort PPU_DATA = 0x2007;

    public const ushort NAMETABLE_A = 0x2000;
    public const ushort NAMETABLE_B = 0x2400;
    public const ushort NAMETABLE_C = 0x2800;
    public const ushort NAMETABLE_D = 0x2c00;

    // TODO Macro outside of scope
    [Macro]
    public static void VramColRow(byte col, byte row, ushort nametable)
    {
        VramAddress((ushort)(nametable + row * 0x20 + col));
    }

    [Macro]
    public static void VramAddress(ushort address)
    {
        BIT(PPUSTATUS);
        LDA(address / 256);
        STA(PPUADDR);
        LDA(address % 256);
        STA(PPUADDR);
    }
}
