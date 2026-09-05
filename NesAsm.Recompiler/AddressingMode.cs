namespace NesAsm.Recompiler;

public enum AddressingMode
{
    Implicit,
    Accumulator,
    Immediate,
    ZeroPage,
    ZeroPageX,
    Absolute,
    AbsoluteX,
    AbsoluteY,
    Relative,
    Indirect,
    IndirectIndexed,
}
