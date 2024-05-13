namespace NesAsm.Emulator;
public abstract partial class FileBasedReference
{
    /// <summary>
    /// PPUCTRL 0x2000
    /// </summary>
    protected const ushort PPUCTRL = 0x2000;

    /// <summary>
    /// PPUMASK 0x2001
    /// </summary>
    protected const ushort PPUMASK = 0x2001;

    /// <summary>
    /// PPUSTATUS 0x2002
    /// </summary>
    protected const ushort PPUSTATUS = 0x2002;

    /// <summary>
    /// PPUADDR 0x2006
    /// </summary>
    protected const ushort PPUADDR = 0x2006;

    /// <summary>
    /// PPUDATA 0x2007
    /// </summary>
    protected const ushort PPUDATA = 0x2007;

}
