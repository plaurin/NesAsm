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
