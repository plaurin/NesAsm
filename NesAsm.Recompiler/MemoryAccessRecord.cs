namespace NesAsm.Recompiler;

public record MemoryAccessRecord(Subroutine Subroutine, Instruction Instruction, ushort RomAddress, ushort TargetAddress, bool IsRead, bool IsWrite, bool IsJump, bool IsBranch, bool IsDirectAccess, int? Size = 1)
{
    public override string ToString()
    {
        var direct = IsDirectAccess ? "D" : "I";
        string read = IsRead ? "R" : " ";
        string write = IsWrite ? "W" : " ";
        string jump = IsJump ? "J" : "";
        string branch = IsBranch ? "B" : "";
        return $"{direct}{read}{write}{jump} {LabelAndAddress}";
    }

    public string LabelAndAddress => Labels.GetLabelAndMemoryAddress(TargetAddress);

    public MemoryRegionKind MemoryRegion
    {
        get
        {
            if (TargetAddress <= 0xFF)
                return MemoryRegionKind.ZeroPage;
            else if (TargetAddress <= 0x1FF)
                return MemoryRegionKind.StackPage;
            else if (TargetAddress <= 0x2FF)
                return MemoryRegionKind.OAMPage;
            if (TargetAddress <= 0x7FF)
                return MemoryRegionKind.OtherRAMPage;
            if (TargetAddress >= 0x2000 && TargetAddress <= 0x2007)
                return MemoryRegionKind.PPURegister;
            if (TargetAddress >= 0x4000 && TargetAddress <= 0x4013 || TargetAddress == 0x4015)
                return MemoryRegionKind.APURegister;
            if (TargetAddress == 0x4014)
                return MemoryRegionKind.OAMData;
            if (TargetAddress == 0x4016 || TargetAddress == 0x4017)
                return MemoryRegionKind.Joypad;
            if (TargetAddress >= 0x6000 && TargetAddress < 0x7FFF)
                return MemoryRegionKind.WorkRAM;
            if (TargetAddress >= 0x8000 && TargetAddress < 0xFFFF)
                return MemoryRegionKind.ROM;
            else
                return MemoryRegionKind.Unknown;
        }
    }

    public bool IsDynamicDispatch => Instruction.IsDynamicDispatch(Instruction.Opcode);
    public bool IsZeroPageAccess => MemoryRegion == MemoryRegionKind.ZeroPage;
    public bool IsStackPageAccess => MemoryRegion == MemoryRegionKind.StackPage;
    public bool IsOAMPageAccess => MemoryRegion == MemoryRegionKind.OAMPage;
    public bool IsOtherRAMPageAccess => MemoryRegion == MemoryRegionKind.OtherRAMPage;
    public bool IsPPURegisterAccess => MemoryRegion == MemoryRegionKind.PPURegister;
    public bool IsAPURegisterAccess => MemoryRegion == MemoryRegionKind.APURegister;
    public bool IsOAMDataAccess => MemoryRegion == MemoryRegionKind.OAMData;
    public bool IsJoypadAccess => MemoryRegion == MemoryRegionKind.Joypad;
    public bool IsWorkRAMAccess => MemoryRegion == MemoryRegionKind.WorkRAM;
    public bool IsROMAccess => MemoryRegion == MemoryRegionKind.ROM;
}
