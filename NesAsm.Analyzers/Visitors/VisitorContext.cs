using Microsoft.CodeAnalysis;

namespace NesAsm.Analyzers.Visitors;

internal class VisitorContext
{
    public VisitorContext(Compilation compilation, AsmWriter writer)
    {
        Compilation = compilation;
        Writer = writer;
    }

    public Compilation Compilation { get; }
    public AsmWriter Writer { get; }
}
