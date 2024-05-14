using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Example;

[Script]
public class PPU : NesScript
{
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
}
