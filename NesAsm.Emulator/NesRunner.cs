namespace NesAsm.Emulator;

public class NesRunner
{
    private readonly Cart _cart;
    private readonly NesMemory _memory;
    private readonly PPUInstance _ppu;
    private readonly CPU _cpu;

    private readonly byte[] _screen = new byte[256 * 240];
    private readonly byte[] _pixelMask = new byte[256 * 240];
    public NesRunner(Cart cart)
    {
        _cart = cart;
        _memory = new NesMemory(cart);
        _ppu = new PPUInstance(_memory);
        _memory.SetPPU(_ppu);
        _cpu = new CPU(_memory);
        _memory.SetCPU(_cpu);
    }

    public byte[] GetScreen() => _screen;

    public int Frame => _ppu.Frame;

    public string RomPath => _cart.RomPath;

    public bool IsPaused { get; set; }
    public bool IsReverse { get; set; }
    public int FastForwardFrameSkip { get; set; } = 0;
    public int FrameSkip { get; set; } = 0;

    private readonly byte[][] _states = new byte[1000][];
    private int _saveIndex;
    private int _saveTotal;

    private readonly List<(byte, byte)> _inputRecords = [];
    private readonly List<byte> _recordingInputs = [];
    private byte _lastInputState;
    private int _sameInputStateFrameCount;

    public byte[] GetState()
    {
        var buffer = new List<byte>();

        buffer.AddRange(_memory._ram);
        buffer.AddRange(_cpu.GetSaveState());
        buffer.AddRange(_ppu.GetSaveState());
        buffer.AddRange(_cart.GetSaveState());

        return buffer.ToArray();
    }

    public void RestoreState(ReadOnlySpan<byte> state)
    {
        var toRestore = state.ToArray();
        SetVBlankAction(() =>
        {
            _memory._ram = toRestore[0..0x800].ToArray();
            _cpu.RestoreState(toRestore[0x800..0x807]);
            _ppu.RestoreState(toRestore[0x807..0x1947]);
            _cart.RestoreState(toRestore[0x1947..]);
        });
    }

    public bool IsRecording { get; private set; }
    public bool IsReplaying { get; private set; }
    public void StartRecording()
    {
        SetVBlankAction(() =>
        {
            IsRecording = true;
            _recordingInputs.Clear();

            _inputRecords.Add((_lastInputState, (byte)_sameInputStateFrameCount));
            _sameInputStateFrameCount = 0;
            _lastInputState = InputManager.State;
        });
    }

    public byte[] StopRecording()
    {
        IsRecording = false;

        return _recordingInputs.ToArray();
    }

    private byte[]? _replayState;
    private byte[]? _replayInput;
    private int _replayIndex;
    private int _replayFrameIndex;
    private bool _isEndlessReplay;

    public void PlayRecordingInLoop(ReadOnlySpan<byte> state, byte[] inputs)
    {
        _isEndlessReplay = true;
        PlayRecording(state, inputs);
    }

    public void PlayRecording(ReadOnlySpan<byte> state, byte[] inputs)
    {
        IsReplaying = true;

        _replayState = state.ToArray();
        _replayInput = inputs;
        _replayIndex = 0;
        _replayFrameIndex = 0;

        RestoreState(state);
    }

    public (byte[] State, byte[] Inputs) GetRecording(int nbFrames)
    {
        // State
        nbFrames = Math.Min(nbFrames, _saveTotal);
        var index = _saveIndex - nbFrames;
        if (index < 0) index += 1000;

        var state = _states[index];

        // Inputs
        var inputs = new List<byte>();
        var cnt = Math.Min(_sameInputStateFrameCount, nbFrames);
        inputs.Add((byte)cnt);
        inputs.Add(_lastInputState);

        nbFrames -= cnt;
        index = _recordingInputs.Count - 1;
        while (nbFrames > 0 && _recordingInputs.Count > 0)
        {
            cnt = Math.Min(_recordingInputs[index], nbFrames);
            inputs.Add((byte)cnt);
            inputs.Add(_recordingInputs[index - 1]);

            nbFrames -= cnt;
            index -= 2;

            if (index < 0) break;
        }

        return (state, inputs.Reverse<byte>().ToArray());
    }

