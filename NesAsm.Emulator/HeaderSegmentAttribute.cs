namespace NesAsm.Emulator;

[AttributeUsage(AttributeTargets.Class)]
public class HeaderSegmentAttribute : Attribute
{
    public HeaderSegmentAttribute(byte prgRomBanks = 2, byte chrRomBanks = 1, byte mapper = 1, bool verticalMirroring = true)
    {
        ProgramRomBanks = prgRomBanks;
        CharacterRomBanks = chrRomBanks;
        Mapper = mapper;
        IsVerticalMirroring = verticalMirroring;
    }

    public byte ProgramRomBanks { get; }
    public byte CharacterRomBanks { get; }
    public byte Mapper { get; }
    public bool IsVerticalMirroring { get; }
}