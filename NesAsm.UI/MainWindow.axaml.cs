using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using NesAsm.Emulator;
using NesAsm.Example.JumpMan;
using SkiaSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NesAsm.UI;
public partial class MainWindow : Window
{
    private readonly SKBitmap _bitmap;
    private readonly SKCanvas _canvas2;
    private readonly byte[] _screen;
    private readonly DispatcherTimer _timer = new();
    private readonly WriteableBitmap writeableBitmap;
    private readonly TimeSpan _frameDuration = TimeSpan.FromSeconds(1) / 60.0;

    private int _x;
    private int _frameNumber;
    private bool _isRendering;
    private SKColor[] _colorPalette;

    private DateTimeOffset _startTime;
    private DateTimeOffset _nextFrameTime;
    private int _startFrame;
    private int _fps;

    public MainWindow()
    {
        InitializeComponent();

        _bitmap = new SKBitmap(256, 240);
        _canvas2 = new SKCanvas(_bitmap);
        _screen = PPU.GetScreen();
        _x = 10;

        writeableBitmap = new WriteableBitmap(
            new PixelSize(_bitmap.Width, _bitmap.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);
        
        Image.Source = writeableBitmap;

        _colorPalette = PPU.Colors.Select(c => new SKColor(c.r, c.g, c.b)).ToArray()!;

        _startTime = DateTimeOffset.Now;
        _nextFrameTime = _startTime;

        _timer.Interval = TimeSpan.FromMilliseconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();

        //Task.Run(() => NesApiCSharp.RunOnce(draw: Draw, gameEntryPoint: PPUExemple.Run)).ConfigureAwait(false);
        //Task.Run(() => NesApiCSharp.RunGame(draw: Draw, reset: GameLoopExemple.Reset, nmi: GameLoopExemple.Nmi)).ConfigureAwait(false);
        //Task.Run(() => NesApiCSharp.RunGame(draw: Draw, reset: ImageLoading.Reset, nmi: ImageLoading.Nmi)).ConfigureAwait(false);
        Task.Run(() => NesApiCSharp.RunGame(draw: Draw, reset: JumpManGame.Reset, nmi: JumpManGame.Nmi)).ConfigureAwait(false);
    }

    //public override void Render(DrawingContext context)
    //{
    //    context.DrawEllipse(Brushes.Red, new Pen(Brushes.Red), new Rect(10, 10, 10, 10) );
    //    context.DrawImage(writeableBitmap, new Rect(0, 0, writeableBitmap.PixelSize.Width, writeableBitmap.PixelSize.Height));
    //}

    bool _canDraw = false;
    bool _running = false;

    void Draw()
    {
        while(!_canDraw)
        {
            // Wait for the next frame to be ready
            Task.Delay(10).Wait();
        }
        Dispatcher.UIThread.Post(() => DrawLoop());
        _canDraw = false;
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
        //if (DateTimeOffset.Now < _nextFrameTime) return;
        _canDraw = true;
        //DrawLoop();
        _nextFrameTime += _frameDuration;
    }

    private void DrawLoop()
    {
        if (_isRendering)
            return;
        _isRendering = true;

        for (int y = 0; y < 240; y++)
            for (int x = 0; x < 256; x++)
            {
                _canvas2.DrawPoint(x, y, _colorPalette[_screen[x + y * 256]]);
            }

        using (var frameBuffer = writeableBitmap.Lock())
        {
            unsafe
            {
                void* dataStart = (void*)frameBuffer.Address;
                Span<byte> buffer = new(dataStart, 256 * 240 * 4); // assume each pixel is 1 byte in size and my image has 4 channels
                _bitmap.Bytes.CopyTo(buffer);
            }
        }

        Image.InvalidateVisual();

        _isRendering = false;

        _x += 1;
        if (_x > 200) _x = 10;

        _frameNumber++;
        FrameCounter.Content = $"FPS: {_fps:N0} Frame: {_frameNumber}";

        if (DateTimeOffset.Now.Second != _startTime.Second) { _fps = _frameNumber - _startFrame; _startFrame = _frameNumber; _startTime = DateTimeOffset.Now; }
    }
}