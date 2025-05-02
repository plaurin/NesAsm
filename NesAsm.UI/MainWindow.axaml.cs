using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using NesAsm.Emulator;
using NesAsm.Example.Game1;
using NesAsm.Example.PPUExamples;
using SkiaSharp;
using System;
using System.Linq;

namespace NesAsm.UI;
public partial class MainWindow : Window
{
    private readonly SKBitmap _bitmap;
    private readonly DispatcherTimer _timer = new();
    private readonly WriteableBitmap writeableBitmap;
    private readonly TimeSpan _frameDuration = TimeSpan.FromSeconds(1) / 60.0;

    private int _x;
    private int _frameNumber;
    private bool _isRendering;
    private SKColor[] _colorPalette;

    private DateTimeOffset _startTime;
    private DateTimeOffset _nextFrameTime;

    public MainWindow()
    {
        InitializeComponent();

        _bitmap = new SKBitmap(256, 240);
        _x = 10;

        writeableBitmap = new WriteableBitmap(
            new PixelSize(_bitmap.Width, _bitmap.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);
        
        Image.Source = writeableBitmap;

        //NesApiCSharp.RunGame(BackgroundExemple.AllExemple);
        NesApiCSharp.RunGame(Game1.Reset);

        _colorPalette = PPU.Colors.Select(c => new SKColor(c.r, c.g, c.b)).ToArray()!;

        _startTime = DateTimeOffset.Now;
        _nextFrameTime = _startTime;

        _timer.Interval = TimeSpan.FromSeconds(1) / 10000.0;
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    //public override void Render(DrawingContext context)
    //{
    //    context.DrawEllipse(Brushes.Red, new Pen(Brushes.Red), new Rect(10, 10, 10, 10) );
    //    context.DrawImage(writeableBitmap, new Rect(0, 0, writeableBitmap.PixelSize.Width, writeableBitmap.PixelSize.Height));
    //}

    private void Timer_Tick(object? sender, EventArgs e)
    {
        //while(true)
        {
            if (DateTimeOffset.Now < _nextFrameTime) return;
            DrawLoop();
            _nextFrameTime += _frameDuration;
        }
    }

    private void DrawLoop()
    {
        if (_isRendering)
            return;
        _isRendering = true;

        using (var canvas = new SKCanvas(_bitmap))
        {
            var screen = PPU.DrawScreen();

            for (int y = 0; y < 240; y++)
                for (int x = 0; x < 256; x++)
                {
                    canvas.DrawPoint(x, y, _colorPalette[screen[x + y * 256]]);
                }
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
        var elapsed = DateTimeOffset.Now - _startTime;
        var fps = _frameNumber / elapsed.TotalSeconds;
        FrameCounter.Content = $"FPS: {fps:N0} Frame: {_frameNumber}";
    }
}