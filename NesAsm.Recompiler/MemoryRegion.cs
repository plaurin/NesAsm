namespace NesAsm.Recompiler;

public enum MemoryRegion
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