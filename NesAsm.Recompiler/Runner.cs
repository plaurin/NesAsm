using NesAsm.Emulator;

namespace NesAsm.Recompiler;

public class Runner
{
    private readonly NESEmulator _emulator;

    public Runner()
    {
        _emulator = new NESEmulator();
    }

    public void Run(IReadOnlyCollection<Subroutine> subroutines, int reset, int nmi)
    {
        var resetSub = subroutines.FirstOrDefault(s => s.Address == reset);
        var nmiSub = subroutines.FirstOrDefault(s => s.Address == nmi);

        if (resetSub != null)
        {
            RunSub(resetSub);
        }

        if (nmiSub != null)
        {
            RunSub(nmiSub);
        }
    }

    private void RunSub(Subroutine sub)
    {
        foreach (var instruction in sub.Instructions)
        {
            switch (instruction.Opcode)
            {
                case 0x00: _emulator.BRK(); break;
                case 0x05: _emulator.ORA(instruction.ByteArgument); break;
                case 0x08: _emulator.PHP(); break;
                case 0x09: _emulator.ORAi(instruction.ByteArgument); break;
                case 0x0A: _emulator.ASL(); break;
                case 0x0E: _emulator.ASL(instruction.UShortArgument); break;

                case 0x10: _emulator.BPL(); break;
                case 0x18: _emulator.CLC(); break;

                case 0x20: _emulator.JSR(); break;
                case 0x25: _emulator.AND(instruction.ByteArgument); break;
                case 0x29: _emulator.ANDi(instruction.ByteArgument); break;
                case 0x2A: _emulator.ROL(); break;
                case 0x2C: _emulator.BIT(instruction.UShortArgument); break;
                case 0x2D: _emulator.AND(instruction.UShortArgument); break;
                case 0x2E: _emulator.ROL(instruction.UShortArgument); break;

                case 0x30: _emulator.BMI(); break;
                case 0x38: _emulator.SEC(); break;
                case 0x3D: _emulator.ANDax(instruction.UShortArgument); break;

                case 0x40: _emulator.RTI(); break;
                case 0x45: _emulator.EOR(instruction.ByteArgument); break;
                case 0x46: _emulator.LSR(instruction.ByteArgument); break;
                case 0x48: _emulator.PHA(); break;
                case 0x49: _emulator.EORi(instruction.ByteArgument); break;
                case 0x4A: _emulator.LSR(); break;
                case 0x4C: _emulator.JMP(); break;

                case 0x60: _emulator.RTS(); break;
                case 0x65: _emulator.ADC(instruction.ByteArgument); break;
                case 0x68: _emulator.PLA(); break;
                case 0x69: _emulator.ADCi(instruction.ByteArgument); break;
                case 0x6A: _emulator.ROR(); break;
                case 0x6C: _emulator.JMP(); break;
                case 0x6D: _emulator.ADC(instruction.UShortArgument); break;

                case 0x75: _emulator.ADCx(instruction.ByteArgument); break;
                case 0x79: _emulator.ADCy(instruction.UShortArgument); break;
                case 0x7D: _emulator.ADCx(instruction.UShortArgument); break;
                case 0x7E: _emulator.RORx(instruction.UShortArgument); break;

                case 0x84: _emulator.STY(instruction.ByteArgument); break;
                case 0x85: _emulator.STA(instruction.ByteArgument); break;
                case 0x86: _emulator.STX(instruction.ByteArgument); break;
                case 0x88: _emulator.DEY(); break;
                case 0x8A: _emulator.TXA(); break;
                case 0x8C: _emulator.STY(instruction.UShortArgument); break;
                case 0x8D: _emulator.STA(instruction.UShortArgument); break;
                case 0x8E: _emulator.STX(instruction.UShortArgument); break;

                case 0x90: _emulator.BCC(); break;
                case 0x91: throw new NotImplementedException("STA IndirectIndexed"); break;
                case 0x95: throw new NotImplementedException("STA ZeroPageX"); break;
                case 0x98: _emulator.TYA(); break;
                case 0x99: throw new NotImplementedException("STA AbsoluteY"); break;
                case 0x9A: _emulator.TXS(); break;
                case 0x9D: throw new NotImplementedException("STA AbsoluteX"); break;

                case 0xA0: _emulator.LDYi(instruction.ByteArgument); break;
                case 0xA2: _emulator.LDXi(instruction.ByteArgument); break;
                case 0xA4: _emulator.LDY(instruction.ByteArgument); break;
                case 0xA5: _emulator.LDA(instruction.ByteArgument); break;
                case 0xA6: _emulator.LDX(instruction.ByteArgument); break;
                case 0xA8: _emulator.TAY(); break;
                case 0xA9: _emulator.LDAi(instruction.ByteArgument); break;
                case 0xAA: _emulator.TAX(); break;
                case 0xAC: _emulator.LDY(instruction.UShortArgument); break;
                case 0xAD: _emulator.LDA(instruction.UShortArgument); break;
                case 0xAE: _emulator.LDX(instruction.UShortArgument); break;

                case 0xB0: _emulator.BCS(); break;
                case 0xB1: throw new NotImplementedException("LDA IndirectIndexed"); break;
                case 0xB5: throw new NotImplementedException("LDA ZeroPageX"); break;
                case 0xB9: throw new NotImplementedException("LDA AbsoluteY"); break;
                case 0xBD: throw new NotImplementedException("LDA AbsoluteX"); break;
                case 0xBE: throw new NotImplementedException("LDX AbsoluteY"); break;

                case 0xC0: _emulator.CPYi(instruction.ByteArgument); break;
                case 0xC5: _emulator.CMP(instruction.ByteArgument); break;
                case 0xC6: throw new NotImplementedException("DEC ZeroPage"); break;
                case 0xC8: _emulator.INY(); break;
                case 0xC9: _emulator.CMPi(instruction.ByteArgument); break;
                case 0xCA: _emulator.DEX(); break;
                case 0xCD: _emulator.CMP(instruction.UShortArgument); break;
                case 0xCE: throw new NotImplementedException("DEC Absolute"); break;

                case 0xD0: _emulator.BNE(); break;
                case 0xD8: _emulator.CLD(); break;
                case 0xD9: throw new NotImplementedException("CMP AbsoluteY"); break;
                case 0xDD: throw new NotImplementedException("CMP AbsoluteX"); break;
                case 0xDE: throw new NotImplementedException("DEC AbsoluteX"); break;

                case 0xE0: _emulator.CPXi(instruction.ByteArgument); break;
                case 0xE6: throw new NotImplementedException("INC ZeroPage"); break;
                case 0xE8: _emulator.INX(); break;
                case 0xE9: throw new NotImplementedException("SBC Immediate"); break;
                case 0xED: throw new NotImplementedException("SBC Absolute"); break;
                case 0xEE: throw new NotImplementedException("INC Absolute"); break;

                case 0xF0: _emulator.BEQ(); break;
                case 0xF5: throw new NotImplementedException("SBC ZeroPageX"); break;
                case 0xF9: throw new NotImplementedException("SBC AbsoluteY"); break;

                default:
                    throw new NotImplementedException($"{instruction.Opcode:X4}: {instruction.Mnemonic} {instruction.Argument}");
            }
        }
    }
}