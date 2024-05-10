using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace NesAsm.Analyzers.Visitors;

internal class TypeCache
{
    private readonly Compilation _compilation;
    private readonly Dictionary<string, List<string>> _macroCache;

    public TypeCache(Compilation compilation)
    {
        _compilation = compilation;
        _macroCache = new Dictionary<string, List<string>>();
    }

    public void Scan(SyntaxTree syntaxTree, SyntaxNode syntaxNode)
    {
        var model = _compilation.GetSemanticModel(syntaxTree);

        TypeInfo typeInfo = model.GetTypeInfo(syntaxNode);
        var classTypeSymbol = (INamedTypeSymbol?)typeInfo.Type;

        if (classTypeSymbol != null)
        {
            var methods = classTypeSymbol.GetMembers().OfType<IMethodSymbol>();

            var macros = methods
                .Where(m => m.MethodKind != MethodKind.Constructor)
                .Where(m => m.GetAttributes().Any(a => a.AttributeClass?.Name == "MacroAttribute"))
                .Select(m => m.Name)
                .ToList();

            _macroCache.Add(classTypeSymbol.Name, macros);
        }
    }
}