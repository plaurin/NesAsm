﻿using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class DataGeneratorScript : NesScript
{
    public static void Start()
    {
        for (X = 0; X < 20; X++)
        {
            LDA(SpriteData, X);
            STA(0x200, X);
        }
    }

    [RomData]
    private static readonly byte[] SpriteData = GenerateSpriteData(47, 48, 0, 2);

    [RomData]
    private static readonly byte[] NamedArgument = GenerateSpriteData(x: 50, y: 51, tileIndex: 2, paletteIndex: 1);

    [RomData]
    private static readonly byte[] MixOrderedAndNamedArgument = GenerateSpriteData(36, 35, paletteIndex: 1, tileIndex: 8);

    [RomData]
    private static readonly byte[] ChangedArgumentOrder = GenerateSpriteData(paletteIndex: 3, y: 62, tileIndex: 4, x: 63);

    [RomData]
    private static readonly byte[] BaseName = NesScript.GenerateSpriteData(x: 74, y: 75, tileIndex: 6, paletteIndex: 0);
}
