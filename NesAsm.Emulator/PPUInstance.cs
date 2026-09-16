namespace NesAsm.Emulator;

public class PPUInstance
{
    private const int CyclesPerScanlines = 341;
    private const int TotalScanlines = 262;

    public const ushort PPUCTRL = 0x2000;
    public const ushort PPUMASK = 0x2001;
    public const ushort PPUSTATUS = 0x2002;
    public const ushort OAMADDR = 0x2003;
    public const ushort OAMDATA = 0x2004;
    public const ushort PPUSCROLL = 0x2005;
    public const ushort PPUADDR = 0x2006;
    public const ushort PPUDATA = 0x2007;

    private byte _ppuCtrl;
    private byte _ppuMask;
    private byte _ppuStatus;
    private byte _oamAddr;
    private byte _oamData;
    private byte _ppuScroll;
    private byte _ppuAddr;
    private byte _ppuData;

    private bool _addrLatch;

    private long _lastCpuCycle;
    private int _scanline;
    private int _scanlineCycle;
    private int _frame;

    public override string ToString() => $"F:{_frame} S:{_scanline} C:{_scanlineCycle}";

    public byte ReadRegister(ushort address)
    {
        switch (address)
        {
            case PPUSTATUS:
                var result = _ppuStatus;
                _ppuStatus &= 0x3F;
                _addrLatch = false;
                return result;
        }
        return 0;
    }

    public void RunToCycle(long cpuCycles)
    {
        for (long i = _lastCpuCycle; i < cpuCycles; i++)
            for (int j = 0; j < 3; j++)
            {
                if (_scanline == 0 && _scanlineCycle == 0)
                {
                    _ppuStatus &= 0x3F;
                }

                _scanlineCycle++;

                if (_scanlineCycle >= CyclesPerScanlines)
                {
                    _scanlineCycle = 0;
                    // Todo Render line

                    _scanline++;
                    if (_scanline == TotalScanlines)
                    {
                        _frame++;
                        _scanline = 0;
                    }
                }

                if (_scanline == 241 && _scanlineCycle == 0)
                {
                    _ppuStatus |= 0x80;
                    //if ((PPUCTRL & 0x80) != 0)
                    {
                        // Request NMI
                    }
                }
            }

        _lastCpuCycle = cpuCycles;
    }
}