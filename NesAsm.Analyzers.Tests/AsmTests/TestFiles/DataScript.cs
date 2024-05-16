using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class DataScript : NesScript
{
    private static readonly ushort JOYPAD1 = 0x4016;
    private static readonly ushort JOYPAD2 = 0x4017;

    public static void Start()
    {
        LDAi(1);
        STA(JOYPAD1);
        LDAi(0);
        STA(JOYPAD1);
    }

    [RomData]
    private static byte[] Palettes = [
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
    private static byte[] InvalidDataType = [0x00, 0x11];

    private static byte[] MissingDataType = [0x00, 0x11];
}
