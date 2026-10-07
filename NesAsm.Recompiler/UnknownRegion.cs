namespace NesAsm.Recompiler;

public record UnknownRegion : MemoryRegion
{
    private readonly ushort _address;
    private readonly int _size;
    public UnknownRegion(ushort address, int size)
    {
        _address = address;
        _size = size;
    }
    public override ushort Address => _address;
    public override int Size => _size;
    public override string ToString()
    {
        var labelAndAddress = Labels.GetLabelAndMemoryAddress(Address);
        if (Size == 1)
            return $"Unknown {labelAndAddress} (Size: {Size})";
        else
        {
            return $"Unknown {labelAndAddress} to ${Address + Size - 1:X4} (Size: {Size})";
        }
    }
}
