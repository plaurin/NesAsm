namespace NesAsm.Emulator;

public abstract partial class ScriptBase : FileBasedReference
{
    protected static byte[] GenerateSpriteData(byte x, byte y, byte tileIndex, byte paletteIndex)
    {
        return
        [
            y,
            tileIndex,
            paletteIndex,
            x,
        ];
    }
}
