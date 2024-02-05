using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NesAsm.Analyzers.Visitors;

internal class MethodVisitorContext : ClassVisitorContext
{
    private readonly Stack<ForLoopData> _forLoopDataStack = new();
    private int _forLoopIndex = 1;

    private readonly Stack<IfExitData> _ifExitDataStack = new();
    private int _ifExitIndex = 1;

    public MethodVisitorContext(ClassVisitorContext context) : base(context)
    {
    }

    public int ForLoopIndex => _forLoopIndex;

    public int IdExitIndex => _ifExitIndex;

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

    internal void PushIfExitData(string labelName, string ifExitPattern)
    {
        var ifExitData = new IfExitData { LabelName = labelName, IfExitPattern = ifExitPattern };

        _ifExitDataStack.Push(ifExitData);
        _ifExitIndex++;
    }

    internal bool IsLineMatchingIfExit(string line)
    {
        return _ifExitDataStack.Count > 0 && _ifExitDataStack.Peek().IfExitPattern == line;
    }

    internal IfExitData PopIfExitData()
    {
        return _ifExitDataStack.Pop();
    }

    internal void EnsureAllIfExitReached(Location location)
    {
        if (_ifExitDataStack.Count > 0)
        {
            ReportDiagnostic(Diagnostics.IfExitNotReached, location);
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

    public struct IfExitData
    {
        public string LabelName;
        public string IfExitPattern;
    }
}


