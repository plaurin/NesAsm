using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class CharScript : ScriptBase
{
    public CharScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
    }

    public byte[] Sprite = [
        0b_01000001,
        0b_11000010,
        0b_01000100,
        0b_01001000,
        0b_00010000,
        0b_00100000,
        0b_01000000,
        0b_10000000,

        0B_00000001,
        0B_00000010,
        0B_00000100,
        0B_00001000,
        0B_00010110,
        0B_00100001,
        0B_01000010,
        0B_10000111,
    ];
}
