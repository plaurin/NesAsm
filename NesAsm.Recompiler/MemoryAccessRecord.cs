namespace NesAsm.Recompiler;

public record MemoryAccessRecord(Subroutine Subroutine, Instruction Instruction, ushort RomAddress, ushort TargetAddress, bool IsRead, bool IsWrite, bool IsDirectAccess);