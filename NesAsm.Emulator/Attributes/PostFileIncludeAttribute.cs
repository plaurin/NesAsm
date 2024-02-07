namespace NesAsm.Emulator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class PostFileIncludeAttribute : Attribute
{
    public PostFileIncludeAttribute(string filepath)
    {
        Filepath = filepath;
    }

    public string Filepath { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public class FileIncludeAttribute<T> : Attribute where T : ScriptBase
{
    public FileIncludeAttribute()
    {
        Filepath = nameof(T);
    }

    public string Filepath { get; }
}
