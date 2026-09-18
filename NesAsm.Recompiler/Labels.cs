namespace NesAsm.Recompiler;

public class Labels
{
    private static readonly Dictionary<int, string> _romLabels = [];

    public static string GetLabelAndMemoryAddress(int address)
    {
        var result = GetLabelOrMemoryAddress(address);
        var hexAddress = address <= 0xFF ? address.ToString("X2") : address.ToString("X4");
        return result.Contains(hexAddress) ? result : $"{result}_{hexAddress}";
    }

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

            0x4000 => "Sq1Vol_4000",
            0x4001 => "Sq1Sweep_4001",
            0x4002 => "Sq1Lo_4002",
            0x4003 => "Sq1Hi_4003",
            0x4004 => "Sq2Vol_4004",
            0x4005 => "Sq2Sweep_4005",
            0x4006 => "Sq2Lo_4006",
            0x4007 => "Sq2Hi_4007",
            0x4008 => "TriLinear_4008",
            0x400A => "TriLo_400A",
            0x400B => "TriHi_400B",
            0x400C => "NoiseVol_400C",
            0x400E => "NoiseLo_400E",
            0x400F => "NoiseHi_400F",

            0x4011 => "DmcCounter_4011",
            0x4012 => "DmcAddress_4012",
            0x4013 => "DmcLength_4013",
            0x4014 => "SpriteDma_4014",
            0x4015 => "ApuStatus_4015",
            0x4016 => "Controller1_4016",
            0x4017 => "Ctrl2FrameCnt_4017",
            _ => address <= 0xFF ? $"${address:X2}" : $"${address:X4}",
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