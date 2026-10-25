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

    private readonly NesMemory _memory;
    private readonly byte[] _oam = new byte[256];
    private readonly byte[] _vram = new byte[0x4000];

    private byte _ppuCtrl;
    private byte _ppuMask;
    private byte _ppuStatus;
    private byte _oamAddr;
    private byte _ppuScrollX;
    private byte _ppuScrollY;

    private readonly (int NametableIndex, byte ScrollValue)[] _scrollXRecord = new (int, byte)[256];
    private void UpdateScrollPosition()
    {
        var line = _scanline >= 240 ? 0 : _scanline + 1;
        _scrollXRecord[line] = (_ppuCtrl & 0x03, _ppuScrollX);
    }

    private ushort _ppuAddr;
    private byte _ppuData;

    private bool _addrLatch;
    private bool _scrollLatch;

    private long _lastCpuCycle;
    private int _scanline;
    private int _scanlineCycle;
    private int _frame;

    public PPUInstance(NesMemory memory)
    {
        _memory = memory;
    }

    public int Frame => _frame;

    public override string ToString() => $"F:{_frame} S:{_scanline} C:{_scanlineCycle}";

    public bool VBlankStart { get; set; }
    public bool NmiRequested { get; set; }

    public byte BackgroundColorIndex => _vram[0x3F00];
    public byte[] Oam => _oam;
    public byte PpuCtrl => _ppuCtrl;
    public byte PpuMask => _ppuMask;
    public byte ScrollX => _ppuScrollX;
    public byte ScrollY => _ppuScrollY;

    public (int NametableIndex, byte ScrollValue)[] ScrollXRecord => _scrollXRecord;

    public int Scanline => _scanline;
    public int ScanlineCycle => _scanlineCycle;

    private int? _sprite0Scanline = null;
    private int? _sprite0ScanlineCycle = null;

    public ReadOnlySpan<byte> Palette => _vram.AsSpan()[0x3F00..0x3F20];

    private byte _ppuDataBuffer = 0x0;

    public byte ReadRegister(ushort address)
    {
        byte result = 0x0;

        switch (address)
        {
            case PPUCTRL:
                return _ppuCtrl;
            case PPUSTATUS:
                result = _ppuStatus;
                _ppuStatus &= 0x3F;
                _addrLatch = false;
                return result;
            case OAMDATA:
                return _oam[_oamAddr];
            case PPUDATA:
                result = _ppuDataBuffer;
                _ppuDataBuffer = GetVram(_ppuAddr);
                if (_ppuAddr >= 0x3F00) result = _ppuDataBuffer;
                _ppuAddr += ((_ppuCtrl & 0x04) != 0) ? (ushort)32 : (ushort)1;
                return result;
        }

        return 0;
    }

    public void WriteRegister(ushort address, byte value)
    {
        switch (address)
        {
            case PPUCTRL:
                _ppuCtrl = value;
                UpdateScrollPosition();
                break;
            case PPUMASK:
                _ppuMask = value;
                break;
            case PPUSTATUS:
                _ppuStatus &= 0x7F;
                break;
            case OAMADDR:
                _oamAddr = value;
                break;
            case OAMDATA:
                _oam[_oamAddr++] = value;
                break;
            case PPUSCROLL:
                if (value != 0) { } // Debug
                if (!_scrollLatch)
                {
                    _ppuScrollX = value;
                    UpdateScrollPosition();
                }
                else _ppuScrollY = value;
                _scrollLatch = !_scrollLatch;
                break;
            case PPUADDR:
                _ppuAddr = _addrLatch ? (ushort)((_ppuAddr & 0xFF00) | value) : (ushort)((_ppuAddr & 0x00FF) | (ushort)(value << 8));
                _addrLatch = !_addrLatch;
                //UpdateScrollPosition();
                break;
            case PPUDATA:
                _ppuData = value;
                WriteVRAM(_ppuAddr, _ppuData);
                _ppuAddr += ((_ppuCtrl & 0x04) != 0) ? (ushort)32 : (ushort)1;
                break;
            default:
                throw new NotImplementedException($"PPU Register write not implemented for address {address:X4}");
        }
    }

    public void WriteOAMDMA(byte value)
    {
        var baseAddress = value << 8;
        for (int i = 0; i < 256; i++)
        {
            var t = _memory.Read((ushort)(baseAddress + i));
            _oam[_oamAddr++] = t;
        }
    }

    public byte GetVram(ushort address)
    {
        if (address >= 0x3F00 && address < 0x3F20) { } // Debug

        if (address >= 0x0 && address <= 0x1FFF)
            return _memory.Cart.ReadChrMemory(address);
        else if (address >= 0x2000 && address <= 0x3EFF)
            return _vram[address];
        else if (address >= 0x3F00 && address <= 0x3FFF)
        {
            int mirroredAddress = address & 0x1F;
            if ((mirroredAddress & 0x03) == 0x00) mirroredAddress = 0x00;
            return _vram[0x3F00 | mirroredAddress];
        }
        else
            throw new InvalidOperationException($"{address:X4} out of range");
    }

    public void WriteVRAM(ushort address, byte value)
    {
        if (address == 0x3F10 && address < 0x3F20) { } // Debug

        if (address <= 0x1FFF)
            _memory.Cart.WriteChrMemory(address, value);
        else if (address >= 0x2000 && address <= 0x3EFF)
            _vram[address] = value;
        else if (address >= 0x3F00 && address <= 0x3FFF)
        {
            int mirroredAddress = address & 0x1F;
            if ((mirroredAddress & 0x03) == 0x00 && mirroredAddress >= 0x10) mirroredAddress -= 0x10;
            _vram[0x3F00 | mirroredAddress] = value;
        }
        else
            throw new InvalidOperationException($"{address:X4} out of range");
    }

    public void SetSprite0HitAt(int scanline, int x)
    {
        _sprite0Scanline = scanline;
        _sprite0ScanlineCycle = x * 3;
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
                    _scanline++;

                    if (_scanline < 240)
                        _scrollXRecord[_scanline + 1] = _scrollXRecord[_scanline];
                    else if (_scanline == 240)
                        _scrollXRecord[0] = _scrollXRecord[_scanline];

                    if (_scanline == TotalScanlines)
                    {
                        _frame++;
                        _scanline = 0;
                    }
                }

                if (_scanline == 241 && _scanlineCycle == 0)
                {
                    VBlankStart = true;
                    _ppuStatus |= 0x80;
                    if ((_ppuCtrl & 0x80) != 0)
                    {
                        NmiRequested = true;
                    }
                }

                if (_sprite0Scanline.HasValue && _sprite0ScanlineCycle.HasValue &&_scanline == _sprite0Scanline.Value && _scanlineCycle >= _sprite0ScanlineCycle.Value)
                {
                    _ppuStatus |= 0x40;
                }
            }

        _lastCpuCycle = cpuCycles;
    }
}