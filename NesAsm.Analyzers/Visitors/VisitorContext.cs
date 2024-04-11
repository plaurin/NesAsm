using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NesAsm.Analyzers.Visitors;

internal class VisitorContext
{
    public VisitorContext(SourceProductionContext context, Compilation compilation, AsmWriter writer)
    {
        Context = context;
        Compilation = compilation;
        Writer = writer;
    }

    protected VisitorContext(VisitorContext context)
    {
        Context = context.Context;
        Compilation = context.Compilation;
        Writer = context.Writer;
    }

    protected SourceProductionContext Context { get; }
    public Compilation Compilation { get; }
    public AsmWriter Writer { get; }

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
    {
        Context.ReportDiagnostic(Diagnostic.Create(descriptor, location, messageArgs));
    }
}
