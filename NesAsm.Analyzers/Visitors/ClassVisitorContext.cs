namespace NesAsm.Analyzers.Visitors;

internal class ClassVisitorContext : VisitorContext
{
    public ClassVisitorContext(VisitorContext visitorContext, string[] allMethods) : base(visitorContext)
    {
        AllMethods = allMethods;
    }

    public string[] AllMethods { get; }
}