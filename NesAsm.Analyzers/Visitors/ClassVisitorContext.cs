using Microsoft.CodeAnalysis;

namespace NesAsm.Analyzers.Visitors;

internal class ClassVisitorContext
{
    private readonly VisitorContext _classContext;

    public ClassVisitorContext(VisitorContext classContext, string[] allMethods)
    {
        _classContext = classContext;
        AllMethods = allMethods;
    }

    public Compilation Compilation => _classContext.Compilation;
    public AsmWriter Writer => _classContext.Writer;

    public string[] AllMethods { get; }
}