    public void RefreshCheats(IEnumerable<(ushort Address, byte Value, byte? Compare)> activeCheatsInfo) => _memory.RefreshCheats(activeCheatsInfo);

    private Action? _vblankAction;
    private void SetVBlankAction(Action action) => _vblankAction = action;

    public Task RunGame(CancellationToken cancellationToken, Action draw)
    {
        try
        {
            if (_vblankAction != null)
            {
                _vblankAction.Invoke();
                _vblankAction = null;
            }
            else
            {
                _cpu.Init();
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                if (IsPaused) continue;

                if (_cpu.PC == 0x8624) { }

                if (IsReverse)
                {
                    ProcessReverse(draw);
                    continue;
                }

                var ins = _cpu.RunNextInstruction();

                _ppu.RunToCycle(_cpu.Cycles);

                if (_ppu.VBlankStart)
                {
                    ProcessVBlank();

                    SaveFrameState();

                    UpdateScreen(draw);

                    SetNextFrameSprite0Hit();

                    UpdateInput();
                }

                if (_ppu.NmiRequested)
                {
                    _ppu.NmiRequested = false;

                    _cpu.RunNmi();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return Task.CompletedTask;
    }

    public void ProcessReverse(Action draw)
    {
        _saveIndex--;
        if (_saveIndex < 0) _saveIndex = 999;
        var state = _states[_saveIndex];
        RestoreState(state);

        UpdateScreen(draw);

        SetNextFrameSprite0Hit();
    }

    public void ProcessVBlank()
    {
        _ppu.VBlankStart = false;

        if (_vblankAction != null)
        {
            _vblankAction.Invoke();
            _vblankAction = null;
        }
    }

    private void SaveFrameState()
    {
        if (!_isEndlessReplay)
        {
            _states[_saveIndex++] = GetState();
            if (_saveIndex >= 1000) _saveIndex = 0;
            _saveTotal++;
        }
    }

    private void UpdateScreen(Action draw)
    {
        if (_ppu.Frame % (FastForwardFrameSkip + 1) == 0)
        {
            DrawScreen();

            if (_ppu.Frame % (FrameSkip + 1) == 0)
                draw.Invoke();
        }
    }

    private void UpdateInput()
    {
        if (IsReplaying && _replayInput != null)
        {
            if (_replayFrameIndex > _replayInput[_replayIndex + 1])
            {
                _replayIndex += 2;
                _replayFrameIndex = 0;
            }

            if (_replayIndex >= _replayInput.Length)
            {
                if (_isEndlessReplay)
                    PlayRecording(_replayState, _replayInput);
                else
                    IsReplaying = false;
            }
            else
            {
                InputManager.SetState(_replayInput[_replayIndex]);
                _replayFrameIndex++;
            }
            //if (_sameInputStateFrameCount > _inputStates[_inputStateIndex].FrameCount)
            //{
            //    _inputStateIndex++;
            //    _sameInputStateFrameCount = 0;
            //}

            //InputManager.SetState(_inputStates[_inputStateIndex].State);
            //_sameInputStateFrameCount++;
        }
        else
        {
            InputManager.UpdateState();
        }

        if (!IsReplaying)
        {
            var inputState = InputManager.State;
            if (inputState == _lastInputState && _sameInputStateFrameCount < 256)
            {
                _sameInputStateFrameCount++;
            }
            else
            {
                //_inputStates[_inputStateIndex++] = (_lastInputState, (byte)_sameInputStateFrameCount);
                _inputRecords.Add((_lastInputState, (byte)_sameInputStateFrameCount));
                //if (IsRecording) 
                { 
                    _recordingInputs.Add(_lastInputState);
                    _recordingInputs.Add((byte)_sameInputStateFrameCount);
                }
                _sameInputStateFrameCount = 0;
                _lastInputState = inputState;
            }
        }
    }

    private const byte SpriteYOffset = 1; // The PPU adds 1 to the sprite Y position, so we need to subtract it when setting the sprite Y position.

    private void SetNextFrameSprite0Hit()
    {
        if ((_ppu.PpuMask & 0x18) == 0x18)
        {
            for (ushort y = 0; y < 240; y++)
                for (ushort x = 0; x < 256; x++)
                    if ((_pixelMask[x + y * 256] & 0x03) == 0x03)
                    {
                        // first hit
                        _ppu.SetSprite0HitAt(y, x);
                    }
        }
    }

    public void DrawScreen(byte startScanline = 0, byte endScanline = 239)
    {
        bool setbackgroundcolor = true;
        bool drawSprites = (_ppu.PpuMask & 0x10) != 0;
        bool drawBackground = (_ppu.PpuMask & 0x08) != 0;

        if (setbackgroundcolor)
            for (int y = startScanline; y <= endScanline; y++)
                for (int x = 0; x < 256; x++)
                {
                    _screen[x + y * 256] = _ppu.BackgroundColorIndex;
                    _pixelMask[x + y * 256] = 0;
                }

        byte GetSpriteY(int sprite) => _ppu.Oam[sprite * 4 + 0];
        byte GetSpriteX(int sprite) => _ppu.Oam[sprite * 4 + 3];
        bool IsSpriteBehindBackground(int sprite) => (_ppu.Oam[sprite * 4 + 2] & 0x20) != 0;

        void drawSprite(int index)
        {
            var spriteX = GetSpriteX(index);
            var spriteY = GetSpriteY(index) + SpriteYOffset;
            for (int y = 0; y < 8; y++)
                if (spriteY + y >= startScanline && spriteY + y <= endScanline)
                    for (int x = 0; x < 8; x++)
                    {
                        if (spriteX + x < 256)
                        {
                            var screenIndex = (spriteX + x) + (spriteY + y) * 256;
                            if (screenIndex < _screen.Length)
                                _screen[screenIndex] = DrawSpritePixel(index, x, y, screenIndex);
                        }
                    }
        }

        if (drawSprites)
            for (int i = 0; i < 64; i++)
                if (IsSpriteBehindBackground(i))
                    drawSprite(i);

        if (drawBackground)
            for (int y = startScanline; y <= endScanline; y++)
                for (int x = 0; x < 256; x++)
                {
                    var screenIndex = x + y * 256;
                    _screen[screenIndex] = DrawBackgroundPixel(x, y, screenIndex);
                }

        if (drawSprites)
            for (int i = 0; i < 64; i++)
                if (!IsSpriteBehindBackground(i) && GetSpriteY(i) < 240)
                    drawSprite(i);
    }

    private int BackgroundPatternTableIndex => ((_ppu.PpuCtrl & 0x10) != 0) ? 1 : 0;
    private int SpritePatternTableIndex => ((_ppu.PpuCtrl & 0x08) != 0) ? 1 : 0;
    private byte ScrollX => _ppu.ScrollX;
    private byte ScrollY => _ppu.ScrollY;
    private bool VerticalMirroring = true; // Default mirroring mode, can be changed by the game.

    private byte DrawBackgroundPixel(int x, int y, int screenIndex)
    {
        if (x == 24 && y == 16) { } // Debug
        
        var colorIndexIfTransparent = _screen[screenIndex];

        (int nametableX, int nametableY) GetNametablePosition(int screenX, int screenY) => (screenX / 8, screenY / 8);
        (int patternX, int patternY) GetPatternTablePosition(int screenX, int screenY) => (screenX % 8, screenY % 8);
        (int attributeX, int attributeY) GetAttributeTablePosition(int nametableX, int nametableY) => (nametableX / 4, nametableY / 4);
        byte GetPatternIndex(int nametableIndex, int nametableX, int nametableY) => _ppu.GetVram((ushort)(0x2000 + nametableIndex * 0x400 + nametableY * 32 + nametableX));
        byte GetAttributeTable(int tableIndex, int x, int y) => _ppu.GetVram((ushort)(0x23C0 + tableIndex * 0x400 + y * 8 + x));
        byte GetBackgroundPaletteColor(int paletteIndex, int colorIndex) => _ppu.GetVram((ushort)(0x3F00 + paletteIndex * 4 + colorIndex));

        var nametableIndex = _ppu.ScrollXRecord[y].NametableIndex;
        var (nametableX, nametableY) = GetNametablePosition(x + _ppu.ScrollXRecord[y].ScrollValue, y + ScrollY);
        if (nametableX > 31)
        {
            if (VerticalMirroring) nametableIndex = (byte)((nametableIndex + 1) % 2);
            nametableX -= 32;
        }

        if (nametableY > 29)
        {
            if (!VerticalMirroring) nametableIndex = (byte)((nametableIndex + 2) % 4);
            nametableY -= 30;
        }

        var patternIndex = GetPatternIndex(nametableIndex, nametableX, nametableY);

        if (patternIndex != 0x24) { } // Debug

        var (patternX, patternY) = GetPatternTablePosition(x + _ppu.ScrollXRecord[y].ScrollValue, y + ScrollY);
        var colorIndex = GetPatternTableColorIndex(BackgroundPatternTableIndex, (patternIndex % 16) * 8 + patternX, (patternIndex / 16) * 8 + patternY);

        if (colorIndex == 0)
        {
            return colorIndexIfTransparent;
        }

        // Sprite 0 hit detection
        if (colorIndex != 0)
        {
            _pixelMask[screenIndex] |= 0x01;
        }

        var (attributeX, attributeY) = GetAttributeTablePosition(nametableX, nametableY);
        var attributeValue = GetAttributeTable(nametableIndex, attributeX, attributeY);
        if (attributeValue != 0) { } // Debug
        var bitIndex = (nametableX % 2) * 0x04 + (nametableY % 2) * 0x10 + 1;
        var bitIndex2 = (0x03 << ((nametableX / 2 % 2) * 2 + (nametableY / 2 % 2) * 4));
        var bitIndex3 = (nametableX / 2 % 2) * 2 + (nametableY / 2 % 2) * 4;
        var bit0 = attributeValue & bitIndex;
        var bit1 = attributeValue & (bitIndex << 1);
        var r = attributeValue >> bitIndex3 & 0x03;
        var paletteIndex = r;

        return GetBackgroundPaletteColor(paletteIndex, colorIndex);
    }

    private byte DrawSpritePixel(int spriteIndex, int x, int y, int screenIndex)
    {
        var colorIndexIfTransparent = _screen[screenIndex];

        // TODO Move up callstack
        byte GetSpriteTileIndex(int sprite) => _ppu.Oam[sprite * 4 + 1];
        byte GetSpritePaletteIndex(int sprite) => (byte)(_ppu.Oam[sprite * 4 + 2] & 0x03);
        bool IsSpriteFlippedHorizontally(int sprite) => (_ppu.Oam[sprite * 4 + 2] & 0x40) != 0;
        bool IsSpriteFlippedVertically(int sprite) => (_ppu.Oam[sprite * 4 + 2] & 0x80) != 0;
        byte GetSpritePaletteColor(int paletteIndex, int colorIndex) => _ppu.Palettes[0x10 + paletteIndex * 0x04 + colorIndex];

        var patternIndex = GetSpriteTileIndex(spriteIndex);

        var actualX = IsSpriteFlippedHorizontally(spriteIndex) ? 7 - x : x;
        var actualY = IsSpriteFlippedVertically(spriteIndex) ? 7 - y : y;

        var colorIndex = GetPatternTableColorIndex(SpritePatternTableIndex, (patternIndex % 16) * 8 + actualX, (patternIndex / 16) * 8 + actualY);
        if (colorIndex == 0)
        {
            return colorIndexIfTransparent;
        }

        // Sprite 0 Hit detection
        if (spriteIndex == 0 && colorIndex != 0)
        {
            _pixelMask[screenIndex] |= 0x02;
        }

        var paletteIndex = GetSpritePaletteIndex(spriteIndex);

        return GetSpritePaletteColor(paletteIndex, colorIndex);
    }

    private byte GetPatternTableColorIndex(int tableIndex, int x, int y)
    {
        var tx = (x / 8) * 0x10;
        var ty = (y / 8) * 0x100;
        var plane0 = _ppu.GetVram((ushort)(tableIndex * 0x1000 + tx + ty + y % 8));
        var plane1 = _ppu.GetVram((ushort)(tableIndex * 0x1000 + tx + ty + y % 8 + 8));
        var bit0 = plane0 >> (7 - x % 8) & 1;
        var bit1 = plane1 >> (7 - x % 8) & 1;
        return (byte)(bit0 | bit1 << 1);
    }
}
