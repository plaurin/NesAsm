using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.TestFiles;

[PostFileInclude("wrapper.s")]
internal class IterationStatementsScript : ScriptBase
{
    public IterationStatementsScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
        // for - using X or Y register
        for (X = 0; X < 10; X++)
        {
            LDA(Data, X);

            STA(PPUDATA);
        }

        for (Y = 0; Y < 10; Y++)
        {
            LDA(Data, Y);
        }

        // for - nested loop
        for (X = 0; X < 4; X++)
        {
            for (Y = 0; Y < 4; Y++)
            {
                LDA(Data, X);
                LDA(Data, Y);
            }
        }

        // TODO while condition

        // while true - infinite loop
        while (true)
        {
            LDAi(42);
        }
    }

    [RomData]
    private readonly byte[] Data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

    public int Z { get; set; }

    public void InvalidFor()
    {
        // NA0013 - Should be X or Y only
        for (Z = 0; Z < 10; Z++)
        {
        }

        // NA0015 - Should not declare new local variable X
        for (int X = 0; X < 10; X++)
        {
        }

        // NA0014 - Should not mix X and Y
        for (X = 0; Y < 10; X++)
        {
        }

        // NA0016 - Should not use the same register in nester loop
        for (X = 0; X < 10; X++)
            for (X = 0; X < 10; X++)
            {
            }

        // NA0008 - do is not supported
        do
        {
        } while (true);
    }

}
