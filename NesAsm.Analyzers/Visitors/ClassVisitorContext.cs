using System.Collections.Generic;

namespace NesAsm.Analyzers.Visitors;

internal class ClassVisitorContext : VisitorContext
{
    protected readonly List<string> _preScriptReferences = new();
    protected readonly List<string> _scriptReferences = new();
    protected readonly List<string> _consts = new();

    public ClassVisitorContext(VisitorContext visitorContext, string[] allMethods) : base(visitorContext)
    {
        AllMethods = allMethods;
    }

    public ClassVisitorContext(ClassVisitorContext visitorContext) : base(visitorContext)
    {
        AllMethods = visitorContext.AllMethods;
        _preScriptReferences = visitorContext._preScriptReferences;
        _scriptReferences = visitorContext._scriptReferences;
        _consts = visitorContext._consts;
    }

    public string[] AllMethods { get; }

    public IEnumerable<string> ScriptReferences => _scriptReferences;
    
    public IEnumerable<string> Consts => _consts;

    public bool IsStartupClass { get; private set; } = false;

    internal void AddPreScriptReference(string filepath)
    {
        var script = $"{filepath}.s";
        if (!_preScriptReferences.Contains(script))
        {
            _preScriptReferences.Add(script);
            Writer.IncludeFile(script);
            Writer.WriteEmptyLine();
        }
    }

    internal void AddScriptReference(string script)
    {
        if (!_scriptReferences.Contains(script) && !_preScriptReferences.Contains(script))
            _scriptReferences.Add(script);
    }

    internal void SetStartupClass()
    {
        IsStartupClass = true;
    }

    internal void AddConst(string constName)
    {
        _consts.Add(constName);
    }
}