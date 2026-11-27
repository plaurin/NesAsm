namespace NesAsm.Emulator.Mappers;

public class CartMapper1 : Cart
{
    private int _prgBankOffset0;
    private int _prgBankOffset1;
    private int _chrBankOffset0;
    private int _chrBankOffset1;

    private byte _shiftRegister;
    private byte _shiftCount;
    private byte _control = 0x0C;
    private byte _chrBank0;
    private byte _chrBank1;
    private byte _prgBank;

    public CartMapper1(string romPath) 
        : base(romPath)
    {
    }

    private int ChrMode => (_control >> 4) & 0x01;
    private int PrgMode => (_control >> 2) & 0x03;

    public override byte[] GetMapperSaveState()
    {
        var buffer = new byte[]
        {
            _shiftRegister,
            _shiftCount,
            _control,
            _chrBank0,
            _chrBank1,
            _prgBank
        };

        return buffer;
    }

    public override void MapperRestoreState(ReadOnlySpan<byte> state) 
    {
        var index = 0;
        _shiftRegister = state[index++];
        _shiftCount = state[index++];
        _control = state[index++];
        _chrBank0 = state[index++];
        _chrBank1 = state[index++];
        _prgBank = state[index++];

        UpdateMirroring();
        UpdateBanks();
    }

    public override void MapperReset()
    {
        _shiftRegister = 0;
        _shiftCount = 0;
        _control = 0x0C;
        _chrBank0 = 0;
        _chrBank1 = 0;
        _prgBank = 0;
        UpdateMirroring();
        UpdateBanks();
    }

    public override byte ReadPrgMemory(ushort address)
    {
        if (address >= 0x6000 && address <= 0x7FFF)
            return _prgRam[address - 0x6000];
        else if (address >= 0x8000 && address <= 0xBFFF)
        {
            var index = _prgBankOffset0 + (address - 0x8000);
            return PrgRom[index];
        }
        else if (address >= 0xC000 && address <= 0xFFFF)
        {
            var index = _prgBankOffset1 + (address - 0xC000);
            return PrgRom[index];
        }

        return 0;
    }

    public override void WritePrgMemory(ushort address, byte value)
    {
        if (address >= 0x6000 && address <= 0x7FFF)
        {
            _prgRam[address - 0x6000] = value;

        }
        else if (address < 0x6000) { }
        else if ((value & 0x80) != 0)
        {
            _shiftRegister = 0;
            _shiftCount = 0;
            _control |= 0x0C;
            UpdateBanks();
        }
        else
        {
            _shiftRegister = (byte)(_shiftRegister >> 1 | (value & 1) << 4);
            _shiftCount++;

            if (_shiftCount == 5)
            {
                var register = (address >> 13) & 0x03;
                var registerValue = (byte)(_shiftRegister & 0x1F);
                switch (register) 
                {
                    case 0:
                        _control = registerValue;
                        UpdateMirroring();
                        break;
                    case 1: _chrBank0 = registerValue; break;
                    case 2: _chrBank1 = registerValue; break;
                    case 3: _prgBank = registerValue; break;
                }
                _shiftRegister = 0x10;
                _shiftCount = 0;
                UpdateBanks();
            }
        }
    }

    public override byte ReadChrMemory(ushort address)
    {
        if (NbChrBanks == 0)
            return _chrRam[address];
        else
        {
            if (ChrMode == 0)
            {
                return ChrRom[address + _chrBankOffset0];
            }
            else
            {
                if (address < 0x1000)
                    return ChrRom[address + _chrBankOffset0];
                else
                    return ChrRom[address - 0x1000 + _chrBankOffset1];
            }
        }
    }

    public override void WriteChrMemory(ushort address, byte value)
    {
        if (NbChrBanks == 0)
            _chrRam[address] = value;
    }

    private void UpdateMirroring()
    {
        switch (_control & 0x03)
        {
            case 0: Mirroring = MirroringMode.SingleScreenA; break;
            case 1: Mirroring = MirroringMode.SingleScreenB; break;
            case 2: Mirroring = MirroringMode.Vertical; break;
            case 3: Mirroring = MirroringMode.Horizontal; break;
        }
    }

    private void UpdateBanks()
    {
        if (ChrMode == 0) // 8kb bank
        {
            _chrBankOffset0 = (_chrBank0 & 0x1E) * 0x1000;
            _chrBankOffset1 = _chrBankOffset0 + 0x1000;
        }
        else // 2 x 4kb banks
        {
            _chrBankOffset0 = _chrBank0 * 0x1000;
            _chrBankOffset1 = _chrBank1 * 0x1000;
        }

        if (_chrBankOffset0 > ChrSize || _chrBankOffset1 > ChrSize) { /*throw new InvalidOperationException("CHR Bank outside of range");*/ }

        switch (PrgMode)
        {
            case 0: // 32kb at $8000, ignore low bit of bank number
            case 1:
                int bank = _prgBank & 0x0E;
                _prgBankOffset0 = bank * 0x4000;
                _prgBankOffset1 = _prgBankOffset0 + 0x4000;
                break;
            case 2: // fix first bank ar $8000 and switch 16kb at $C000
                _prgBankOffset0 = 0x0000;
                _prgBankOffset1 = _prgBank * 0x4000;
                break;
            case 3: // fix last bank ar $C000 and switch 16kb at $8000
                _prgBankOffset0 = _prgBank * 0x4000;
                _prgBankOffset1 = (NbPrgBanks - 1) * 0x4000;
                break;
        }

        if (_prgBankOffset0 > PrgSize || _prgBankOffset1 > PrgSize) { /*throw new InvalidOperationException("PRG Bank outside of range");*/ }
    }
}