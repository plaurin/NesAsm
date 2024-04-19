namespace NesAsm.Emulator.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FileIncludeAttribute<T> : Attribute where T : FileBasedReference
{
    public FileIncludeAttribute()
    {
        Filepath = nameof(T);
    }

    public string Filepath { get; }
}
