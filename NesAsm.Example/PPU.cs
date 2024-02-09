using NesAsm.Emulator;

namespace NesAsm.Example;

public class PPU : ScriptBase
{
    public PPU(NESEmulator emulator) : base(emulator)
    {
    }

    public const ushort PPU_CTRL = 0x2000;
    public const ushort PPU_MASK = 0x2001;
    public const ushort PPU_STATUS = 0x2002;

    public const ushort OAM_ADDR = 0x2003;
    public const ushort OAM_DATA = 0x2004;
    public const ushort OAM_DMA = 0x4014;

    public const ushort PPU_SCROLL = 0x2005;
    public const ushort PPU_ADDR = 0x2006;
    public const ushort PPU_DATA = 0x2007;
}
