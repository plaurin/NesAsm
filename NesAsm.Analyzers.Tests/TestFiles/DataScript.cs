using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class DataScript : ScriptBase
{
    public DataScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
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
