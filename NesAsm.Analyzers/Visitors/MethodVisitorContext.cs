using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace NesAsm.Analyzers.Visitors;

internal class MethodVisitorContext : ClassVisitorContext
{
    private readonly Stack<ForLoopData> _forLoopDataStack = new();
    private int _forLoopIndex = 1;

    public MethodVisitorContext(ClassVisitorContext context) : base(context)
    {
    }

    public int ForLoopIndex => _forLoopIndex;

    internal void PushForLoopData(string labelName, string conditionOpCode, string conditionOperand, string incrementOpCode, string endloopPattern)
    {
        var forLoopData = new ForLoopData { LabelName = labelName, ConditionOpCode = conditionOpCode, ConditionOperand = conditionOperand, IncrementOpCode = incrementOpCode, EndloopPattern = endloopPattern };

        _forLoopDataStack.Push(forLoopData);
        _forLoopIndex++;
    }

    internal bool IsLineMatchingEndLoop(string line)
    {
        return _forLoopDataStack.Count > 0 && _forLoopDataStack.Peek().EndloopPattern == line;
    }

    internal ForLoopData PopForLoopData()
    {
        return _forLoopDataStack.Pop();
    }

    internal void EnsureAllForLoopEnded(Location location)
    {
        if (_forLoopDataStack.Count > 0)
        {
            ReportDiagnostic(Diagnostics.ForLoopNotClosed, location);
        }
    }

    public struct ForLoopData
    {
        public string LabelName;
        public string ConditionOpCode;
        public string ConditionOperand;
        public string IncrementOpCode;
        public string EndloopPattern;
    }
}
  

