using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NesAsm.Analyzers.Visitors;

internal class TypeCache
{
    private readonly Compilation _compilation;
    private readonly Dictionary<string, MethodInfo[]> _typeCache;

    public TypeCache(Compilation compilation)
    {
        _compilation = compilation;
        _typeCache = new Dictionary<string, MethodInfo[]>();
    }

    public void Scan(ClassDeclarationSyntax classDeclarationSyntax)
    {
        var methods = new List<MethodInfo>();

        foreach (var member in classDeclarationSyntax.Members)
        {
            if (member is MethodDeclarationSyntax method)
            {
                var methodInfo = new MethodInfo { Name = method.Identifier.Text };

                foreach (var attributeList in method.AttributeLists)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        if (attribute.Name.ToString() == "Macro")
                            methodInfo.IsMacro = true;
                        else if (attribute.Name.ToString() == "NoReturnProc")
                            methodInfo.IsNoReturnProc = true;
                    }
                }

                if (!(methodInfo.IsNoReturnProc || methodInfo.IsMacro))
                    methodInfo.IsProc = true;

                methods.Add(methodInfo);
            }
        }

        _typeCache.Add(classDeclarationSyntax.Identifier.Text, methods.ToArray());
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

            _typeCache.Add(classTypeSymbol.Name, macros.Select(m => new MethodInfo { Name = m, IsMacro = true }).ToArray());
        }
    }

    internal MethodInfo GetMethod(string operation, string[] allMethods)
    {
        foreach (var typeInfo in _typeCache)
        {
            if (typeInfo.Value.Select(m => m.Name).Except(allMethods).Count() == 0 && allMethods.Except(typeInfo.Value.Select(m => m.Name)).Count() == 0)
            {
                var result = typeInfo.Value.FirstOrDefault(m => m.Name == operation);
                if (result != null)
                {
                    return result;
                }
            }
        }

        throw new InvalidOperationException($"Method {operation} not found");
    }
}

internal class MethodInfo
{
    public string Name { get; set; } = string.Empty;
    public bool IsProc { get; set; }
    public bool IsMacro { get; set; }
    public bool IsNoReturnProc { get; set; }
}