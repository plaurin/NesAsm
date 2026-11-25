using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using NesAsm.Emulator;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NesAsm.UI;

public partial class SaveStateView : UserControl
{
    private NesRunner? _runner;
    private int _frameSkip;
    private bool _isOneFrame;
    private readonly SKBitmap _bitmap;
    private byte[]? _screen;
    private WriteableBitmap? _writeableBitmap;
    private readonly DispatcherTimer _timer = new();

    public SaveStateView()
    {
        InitializeComponent();

        _bitmap = new SKBitmap(256, 240);
    }

    public void Execute(string romPath, SaveState? saveState = null, Recording? recording = null, int frameSkip = 0, float scale = 1.0f, bool isOneFrame = false)
    {
        SavePreview.Width = 256 * scale;
        SavePreview.Height = 240 * scale;
        if (scale > 1.0f)
            RenderOptions.SetBitmapInterpolationMode(SavePreview, BitmapInterpolationMode.None);

        _runner = new NesRunner(Cart.Create(romPath))
        {
            FrameSkip = frameSkip
        };

        if (saveState != null)
            _runner.RestoreState(saveState.State);

        if (recording != null)
            _runner.PlayRecordingInLoop(recording.State, recording.Inputs);

        _frameSkip = frameSkip;
        _isOneFrame = isOneFrame;

        _screen = _runner.GetScreen();

        _writeableBitmap = new WriteableBitmap(
            new PixelSize(_bitmap.Width, _bitmap.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);

        SavePreview.Source = _writeableBitmap;

        _watch.Start();

        _timer.Interval = TimeSpan.FromMilliseconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();

        Task.Run(() => _runner.RunGame(CancellationToken.None, draw: Draw));
    }

    bool _canDraw = false;
    bool _running = false;
    private bool _isRendering;
    private readonly Stopwatch _watch = new();
    private TimeSpan _lastWatch;
    const double FrameSpan = 1000 / 60;
    private readonly TimeSpan _frameDuration = TimeSpan.FromSeconds(1) / 60.0;
    private DateTimeOffset _nextFrameTime;

    private int frameCount;
    void Draw()
    {
        while (!_canDraw)
        {
            // Wait for the next frame timing
            Task.Delay(1).Wait();
            PPU.Log("Waiting can draw");
        }
        _isRendering = true;

        Dispatcher.UIThread.Post(() => DrawLoop());

        _canDraw = false;

        var frameTime = _watch.Elapsed - _lastWatch;
        if (frameTime.TotalMilliseconds < FrameSpan) Task.Delay((int)(FrameSpan - frameTime.TotalMilliseconds)).Wait();

        frameCount++;
        if (_isOneFrame && frameCount == 2) _runner!.IsPaused = true;

        while (_isRendering)
        {
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!_running)
        {
            _running = true;
            return;
        }

        if (_isRendering || _canDraw) return;
        if (DateTimeOffset.Now < _nextFrameTime) return;
        _canDraw = true;
        _nextFrameTime += _frameDuration;
    }

    private void DrawLoop()
    {
        if (_screen == null || _writeableBitmap == null || _runner == null) return;

        if (_runner.Frame % (_frameSkip + 1) == 0)
        {
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
                    _bitmap.GetPixelSpan().CopyTo(buffer);
                }
            }

            SavePreview.InvalidateVisual();
        }

        _isRendering = false;

        var frameTime = _watch.Elapsed - _lastWatch;
        _lastWatch = _watch.Elapsed;
    }
}