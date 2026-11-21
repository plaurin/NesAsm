using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using NesAsm.Emulator;
using NesAsm.Example.BoxingRPG;
using NesAsm.Example.FPS;
using NesAsm.Example.JumpMan;
using NesAsm.Example.PPUExamples;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NesAsm.UI;
public partial class MainWindow : Window
{
    private readonly List<string> _recentRoms = new();
    private readonly Database _database;
    private readonly SKBitmap _bitmap;
    private byte[]? _screen;
    private WriteableBitmap? _writeableBitmap;
    private readonly DispatcherTimer _timer = new();
    private readonly DispatcherTimer _savedStateTimer = new();
    private readonly TimeSpan _frameDuration = TimeSpan.FromSeconds(1) / 60.0;

    private int _frameNumber;
    private bool _isRendering;
    private readonly SKColor[] _colorPalette;
    private DateTimeOffset _startTime;
    private DateTimeOffset _nextFrameTime;
    private readonly Stopwatch _watch;
    private TimeSpan _lastWatch;
    private int _startFrame;
    private int _fps;

    private NesRunner? _runner;

    const double FrameSpan = 1000 / 60;

    private CancellationTokenSource _cancellationTokenSource = new();

    public MainWindow()
    {
        InitializeComponent();

        var topLevel = TopLevel.GetTopLevel(this);
        topLevel!.KeyDown += OnKeyDown;
        topLevel!.KeyUp += OnKeyUp;

        MainMenu.KeyDown += OnKeyDown;
        MainMenu.IsEnabled = false;

        _database = new Database(GameDataFolder());

        _bitmap = new SKBitmap(256, 240);

        InitMenu();
        //_screen = PPU.GetScreen();

        //writeableBitmap = new WriteableBitmap(
        //    new PixelSize(_bitmap.Width, _bitmap.Height),
        //    new Vector(96, 96),
        //    PixelFormat.Bgra8888,
        //    AlphaFormat.Opaque);

        //Image.Source = writeableBitmap;

        _colorPalette = PPU.Colors.Select(c => new SKColor(c.r, c.g, c.b)).ToArray()!;

        _startTime = DateTimeOffset.Now;
        _nextFrameTime = _startTime;
        _watch = new Stopwatch();
        _watch.Start();

        _timer.Interval = TimeSpan.FromMilliseconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();

        _savedStateTimer.Interval = TimeSpan.FromSeconds(2);
        _savedStateTimer.Tick += SavedStateTimer_Tick;

        var cancellationToken = _cancellationTokenSource.Token;

        //RunGame("Jump Man");
        //RunGame("Vertical Scrolling");
        //RunGame("Boxing Game");
        //RunGame("FPS Game");
        //RunGame(_recentRoms.First());
        RunGame("Resume ROM");

        //Task.Run(() => NesApiCSharp.RunOnce(draw: Draw, gameEntryPoint: PPUExemple.Run)).ConfigureAwait(false);
        //Task.Run(() => NesApiCSharp.RunGame(draw: Draw, reset: GameLoopExemple.Reset, nmi: GameLoopExemple.Nmi)).ConfigureAwait(false);
        //Task.Run(() => NesApiCSharp.RunGame(draw: Draw, reset: ImageLoading.Reset, nmi: ImageLoading.Nmi)).ConfigureAwait(false);
    }

    private string GameDataFolder()
    {
        const string projectFolder = "NesAsm.UI";
        var cur = Directory.GetCurrentDirectory();
        var ind = cur.IndexOf(projectFolder) + projectFolder.Length;
        return Path.Combine(cur[..ind], "GameData");
    }

    private void InitMenu()
    {
        // Recent ROMs
        var recentFilePath = Path.Combine(GameDataFolder(), "RecentRoms.txt");
        if (File.Exists(recentFilePath))
        {
            _recentRoms.AddRange(File.ReadAllLines(recentFilePath));
        }

        foreach (var path in _recentRoms)
        {
            AddRomInMenu(path, RecentRomsMenuItem);
        }

        // Custom ROMs
        var argFilePath = Path.Combine(GameDataFolder(), "args");
        if (File.Exists(argFilePath))
        {
            var customRoms = File.ReadAllLines(argFilePath).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            foreach (var path in customRoms)
            {
                AddRomInMenu(path, CustomRomsMenuItem);
            }
        }

        // Mapper 0
        var mapper0Roms = Path.Combine(GameDataFolder(), "mapper0_nrom.txt");
        foreach (var path in File.ReadAllLines(mapper0Roms))
        {
            AddRomInMenu(path, Mapper0RomsMenuItem);
        }
    }

