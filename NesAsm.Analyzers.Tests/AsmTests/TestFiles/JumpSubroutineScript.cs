﻿using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class JumpSubroutineScript : NesScript
{
    public static void ProcA()
    {
        ProcB();
        ProcC();
    }

    public static void ProcB()
    {
        LDAi(1);

        ProcC();
    }

    [NoReturnProc]
    public static void ProcC()
    {
        LDAi(2);

        JumpInsideProc();
    }

    [NoReturnProc]
    public static void JumpInsideProc()
    {
        loop:

        LDAi(2);

        goto loop;
    }
}
