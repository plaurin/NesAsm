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

    public void Scan(SyntaxTree syntaxTree, SyntaxNode syntaxNode)
    {
        var model = _compilation.GetSemanticModel(syntaxTree);

        TypeInfo typeInfo = model.GetTypeInfo(syntaxNode);
        var classTypeSymbol = (INamedTypeSymbol?)typeInfo.Type;

        if (classTypeSymbol != null)
        {
            if (_typeCache.ContainsKey(classTypeSymbol.Name))
            {
                return;
            }

            var methods = new List<MethodInfo>();

            foreach (var member in classTypeSymbol.GetMembers())
            {
                if (member is IMethodSymbol method)
                {
                    var methodInfo = new MethodInfo { Name = method.Name };

                    foreach (var attribute in method.GetAttributes())
                    {
                        //foreach (var attribute in attributeList.Attributes)
                        {
                            if (attribute.AttributeClass?.Name == "MacroAttribute")
                                methodInfo.IsMacro = true;
                            else if (attribute.AttributeClass?.Name == "NoReturnProcAttribute")
                                methodInfo.IsNoReturnProc = true;
                        }
                    }

                    if (!(methodInfo.IsNoReturnProc || methodInfo.IsMacro))
                        methodInfo.IsProc = true;

                    methods.Add(methodInfo);
                }
            }
            //var methods = classTypeSymbol.GetMembers().OfType<IMethodSymbol>();

            //var macros = methods
            //    .Where(m => m.MethodKind != MethodKind.Constructor)
            //    .Where(m => m.GetAttributes().Any(a => a.AttributeClass?.Name == "MacroAttribute"))
            //    .Select(m => m.Name)
            //    .ToList();

            _typeCache.Add(classTypeSymbol.Name, methods.ToArray());
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

    internal MethodInfo GetMethod(string className, string methodName)
    {
        if (_typeCache.TryGetValue(className, out var methods))
        {
            var result = methods.FirstOrDefault(m => m.Name == methodName);
            if (result != null)
            {
                return result;
            }

            throw new InvalidOperationException($"Method {methodName} not found");
        }

        throw new InvalidOperationException($"Class {className} not found");
    }
}

internal class MethodInfo
{
    public string Name { get; set; } = string.Empty;
    public bool IsProc { get; set; }
    public bool IsMacro { get; set; }
    public bool IsNoReturnProc { get; set; }
}