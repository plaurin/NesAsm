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

    public void LDX(byte value) => _x = value;
    public void LDY(byte value) => _y = value;
    public void INX() => _x++;
    public void INY() => _y++;
    public void DEX() => _x--;
    public void DEY() => _y--;
}
