namespace NesAsm.Recompiler;

public class Labels
{
    private static readonly Dictionary<int, string> _romLabels = [];

    public static string GetLabelOrMemoryAddress(int? address)
    {
        if (address.HasValue && _romLabels.TryGetValue(address.Value, out var label))
        {
            return label;
        }

        return address switch
        {
            0x2000 => "PpuControl_2000",
            0x2001 => "PpuMask_2001",
            0x2002 => "PpuStatus_2002",
            0x2003 => "OamAddr_2003",
            0x2004 => "OamData_2004",
            0x2005 => "PpuScroll_2005",
            0x2006 => "PpuAddr_2006",
            0x2007 => "PpuData_2007",

            0x4011 => "DmcCounter_4011",
            0x4012 => "DmcAddress_4012",
            0x4013 => "DmcLength_4013",
            0x4014 => "SpriteDma_4014",
            0x4015 => "ApuStatus_4015",
            0x4016 => "Controller1_4016",
            0x4017 => "Ctrl2FrameCounter_4017",
            _ => $"${address:X4}",
        };
    }

    public static void AddMemoryLabel(int address, string label)
    {
        _romLabels.Add(address, label);
    }

    public static string GetLabel(int address)
    {
        if (_romLabels.TryGetValue(address, out var label))
        {
            return label;
        }

        return string.Empty;
    }
}