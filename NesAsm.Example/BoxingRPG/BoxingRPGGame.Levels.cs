namespace NesAsm.Example.JumpMan;

public static partial class BoxingRPGGame
{
    // 256 x 240 (16 x 15 meta tiles)
    // 2 Header - Monsters stats
    // 8 Sky (5 visible)
    // 7 Ground
    // 2 Footer - Party stats

    // 30 meta tiles high
    // -2 - 8 - 7 - 2
    // 11 left for extra stats
    // -4 Extended party stats?
    // 22 left for sky and ground

    private static void LoadFightWith(string monster, string area)
    {
        // Draw Field
        DrawBlockFill(0, 2, 16, 8, SkyTiles, SkyPaletteIndex);
        DrawBlockFill(0, 10, 16, 7, GroundTiles, GroundPaletteIndex);

        DrawBlock(1, 9, BushLeftTiles, BushPaletteIndex);
        DrawBlock(2, 9, BushTiles, BushPaletteIndex);
        DrawBlock(3, 9, BushRightTiles, BushPaletteIndex);

        DrawBlock(11, 9, HillLeftTiles, HillPaletteIndex);
        DrawBlock(12, 9, HillSpotTiles, HillPaletteIndex);
        DrawBlock(12, 8, HillTopTiles, HillPaletteIndex);
        DrawBlock(13, 9, HillRightTiles, HillPaletteIndex);

        // DEBUG
        DrawBlock(0, 2, CloudTopTiles, CloudPaletteIndex);

        DrawBlock(5, 5, CloudTopLeftTiles, CloudPaletteIndex);
        DrawBlock(5, 6, CloudBottomLeftTiles, CloudPaletteIndex);
        DrawBlock(6, 5, CloudTopTiles, CloudPaletteIndex);
        DrawBlock(6, 6, CloudBottomTiles, CloudPaletteIndex);
        DrawBlock(7, 5, CloudTopRightTiles, CloudPaletteIndex);
        DrawBlock(7, 6, CloudBottomRightTiles, CloudPaletteIndex);
    }
}