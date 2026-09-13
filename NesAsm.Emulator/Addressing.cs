namespace NesAsm.Emulator;

public readonly record struct Addressing(AddressingMode Mode, ushort Address, byte ExtraCycle);
