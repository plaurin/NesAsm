namespace NesAsm.Recompiler;

public enum MemoryRegionKind
{
    Unknown,
    ZeroPage,
    StackPage,
    OAMPage,
    OtherRAMPage,
    PPURegister,
    APURegister,
    OAMData,
    Joypad,
    WorkRAM,
    ROM
}