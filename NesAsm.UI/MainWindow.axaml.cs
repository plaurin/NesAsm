using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SkiaSharp;
using System;

namespace NesAsm.UI;
public partial class MainWindow : Window
{
    private readonly SKBitmap _bitmap;
    private readonly DispatcherTimer _timer = new();

    private int _x;
    private DateTimeOffset _startTime;
    private int _frameNumber;
    private bool _isRendering;

    public MainWindow()
    {
        InitializeComponent();

        _bitmap = new SKBitmap(256, 240);
        _x = 10;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!_timer.IsEnabled)
        {
            _timer.Interval = TimeSpan.FromSeconds(1) / 100.0;
            _timer.Tick += Timer_Tick;
            _timer.Start();
            _startTime = DateTimeOffset.Now;
        }
        else
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_isRendering)
            return;
        _isRendering = true;

        using (var canvas = new SKCanvas(_bitmap))
        {
            canvas.Clear(SKColors.White);
            // Draw whatever you like, e.g., a red rectangle
            using var paint = new SKPaint { Color = SKColors.Red, IsAntialias = true };
            canvas.DrawRect(_x, 50, 100, 100, paint);
        }

        var writeableBitmap = new WriteableBitmap(
            new PixelSize(_bitmap.Width, _bitmap.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);

        using (var frameBuffer = writeableBitmap.Lock())
        {
            unsafe
            {
                void* dataStart = (void*)frameBuffer.Address;
                Span<byte> buffer = new Span<byte>(dataStart, 256 * 240 * 4); // assume each pixel is 1 byte in size and my image has 4 channels
                _bitmap.Bytes.CopyTo(buffer);
            }
        }

        Image.Source = writeableBitmap;

        _isRendering = false;

        _x += 1;
        if (_x > 200) _x = 10;

        _frameNumber++;
        var elapsed = DateTimeOffset.Now - _startTime;
        var fps = _frameNumber / elapsed.TotalSeconds;
        FrameCounter.Content = $"FPS: {fps:N0} Frame: {_frameNumber}";
    }
}