using Microsoft.CodeAnalysis;

namespace NesAsm.Analyzers.Visitors;

internal class VisitorContext
{
    public VisitorContext(SourceProductionContext context, Compilation compilation, AsmWriter asmWriter, CsWriter? csWriter = null)
    {
        Context = context;
        Compilation = compilation;
        Writer = asmWriter;
        CsWriter = csWriter;
        TypeCache = new TypeCache(Compilation);
    }

    protected VisitorContext(VisitorContext context)
    {
        Context = context.Context;
        Compilation = context.Compilation;
        Writer = context.Writer;
        CsWriter = context.CsWriter;
        TypeCache = context.TypeCache;
    }

    protected SourceProductionContext Context { get; }
    public Compilation Compilation { get; }
    public AsmWriter Writer { get; }
    public CsWriter? CsWriter { get; }
    public TypeCache TypeCache { get; }

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
    {
        Context.ReportDiagnostic(Diagnostic.Create(descriptor, location, messageArgs));
    }
}
