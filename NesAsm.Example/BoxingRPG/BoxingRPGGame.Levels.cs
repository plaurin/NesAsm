namespace NesAsm.Example.JumpMan;

public static partial class BoxingRPGGame
{
    enum MapPattern
    {
        HillStart,
        HillLeft1,
        HillTop1,
        HillTop2,
        HillRight1,
        HillEnd,

        BushStart,
        BushFull,
        BushEnd,

        CloudStart,
        CloudFull,
        CloudEnd,

        CoinBox,
        Brick,
        EmptyBox,
        Block,

        PipeLeft,
        PipeRight,

        Hole,

        Flag,

        CastleWall,
        CastleWallLvl2Left,
        CastleWallLvl2,
        CastleWallLvl2Right,
        CastleDoor
    }

    static List<(MapPattern, int)> BackgroundPatterns = [];
    static List<(MapPattern, int, int)> BackgroundPatternsHeight = [];
    static int PatternLegth;

    static List<(MapPattern, int, int)> ForegroundElements = [];

    private static void LoadFightWith(string monster)
    {
        // Draw Background
        DrawBlock(0, 12, QuestionTiles, QuestionPalette);
        DrawBlock(5, 10, HudCoinTiles, HudCoinPalette);
        DrawBlock(15, 10, HudCoinTiles, HudCoinPalette);

        //// Repeating background patterns
        //PatternLegth = 48;

        //SetBigHill(0);
        //SetCloud(8, 1, 3);
        //SetBush(11, 3);
        //SetSmallHill(16);
        //SetCloud(19, 1, 2);
        //SetBush(23, 1);
        //SetCloud(27, 3, 3);
        //SetCloud(36, 2, 2);
        //SetBush(41, 2);

        //// Forground map elements
        //SetForegroundElement(16, 9, MapPattern.CoinBox);

        //SetForegroundElement(20, 9, MapPattern.Brick);
        //SetForegroundElement(21, 9, MapPattern.CoinBox);
        //SetForegroundElement(22, 5, MapPattern.CoinBox);
        //SetForegroundElement(22, 9, MapPattern.Brick);
        //SetForegroundElement(23, 9, MapPattern.CoinBox);
        //SetForegroundElement(24, 9, MapPattern.Brick);

        //SetForegroundElement(28, 2, MapPattern.PipeLeft);
        //SetForegroundElement(29, 2, MapPattern.PipeRight);

        //SetForegroundElement(38, 3, MapPattern.PipeLeft);
        //SetForegroundElement(39, 3, MapPattern.PipeRight);

        //SetForegroundElement(46, 4, MapPattern.PipeLeft);
        //SetForegroundElement(47, 4, MapPattern.PipeRight);

        //SetForegroundElement(57, 4, MapPattern.PipeLeft);
        //SetForegroundElement(58, 4, MapPattern.PipeRight);

        //SetForegroundElement(64, 8, MapPattern.CoinBox);

        //SetForegroundElement(69, 0, MapPattern.Hole);
        //SetForegroundElement(70, 0, MapPattern.Hole);

        //SetForegroundElement(77, 9, MapPattern.Brick);
        //SetForegroundElement(78, 9, MapPattern.CoinBox);
        //SetForegroundElement(79, 9, MapPattern.Brick);

        //SetForegroundElement(80, 5, MapPattern.Brick);
        //SetForegroundElement(81, 5, MapPattern.Brick);
        //SetForegroundElement(82, 5, MapPattern.Brick);
        //SetForegroundElement(83, 5, MapPattern.Brick);
        //SetForegroundElement(84, 5, MapPattern.Brick);
        //SetForegroundElement(85, 5, MapPattern.Brick);
        //SetForegroundElement(86, 5, MapPattern.Brick);
        //SetForegroundElement(86, 0, MapPattern.Hole);
        //SetForegroundElement(87, 5, MapPattern.Brick);
        //SetForegroundElement(87, 0, MapPattern.Hole);

        //SetForegroundElement(91, 5, MapPattern.Brick);
        //SetForegroundElement(92, 5, MapPattern.Brick);
        //SetForegroundElement(93, 5, MapPattern.Brick);
        //SetForegroundElement(94, 5, MapPattern.CoinBox);
        //SetForegroundElement(94, 9, MapPattern.CoinBox);

        //SetForegroundElement(100, 9, MapPattern.Brick);
        //SetForegroundElement(101, 9, MapPattern.CoinBox);

        //SetForegroundElement(106, 9, MapPattern.CoinBox);
        //SetForegroundElement(109, 5, MapPattern.CoinBox);
        //SetForegroundElement(109, 9, MapPattern.CoinBox);
        //SetForegroundElement(112, 9, MapPattern.CoinBox);

        //SetForegroundElement(118, 9, MapPattern.Brick);

        //SetForegroundElement(121, 5, MapPattern.Brick);
        //SetForegroundElement(122, 5, MapPattern.Brick);
        //SetForegroundElement(123, 5, MapPattern.Brick);

        //SetForegroundElement(128, 5, MapPattern.Brick);
        //SetForegroundElement(129, 5, MapPattern.CoinBox);
        //SetForegroundElement(129, 9, MapPattern.Brick);
        //SetForegroundElement(130, 5, MapPattern.CoinBox);
        //SetForegroundElement(130, 9, MapPattern.Brick);
        //SetForegroundElement(131, 5, MapPattern.Brick);

        //SetForegroundElement(134, 1, MapPattern.Block);
        //SetForegroundElement(135, 2, MapPattern.Block);
        //SetForegroundElement(136, 3, MapPattern.Block);
        //SetForegroundElement(137, 4, MapPattern.Block);

        //SetForegroundElement(140, 4, MapPattern.Block);
        //SetForegroundElement(141, 3, MapPattern.Block);
        //SetForegroundElement(142, 2, MapPattern.Block);
        //SetForegroundElement(143, 1, MapPattern.Block);

        //SetForegroundElement(148, 1, MapPattern.Block);
        //SetForegroundElement(149, 2, MapPattern.Block);
        //SetForegroundElement(150, 3, MapPattern.Block);
        //SetForegroundElement(151, 4, MapPattern.Block);
        //SetForegroundElement(152, 4, MapPattern.Block);

        //SetForegroundElement(153, 0, MapPattern.Hole);
        //SetForegroundElement(154, 0, MapPattern.Hole);

        //SetForegroundElement(155, 4, MapPattern.Block);
        //SetForegroundElement(156, 3, MapPattern.Block);
        //SetForegroundElement(157, 2, MapPattern.Block);
        //SetForegroundElement(158, 1, MapPattern.Block);

        //SetForegroundElement(163, 2, MapPattern.PipeLeft);
        //SetForegroundElement(164, 2, MapPattern.PipeRight);

        //SetForegroundElement(168, 9, MapPattern.Brick);
        //SetForegroundElement(169, 9, MapPattern.Brick);
        //SetForegroundElement(170, 9, MapPattern.CoinBox);
        //SetForegroundElement(171, 9, MapPattern.Brick);

        //SetForegroundElement(179, 2, MapPattern.PipeLeft);
        //SetForegroundElement(180, 2, MapPattern.PipeRight);

        //SetForegroundElement(181, 1, MapPattern.Block);
        //SetForegroundElement(182, 2, MapPattern.Block);
        //SetForegroundElement(183, 3, MapPattern.Block);
        //SetForegroundElement(184, 4, MapPattern.Block);
        //SetForegroundElement(185, 5, MapPattern.Block);
        //SetForegroundElement(186, 6, MapPattern.Block);
        //SetForegroundElement(187, 7, MapPattern.Block);
        //SetForegroundElement(188, 8, MapPattern.Block);
        //SetForegroundElement(189, 8, MapPattern.Block);

        //SetForegroundElement(198, 0, MapPattern.Flag);
        //SetForegroundElement(198, 1, MapPattern.Block);

        //SetForegroundElement(202, 0, MapPattern.CastleWall);
        //SetForegroundElement(203, 0, MapPattern.CastleWallLvl2Left);
        //SetForegroundElement(204, 0, MapPattern.CastleDoor);
        //SetForegroundElement(205, 0, MapPattern.CastleWallLvl2Right);
        //SetForegroundElement(206, 0, MapPattern.CastleWall);
    }

