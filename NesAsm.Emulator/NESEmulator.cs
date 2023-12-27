﻿namespace NesAsm.Emulator;

public class NESEmulator
{
    private readonly byte[] _memory = new byte[64 * 1024];

    private byte _a;
    private byte _x;
    private byte _y;

    public ReadOnlySpan<byte> Memory => _memory;
    public byte A => _a;
    public byte X => _x;
    public byte Y => _y;

    // Immediate Load instructions
    public void LDAi(byte value) => _a = value;
    public void LDXi(byte value) => _x = value;
    public void LDYi(byte value) => _y = value;

    public void INX() => _x++;
    public void INY() => _y++;

    public void DEC() => _a--;
    public void DEX() => _x--;
    public void DEY() => _y--;

    public void TAY() => _y = _a;
    public void TYA() => _a = _y;
    public void TAX() => _x = _a;
    public void TXA() => _a = _x;

    // Zero Page Store instructions
    public void STA(byte address) => _memory[address] = _a;
    public void STX(byte address) => _memory[address] = _x;
    public void STY(byte address) => _memory[address] = _y;

    // Zero Page Load instructions
    public void LDA(byte address) => _a = _memory[address];
    public void LDX(byte address) => _x = _memory[address];
    public void LDY(byte address) => _y = _memory[address];

    // Absolute Mode Store instructions
    public void STA(ushort address) => _memory[address] = _a;
    public void STX(ushort address) => _memory[address] = _x;
    public void STY(ushort address) => _memory[address] = _y;

    // Absolute Mode Load instructions
    public void LDA(ushort address) => _a = _memory[address];
    public void LDX(ushort address) => _x = _memory[address];
    public void LDY(ushort address) => _y = _memory[address];
}
