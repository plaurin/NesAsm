namespace NesAsm.Emulator.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FileIncludeAttribute<T> : Attribute where T : NesScript
{
    public FileIncludeAttribute(string? relativeFolderPath = null)
    {
        Filepath = nameof(T);
        RelativeFolderPath = relativeFolderPath;
    }

    public string Filepath { get; }
    public string? RelativeFolderPath { get; }
}
