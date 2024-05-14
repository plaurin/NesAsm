using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

[Script]
internal class DataScript : NesScript
{
    private readonly ushort JOYPAD1 = 0x4016;
    private readonly ushort JOYPAD2 = 0x4017;

    public void Main()
    {
        LDAi(1);
        STA(JOYPAD1);
        LDAi(0);
        STA(JOYPAD1);
    }

    [RomData]
    private byte[] Palettes = [
        // Background palettes
        0x0F, 0x20, 0x21, 0x22,
        0x0F, 0x00, 0x00, 0x00,
        0x0F, 0x00, 0x00, 0x00,
        0x0F, 0x00, 0x00, 0x00,

        // Sprite palettes
        0x0F, 0x20, 0x27, 0x31,
        0x0F, 0x00, 0x00, 0x00,
        0x0F, 0x00, 0x00, 0x00,
        0x0F, 0x00, 0x00, 0x00,
    ];

    [RomData]
    [CharData]
    private byte[] InvalidDataType = [0x00, 0x11];

    private byte[] MissingDataType = [0x00, 0x11];
}
