using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.BoxingRPG;

public static partial class BoxingRPGGame
{
    private static void DrawMetaSprite(ref byte startingSpriteIndex, byte x, byte y, byte palette, byte[,] spriteIndexes, bool[,]? flipHorizontals = null, bool[,]? flipVerticals = null, bool? drawBehindBackground = null)
    {
        var xOffset = x - 8 * spriteIndexes.GetLength(1) / 2;
        var yOffset = y - 8 * spriteIndexes.GetLength(0);
        var spriteIndex = startingSpriteIndex;
        for (int i = 0; i < spriteIndexes.GetLength(1); i++)
            for (int j = 0; j < spriteIndexes.GetLength(0); j++)
            {
                var flipHorizontally = flipHorizontals != null && flipHorizontals[j, i];
                var flipVertically = flipVerticals != null && flipVerticals[j, i];
                SetSpriteData(spriteIndex++, (byte)(xOffset + i * 8), (byte)(yOffset + j * 8), spriteIndexes[j, i], palette, isBehindBackground: drawBehindBackground.GetValueOrDefault(), flipHorizontally: flipHorizontally, flipVertically: flipVertically);
            }

        startingSpriteIndex = spriteIndex;
    }

    record MetaSpriteFrame(int nbFrames, byte[,] spriteIndexes, bool[,]? flipHorizontals = null, bool[,]? flipVerticals = null);

    private static void DrawMetaSpriteAnimation(ref byte startingSpriteIndex, byte x, byte y, byte palette, MetaSpriteFrame[] frames, int animationFrame, int totalAnimationFrames, bool? drawBehindBackground = null)
    {
        var currentAnimationFrame = animationFrame % totalAnimationFrames;
        for (int i = 0; i < frames.Length; i++)
        {
            if (currentAnimationFrame < frames[i].nbFrames)
            {
                DrawMetaSprite(ref startingSpriteIndex, x, y, palette, frames[i].spriteIndexes, frames[i].flipHorizontals, frames[i].flipVerticals, drawBehindBackground);
                return;
            }

            currentAnimationFrame -= frames[i].nbFrames;
        }
    }
}