using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class CharScript : NesScript
{
    public void Main()
    {
    }

    [CharData]
    public byte[] Sprite = [
        0b_01000001,
        0b_11000010,
        0b_01000100,
        0b_01001000,
        0b_00010000,
        0b_00100000,
        0b_01000000,
        0b_10000000,

        0b_00000001,
        0b_00000010,
        0b_00000100,
        0b_00001000,
        0b_00010110,
        0b_00100001,
        0b_01000010,
        0b_10000111,
    ];
}
