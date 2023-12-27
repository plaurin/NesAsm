namespace NesAsm.Emulator;

[AttributeUsage(AttributeTargets.Class)]
public class PostFileIncludeAttribute : Attribute
{
    public PostFileIncludeAttribute(string filepath)
    {
        Filepath = filepath;
    }

    public string Filepath { get; }
}
