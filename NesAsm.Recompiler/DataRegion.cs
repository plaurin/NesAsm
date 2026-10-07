namespace NesAsm.Recompiler;

public record DataRegion : MemoryRegion
{
    private readonly ushort _address;
    private readonly int _size;

    public DataRegion(ushort address, int size)
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
            return $"Data {labelAndAddress} (Size: {Size})";
        else
        {
            return $"Data {labelAndAddress} to ${Address + Size - 1:X4} (Size: {Size})";
        }
    }
}