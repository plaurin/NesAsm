using System.Collections.Generic;

namespace NesAsm.Analyzers.Visitors;

internal class ClassVisitorContext : VisitorContext
{
    protected readonly List<string> _scriptReferences = new();

    public ClassVisitorContext(VisitorContext visitorContext, string[] allMethods) : base(visitorContext)
    {
        AllMethods = allMethods;
    }

    public ClassVisitorContext(ClassVisitorContext visitorContext) : base(visitorContext)
    {
        AllMethods = visitorContext.AllMethods;
        _scriptReferences = visitorContext._scriptReferences;
    }

    public string[] AllMethods { get; }

    public IEnumerable<string> ScriptReferences => _scriptReferences;

    internal void AddScriptReference(string script)
    {
        if (!_scriptReferences.Contains(script))
            _scriptReferences.Add(script);
    }
}