using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

[Script]
internal class MultiProcScript : FileBasedReference
{
    public static readonly byte Data = 0xFF;

    [RomData]
    public static byte[] SpritePalettes = [
        0x0F,
        0x20,
        0x0F,
        0x0F, // Palette 0
        0x0F,
        0x11,
        0x0F,
        0x0F, // Palette 1
        0x0F,
        0x15,
        0x0F,
        0x0F, // Palette 2
    ];

    public void ProcA()
    {
        LDAi(10);
    }

    public void ProcB()
    {
        LDAi(20);
    }

    public void ProcC()
    {
        LDAi(30);
    }
}
