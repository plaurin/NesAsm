namespace NesAsm.Emulator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ImportCharAttribute : Attribute
{
    public ImportCharAttribute(string filename)
    {
        Filename = filename;
    }

    public string Filename { get; }
}
