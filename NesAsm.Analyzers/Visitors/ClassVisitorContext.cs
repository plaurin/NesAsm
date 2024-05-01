using System.Collections.Generic;
using System.Linq;

namespace NesAsm.Analyzers.Visitors;

internal class ClassVisitorContext : VisitorContext
{
    protected readonly List<ScriptReference> _preScriptReferences = new();
    protected readonly List<ScriptReference> _scriptReferences = new();
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

    public IEnumerable<string> ScriptReferences => _scriptReferences.Select(sr => sr.RelativePath != null ? $"{sr.RelativePath}/{sr.Script}" : sr.Script);
    
    public IEnumerable<string> Consts => _consts;

    public bool IsStartupClass { get; private set; } = false;

    internal void AddPreScriptReference(string filepath, string? relativePath = null)
    {
        var script = $"{filepath}.s";
        if (_preScriptReferences.All(sr => sr.Script != script))
        {
            _preScriptReferences.Add(new ScriptReference { Script = script, RelativePath = relativePath });
            var scriptPath = relativePath != null ? $"{relativePath}/{script}" : script;
            Writer.IncludeFile(scriptPath);
            Writer.WriteEmptyLine();
        }
    }

    internal void AddScriptReference(string script, string? relativePath = null)
    {
        if (_scriptReferences.All(sr => sr.Script != script) && _preScriptReferences.All(sr => sr.Script != script))
            _scriptReferences.Add(new ScriptReference { Script = script, RelativePath = relativePath });
    }

    internal void SetStartupClass()
    {
        IsStartupClass = true;
    }

    internal void AddConst(string constName)
    {
        _consts.Add(constName);
    }

    internal struct ScriptReference
    {
        public string Script { get; set; }
        public string? RelativePath { get; set; }
    }
}