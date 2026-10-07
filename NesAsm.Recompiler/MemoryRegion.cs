using NesAsm.Emulator.Instructions;

namespace NesAsm.Recompiler;

public abstract record MemoryRegion()
{
    public abstract ushort Address { get; }
    public abstract int Size { get; }
}
