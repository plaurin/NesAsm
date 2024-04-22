using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class MultiProcScript : ScriptBase
{
    public MultiProcScript(NESEmulator emulator) : base(emulator)
    {
    }

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