    #region Background Patterns

    private static void SetBigHill(int x) => SetBackgroundPattern(x, MapPattern.HillStart, MapPattern.HillLeft1, MapPattern.HillTop2, MapPattern.HillRight1, MapPattern.HillEnd);
    private static void SetSmallHill(int x) => SetBackgroundPattern(x, MapPattern.HillStart, MapPattern.HillTop1, MapPattern.HillEnd);
    private static void SetBush(int x, int length) => SetBackgroundPatterns(x, length, MapPattern.BushStart, MapPattern.BushFull, MapPattern.BushEnd);
    private static void SetCloud(int x, int length, int height) => SetBackgroundPatternsHeight(x, length, height, MapPattern.CloudStart, MapPattern.CloudFull, MapPattern.CloudEnd);

    private static void SetBackgroundPattern(int x, params MapPattern[] patterns)
    {
        for (int i = 0; i < patterns.Length; i++) BackgroundPatterns.Add((patterns[i], x + i));
    }

    private static void SetBackgroundPatternHeight(int x, int height, params MapPattern[] patterns)
    {
        for (int i = 0; i < patterns.Length; i++) BackgroundPatternsHeight.Add((patterns[i], x + i, height));
    }

    private static void SetBackgroundPatterns(int x, int length, MapPattern patternStart, MapPattern patternMiddle, MapPattern patternEnd)
    {
        SetBackgroundPattern(x, patternStart);
        for (int i = 0; i < length; i++) BackgroundPatterns.Add((patternMiddle, x + i + 1));
        SetBackgroundPattern(x + length + 1, patternEnd);
    }

    private static void SetBackgroundPatternsHeight(int x, int length, int height, MapPattern patternStart, MapPattern patternMiddle, MapPattern patternEnd)
    {
        SetBackgroundPatternHeight(x, height, patternStart);
        for (int i = 0; i < length; i++) BackgroundPatternsHeight.Add((patternMiddle, x + i + 1, height));
        SetBackgroundPatternHeight(x + length + 1, height, patternEnd);
    }

    #endregion

    #region Foreground map elements

    private static void SetForegroundElement(int x, int height, MapPattern mapPattern) => ForegroundElements.Add((mapPattern, x, height));

    #endregion
}