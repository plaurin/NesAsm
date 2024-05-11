namespace NesAsm.Emulator;
using static NesAsm.Emulator.NESEmulatorStatic;

public abstract partial class FileBasedReference // TODO Rename Script in the end
{
    protected static Register A = new();
    protected static Register X = new();
    protected static Register Y = new();

    protected static void LDAa(byte argumentValue) => LDAi(argumentValue);
    protected static void LDXa(byte argumentValue) => LDXi(argumentValue);
    protected static void LDYa(byte argumentValue) => LDYi(argumentValue);

    protected static void LDAa(bool argumentValue) => LDAi((byte)(argumentValue ? 1 : 0));
    protected static void LDXa(bool argumentValue) => LDXi((byte)(argumentValue ? 1 : 0));
    protected static void LDYa(bool argumentValue) => LDYi((byte)(argumentValue ? 1 : 0));
}