    private void AddRomInMenu(string romPath, MenuItem menu)
    {
        var menuItem = new MenuItem { Header = Path.GetFileName(romPath) };
        menuItem.Click += (s, e) => RunGame(romPath);

        menu.Items.Add(menuItem);
    }

    private void InitRenderTarget(byte[] target)
    {
        _screen = target;

        _writeableBitmap = new WriteableBitmap(
            new PixelSize(_bitmap.Width, _bitmap.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);

        Image.Source = _writeableBitmap;

        _frameNumber = 0;
    }

    private void RunGame(string gameName)
    {
        MainMenu.IsEnabled = false;

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new();
        var cancellationToken = _cancellationTokenSource.Token;

        void RunCSharpGame(string title, Action<CancellationToken?> reset, Action? nmi)
        {
            Title = title;
            InitRenderTarget(PPU.GetScreen());
            Task.Run(() => NesApiCSharp.RunGame(cancellationToken, draw: Draw, reset, nmi), cancellationToken).ConfigureAwait(false);
        }

        switch (gameName)
        {
            case "Jump Man":
                RunCSharpGame("Jump Man", JumpManGame.Reset, JumpManGame.Nmi);
                break;
            case "Vertical Scrolling":
                RunCSharpGame("Vertical Scrolling", VerticalScrolling.Reset, VerticalScrolling.Nmi);
                break;
            case "Boxing Game":
                RunCSharpGame("Boxing Game", BoxingRPGGame.Reset, BoxingRPGGame.Nmi);
                break;
            case "FPS Game":
                RunCSharpGame("FPS Game", FPSGame.Reset, FPSGame.Nmi);
                break;
            case "Resume ROM":
                var lastRomFile = Path.Combine(GameDataFolder(), "RomPath.txt");
                if (!File.Exists(lastRomFile))
                {
                    MainMenu.IsEnabled = true;
                    return;
                }

                var romPath2 = File.ReadAllText(lastRomFile);

                ChangeGame(romPath2, cancellationToken);
                break;
            default:
                ChangeGame(gameName, cancellationToken);
                break;
        }
    }

    private void ChangeGame(string romPath, CancellationToken cancellationToken)
    {
        _runner = new NesRunner(new Cart(romPath));
        Title = Path.GetFileName(romPath);

        // Recent Roms
        if (!_recentRoms.Remove(romPath))
        {
            AddRomInMenu(romPath, RecentRomsMenuItem);
        }
        _recentRoms.Insert(0, romPath);

        // Cheats
        _database.ChangeCurrentGame(Path.GetFileNameWithoutExtension(romPath));
        CheatsMenuItem.Items.Clear();
        foreach (var cheat in _database.GetCheats())
        {
            var menuItem = new MenuItem
            { 
                Header = cheat.Description,
                IsChecked = cheat.IsActive,
                ToggleType = MenuItemToggleType.CheckBox,
                StaysOpenOnClick = true
            };

            menuItem.Click += (s, e) =>
            {
                if (s is MenuItem menuItem)
                {
                    _database.ToggleCheat(menuItem.Header?.ToString() ?? "Unknown", menuItem.IsChecked);
                    _runner.RefreshCheats(_database.GetActiveCheatsInfo());
                }
            };

            CheatsMenuItem.Items.Add(menuItem);
        }

        _runner.RefreshCheats(_database.GetActiveCheatsInfo());

        // Resume from Save State
        var saveState = _database.GetLastSaveState();
        if (saveState != null)
            _runner.RestoreState(saveState);

        InitRenderTarget(_runner.GetScreen());
        Task.Run(() => _runner.RunGame(cancellationToken, draw: Draw), cancellationToken).ConfigureAwait(false);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.PhysicalKey == PhysicalKey.Escape) ToggleMenu();
        if (e.PhysicalKey == PhysicalKey.Tab) ToggleFastForward();
        if (e.PhysicalKey == PhysicalKey.F5) SaveState();
        if (e.PhysicalKey == PhysicalKey.F6) LoadState();
        if (e.PhysicalKey == PhysicalKey.Backspace) ToggleReverse();
        if (e.PhysicalKey == PhysicalKey.R) ToggleRecording();
        if (e.PhysicalKey == PhysicalKey.P) PlayRecording();

        if (e.PhysicalKey == PhysicalKey.ArrowLeft) InputManager.SetState(0, true);
        if (e.PhysicalKey == PhysicalKey.ArrowRight) InputManager.SetState(1, true);
        if (e.PhysicalKey == PhysicalKey.ArrowUp) InputManager.SetState(2, true);
        if (e.PhysicalKey == PhysicalKey.ArrowDown) InputManager.SetState(3, true);
        if (e.PhysicalKey == PhysicalKey.Z) InputManager.SetState(4, true);
        if (e.PhysicalKey == PhysicalKey.X) InputManager.SetState(5, true);
        if (e.PhysicalKey == PhysicalKey.A) InputManager.SetState(6, true);
        if (e.PhysicalKey == PhysicalKey.S) InputManager.SetState(7, true);
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.PhysicalKey == PhysicalKey.ArrowLeft) InputManager.SetState(0, false);
        if (e.PhysicalKey == PhysicalKey.ArrowRight) InputManager.SetState(1, false); 
        if (e.PhysicalKey == PhysicalKey.ArrowUp) InputManager.SetState(2, false);
        if (e.PhysicalKey == PhysicalKey.ArrowDown) InputManager.SetState(3, false);
        if (e.PhysicalKey == PhysicalKey.Z) InputManager.SetState(4, false);
        if (e.PhysicalKey == PhysicalKey.X) InputManager.SetState(5, false);
        if (e.PhysicalKey == PhysicalKey.A) InputManager.SetState(6, false);
        if (e.PhysicalKey == PhysicalKey.S) InputManager.SetState(7, false);
    }

    private void ToggleMenu()
    {
        MainMenu.IsEnabled = !MainMenu.IsEnabled;
        MainMenu.Close();
        if (_runner != null) _runner.IsPaused = MainMenu.IsEnabled;
        PauseOverlay.IsVisible = MainMenu.IsEnabled;
    }

    private void ToggleFastForward() 
    {
        if (_runner != null)
        {
            if (_runner.FastForwardFrameSkip == 0)
                _runner.FastForwardFrameSkip = 8;
            else
                _runner.FastForwardFrameSkip = 0;
        }
    }

    private void ToggleReverse()
    {
        if (_runner != null) _runner.IsReverse = !_runner.IsReverse;
    }

    private void ToggleRecording()
    {
        if (_runner == null) return;
        if (_runner.IsRecording)
            StopRecording();
        else 
            StartRecording();
    }

    private void StartRecording()
    {
        var state = _runner?.GetState();
        if (state != null)
        {
            _runner?.StartRecording();
            _database.StartRecording(state, _runner!.Frame);
        }
    }

    private void StopRecording()
    {
        var startFrameIndex = _database.GetRecordingFrameIndex();
        if (startFrameIndex != null)
        {
            var inputs = _runner!.StopRecording();
            _database.StopRecording(inputs.ToArray(), _runner!.Frame);
        }
    }

    private void PlayRecording()
    {
        var recording = _database.Recordings.First();
        _runner?.PlayRecording(recording.State.ToArray(), recording.Inputs);
    }

    private void SaveState()
    {
        var state = _runner?.GetState();
        if (state != null)
        {
            _database.SaveGameState(state, _runner!.Frame);

            SavedStateOverlay.IsVisible = true;
            _savedStateTimer.Start();
        }
    }

    private void SavedStateTimer_Tick(object? sender, EventArgs e)
    {
        SavedStateOverlay.IsVisible = false;
        _savedStateTimer.Stop();
    }

    private void LoadState()
    {
        if (_runner != null) _runner.IsPaused = true;
        LoadOverlay.IsVisible = true;

        //SaveStateView.Execute(_runner!.RomPath, _database.SaveStates.ElementAt(0), frameSkip: 2);
        //SaveStateView2.Execute(_runner!.RomPath, _database.SaveStates.ElementAt(1), frameSkip: 2);
        //SaveStateView3.Execute(_runner!.RomPath, _database.SaveStates.ElementAt(2), frameSkip: 2);
        //SaveStateView4.Execute(_runner!.RomPath, _database.SaveStates.ElementAt(3), frameSkip: 2);
        SaveStateView.Execute(_runner!.RomPath, recording: _database.Recordings.ElementAt(0), frameSkip: 0, scale: 1);
        SaveStateView2.Execute(_runner!.RomPath, recording: _database.Recordings.ElementAt(1), frameSkip: 3, scale: 0.5f);
        SaveStateView3.Execute(_runner!.RomPath, recording: _database.Recordings.ElementAt(0), frameSkip: 3, scale: 0.25f);
        SaveStateView4.Execute(_runner!.RomPath, recording: _database.Recordings.ElementAt(1), frameSkip: 3, scale: 2);

    }

    private string GetInputStateString()
    {
        var state = InputManager.State;

        return $"{((state & 0x80) == 0x80 ? "R" : "-")}{((state & 0x40) == 0x40 ? "L" : "-")}{((state & 0x20) == 0x20 ? "D" : "-")}{((state & 0x10) == 0x10 ? "U" : "-")}{((state & 0x08) == 0x08 ? "St" : "--")}{((state & 0x04) == 0x04 ? "Se" : "--")}{((state & 0x02) == 0x02 ? "B" : "-")}{((state & 0x01) == 0x01 ? "A" : "-")}";
    }

    public void Exit()
    {
        _cancellationTokenSource.Cancel();

        if (_runner != null)
        {
            var romPath = _runner.RomPath;
            File.WriteAllText(Path.Combine(GameDataFolder(), "RomPath.txt"), romPath);

            var state = _runner.GetState();
            File.WriteAllBytes(Path.Combine(GameDataFolder(), "State.bin"), state);
            _database.SaveLastStateOnExit(state, _runner.Frame);

            File.WriteAllLines(Path.Combine(GameDataFolder(), "RecentRoms.txt"), _recentRoms);

            _database.PersistActiveCheats();
        }

        Close();
    }

    bool _canDraw = false;
    bool _running = false;

    void Draw()
    {
        while(!_canDraw)
        {
            // Wait for the next frame timing
            Task.Delay(1).Wait();
            PPU.Log("Waiting can draw");
        }
        //PPU.Log("CanDraw next frame");
        _isRendering = true;

        Dispatcher.UIThread.Post(() => DrawLoop());
        _canDraw = false;

        var frameTime = _watch.Elapsed - _lastWatch;
        if (frameTime.TotalMilliseconds < FrameSpan) Task.Delay((int)(FrameSpan - frameTime.TotalMilliseconds)).Wait();

        while (_isRendering)
        {
            // Wait for the next frame to be ready to draw
            //Task.Delay(1).Wait();
            //PPU.Log("Waiting is rendering");
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!_running)
        {
            _running = true;
            //NesApiCSharp.RunGame(draw: Draw, reset: GameLoopExemple.Reset, nmi: GameLoopExemple.Nmi);
            return;
        }

        //NesApiCSharp.RunOnce(Game1.Reset);

        if (_isRendering || _canDraw) return;
        if (DateTimeOffset.Now < _nextFrameTime) return;
        _canDraw = true;
        //DrawLoop();
        _nextFrameTime += _frameDuration;
    }

    private void DrawLoop()
    {
        if (_screen == null || _writeableBitmap == null || _runner == null) return;

        //PPU.Log($"Start to draw UI frame {_frameNumber}");
        var pixelsPtr = _bitmap.GetPixels();
        unsafe
        {
            byte* ptr = (byte*)pixelsPtr.ToPointer();
            for (int y = 0; y < 240; y++)
                for (int x = 0; x < 256; x++)
                {
                    var i = x + y * 256;
                    ptr[i * 4] = PPU.Colors[_screen[i]].b;
                    ptr[i * 4 + 1] = PPU.Colors[_screen[i]].g;
                    ptr[i * 4 + 2] = PPU.Colors[_screen[i]].r;
                    ptr[i * 4 + 3] = 255;
                }
        }

        using (var frameBuffer = _writeableBitmap.Lock())
        {
            unsafe
            {
                void* dataStart = (void*)frameBuffer.Address;
                Span<byte> buffer = new(dataStart, 256 * 240 * 4); // assume each pixel is 1 byte in size and my image has 4 channels
                //_bitmap.Bytes.CopyTo(buffer);
                _bitmap.GetPixelSpan().CopyTo(buffer);
            }
        }

        //PPU.Log($"End draw UI frame {_frameNumber}");

        Image.InvalidateVisual();
        //Task.Delay(300).Wait();

        _isRendering = false;

        //_x += 1;
        //if (_x > 200) _x = 10;

        var frameTime = _watch.Elapsed - _lastWatch;
        PPU.Log($"Frame time for {_frameNumber} : {frameTime} ({frameTime.TotalMilliseconds - (1000 / 60):0000})");
        _lastWatch = _watch.Elapsed;

        _frameNumber++;

        var inputState = GetInputStateString();
        var recordingState = _runner.IsRecording ? "Recording" : _runner.IsReplaying ? "Replaying" : "";
        var title = $"FPS: {_fps:N0} {inputState} {recordingState}";
        if (FrameCounter.Content == null || (string)FrameCounter.Content != title)
            FrameCounter.Content = title;

        if (DateTimeOffset.Now.Second != _startTime.Second)
        {
            _fps = _frameNumber - _startFrame;
            _startFrame = _frameNumber;
            _startTime = DateTimeOffset.Now;
        }
    }
}