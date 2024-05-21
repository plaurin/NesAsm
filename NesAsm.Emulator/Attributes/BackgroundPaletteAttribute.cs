namespace NesAsm.Emulator.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class BackgroundPaletteAttribute : Attribute
{
    public BackgroundPaletteAttribute(int paletteIndex, int tileIndex, string? name = null)
    {
        PaletteIndex = paletteIndex;
        TileIndex = tileIndex;
        Name = name;
    }

    public int PaletteIndex { get; }
    public int TileIndex { get; }
    public string? Name { get; }
}

