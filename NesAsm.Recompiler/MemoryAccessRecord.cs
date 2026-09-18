namespace NesAsm.Recompiler;

public record MemoryAccessRecord(Subroutine Subroutine, Instruction Instruction, ushort RomAddress, ushort TargetAddress, bool IsRead, bool IsWrite, bool IsJump, bool IsDirectAccess)
{
    public override string ToString()
    {
        var direct = IsDirectAccess ? "D" : "I";
        string read = IsRead ? "R" : " ";
        string write = IsWrite ? "W" : " ";
        string jump = IsJump ? "J" : "";
        return $"{direct}{read}{write}{jump} {Labels.GetLabelAndMemoryAddress(TargetAddress)}";
    }

    public MemoryRegion MemoryRegion
    {
        get
        {
            if (TargetAddress <= 0xFF)
                return MemoryRegion.ZeroPage;
            else if (TargetAddress <= 0x1FF)
                return MemoryRegion.StackPage;
            else if (TargetAddress <= 0x2FF)
                return MemoryRegion.OAMPage;
            if (TargetAddress <= 0x7FF)
                return MemoryRegion.OtherRAMPage;
            if (TargetAddress >= 0x2000 && TargetAddress <= 0x2007)
                return MemoryRegion.PPURegister;
            if (TargetAddress >= 0x4000 && TargetAddress <= 0x4013 || TargetAddress == 0x4015)
                return MemoryRegion.APURegister;
            if (TargetAddress == 0x4014)
                return MemoryRegion.OAMData;
            if (TargetAddress == 0x4016 || TargetAddress == 0x4017)
                return MemoryRegion.Joypad;
            if (TargetAddress >= 0x6000 && TargetAddress < 0x7FFF)
                return MemoryRegion.WorkRAM;
            if (TargetAddress >= 0x8000 && TargetAddress < 0xFFFF)
                return MemoryRegion.ROM;
            else
                return MemoryRegion.Unknown;
        }
    }

    public bool IsDynamicDispatch => Instruction.IsDynamicDispatch(Instruction.Opcode);
    public bool IsZeroPageAccess => MemoryRegion == MemoryRegion.ZeroPage;
    public bool IsStackPageAccess => MemoryRegion == MemoryRegion.StackPage;
    public bool IsOAMPageAccess => MemoryRegion == MemoryRegion.OAMPage;
    public bool IsOtherRAMPageAccess => MemoryRegion == MemoryRegion.OtherRAMPage;
    public bool IsPPURegisterAccess => MemoryRegion == MemoryRegion.PPURegister;
    public bool IsAPURegisterAccess => MemoryRegion == MemoryRegion.APURegister;
    public bool IsOAMDataAccess => MemoryRegion == MemoryRegion.OAMData;
    public bool IsJoypadAccess => MemoryRegion == MemoryRegion.Joypad;
    public bool IsWorkRAMAccess => MemoryRegion == MemoryRegion.WorkRAM;
    public bool IsROMAccess => MemoryRegion == MemoryRegion.ROM;
}
