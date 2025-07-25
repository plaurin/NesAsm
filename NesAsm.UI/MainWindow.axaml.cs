using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using NesAsm.Emulator;
using NesAsm.Example.JumpMan;
using NesAsm.Example.PPUExamples;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NesAsm.UI;
public partial class MainWindow : Window
{
    private readonly SKBitmap _bitmap;
    private readonly byte[] _screen;
    private readonly DispatcherTimer _timer = new();
    private readonly WriteableBitmap writeableBitmap;
    private readonly TimeSpan _frameDuration = TimeSpan.FromSeconds(1) / 60.0;

    private int _frameNumber;
    private bool _isRendering;
    private SKColor[] _colorPalette;

    private DateTimeOffset _startTime;
    private DateTimeOffset _nextFrameTime;
    private Stopwatch _watch;
    private TimeSpan _lastWatch;
    private int _startFrame;
    private int _fps;

    const double FrameSpan = 1000 / 60;

    private CancellationTokenSource _cancellationTokenSource = new();

    public MainWindow()
    {
        InitializeComponent();

        var topLevel = TopLevel.GetTopLevel(this);
        topLevel!.KeyDown += OnKeyDown;
        topLevel!.KeyUp += OnKeyUp;

        _bitmap = new SKBitmap(256, 240);
        _screen = PPU.GetScreen();

        writeableBitmap = new WriteableBitmap(
            new PixelSize(_bitmap.Width, _bitmap.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);
        
        Image.Source = writeableBitmap;

        _colorPalette = PPU.Colors.Select(c => new SKColor(c.r, c.g, c.b)).ToArray()!;

        _startTime = DateTimeOffset.Now;
        _nextFrameTime = _startTime;
        _watch = new Stopwatch();
        _watch.Start();

        _timer.Interval = TimeSpan.FromMilliseconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();

        var cancellationToken = _cancellationTokenSource.Token;

        //RunGame("Vertical Scrolling");
        RunGame("Boxing Game");
        //Task.Run(() => NesApiCSharp.RunOnce(draw: Draw, gameEntryPoint: PPUExemple.Run)).ConfigureAwait(false);
        //Task.Run(() => NesApiCSharp.RunGame(draw: Draw, reset: GameLoopExemple.Reset, nmi: GameLoopExemple.Nmi)).ConfigureAwait(false);
        //Task.Run(() => NesApiCSharp.RunGame(draw: Draw, reset: ImageLoading.Reset, nmi: ImageLoading.Nmi)).ConfigureAwait(false);
    }

    private void RunGame(string gameName)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new();
        var cancellationToken = _cancellationTokenSource.Token;

        switch (gameName)
        {
            case "Jump Man":
                Task.Run(() => NesApiCSharp.RunGame(cancellationToken, draw: Draw, reset: JumpManGame.Reset, nmi: JumpManGame.Nmi), cancellationToken).ConfigureAwait(false);
                break;
            case "Vertical Scrolling":
                Task.Run(() => NesApiCSharp.RunGame(cancellationToken, draw: Draw, reset: VerticalScrolling.Reset, nmi: VerticalScrolling.Nmi), cancellationToken).ConfigureAwait(false);
                break;
            case "Boxing Game":
                Task.Run(() => NesApiCSharp.RunGame(cancellationToken, draw: Draw, reset: BoxingRPGGame.Reset, nmi: BoxingRPGGame.Nmi)).ConfigureAwait(false);
                break;
            default:
                throw new InvalidOperationException($"Game {gameName} is not implemented yet.");
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.PhysicalKey == PhysicalKey.ArrowRight) InputManager.Right = true;
        if (e.PhysicalKey == PhysicalKey.ArrowLeft) InputManager.Left = true;
        if (e.PhysicalKey == PhysicalKey.ArrowUp) InputManager.Up = true;
        if (e.PhysicalKey == PhysicalKey.ArrowDown) InputManager.Down = true;
        if (e.PhysicalKey == PhysicalKey.Z) InputManager.B = true;
        if (e.PhysicalKey == PhysicalKey.X) InputManager.A = true;
        if (e.PhysicalKey == PhysicalKey.A) InputManager.Select = true;
        if (e.PhysicalKey == PhysicalKey.S) InputManager.Start = true;
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.PhysicalKey == PhysicalKey.ArrowRight) InputManager.Right = false;
        if (e.PhysicalKey == PhysicalKey.ArrowLeft) InputManager.Left = false;
        if (e.PhysicalKey == PhysicalKey.ArrowUp) InputManager.Up = false;
        if (e.PhysicalKey == PhysicalKey.ArrowDown) InputManager.Down = false;
        if (e.PhysicalKey == PhysicalKey.Z) InputManager.B = false;
        if (e.PhysicalKey == PhysicalKey.X) InputManager.A = false;
        if (e.PhysicalKey == PhysicalKey.A) InputManager.Select = false;
        if (e.PhysicalKey == PhysicalKey.S) InputManager.Start = false;
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
        //if (_frameNumber % 10 == 0)
        {
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

            using (var frameBuffer = writeableBitmap.Lock())
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
        }

        Image.InvalidateVisual();
        //Task.Delay(300).Wait();

        _isRendering = false;

        //_x += 1;
        //if (_x > 200) _x = 10;

        var frameTime = _watch.Elapsed - _lastWatch;
        PPU.Log($"Frame time for {_frameNumber} : {frameTime} ({frameTime.TotalMilliseconds - (1000 / 60):0000})");
        _lastWatch = _watch.Elapsed;

        _frameNumber++;
        FrameCounter.Content = $"FPS: {_fps:N0} Frame: {_frameNumber}";

        if (DateTimeOffset.Now.Second != _startTime.Second)
        {
            _fps = _frameNumber - _startFrame;
            _startFrame = _frameNumber;
            _startTime = DateTimeOffset.Now;
        }
    }
}