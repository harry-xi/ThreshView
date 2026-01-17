using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace ThreshView.Views;

public partial class CustomImageViewer : UserControl
{
    // 跟踪文件或图片唯一标识
    public static readonly StyledProperty<string?> ImageKeyProperty =
        AvaloniaProperty.Register<CustomImageViewer, string?>(nameof(ImageKey));

    public static readonly StyledProperty<ICommand?> OpenCommandProperty =
        AvaloniaProperty.Register<CustomImageViewer, ICommand?>(nameof(OpenCommand));

    public static readonly StyledProperty<WriteableBitmap?> SourceProperty =
        AvaloniaProperty.Register<CustomImageViewer, WriteableBitmap?>(nameof(Source));

    private readonly Border? _backgroundPanel;

    private readonly Border? _grayInfoPanel;
    private readonly TextBlock? _grayValue;
    private readonly Button? _openImageButton;
    private bool _isDragging;
    private Point _lastDragPoint;
    private string? _lastImageKey;
    private Point _pan = new(0, 0);

    private double _scale = 1.0;

    public CustomImageViewer()
    {
        AvaloniaXamlLoader.Load(this);
        PointerWheelChanged += OnPointerWheelChanged;
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerExited += OnPointerReleased;
        this.GetObservable(BoundsProperty).Subscribe(_ =>
        {
            ResetZoomAndPan();
            InvalidateVisual();
        });

        _grayInfoPanel = this.FindControl<Border>("PART_GrayInfoPanel");
        _grayValue = this.FindControl<TextBlock>("PART_GrayValue");
        _backgroundPanel = this.FindControl<Border>("PART_BackgroundPanel");
        _openImageButton = this.FindControl<Button>("PART_OpenImageButton");
        if (_openImageButton != null)
            _openImageButton.Click += (_, e) => OpenImageClicked?.Invoke(this, e);
        _backgroundPanel?.IsVisible = Source == null;
    }

    public string? ImageKey
    {
        get => GetValue(ImageKeyProperty);
        set => SetValue(ImageKeyProperty, value);
    }

    public ICommand? OpenCommand
    {
        get => GetValue(OpenCommandProperty);
        set => SetValue(OpenCommandProperty, value);
    }

    public WriteableBitmap? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    private void ResetZoomAndPan()
    {
        if (Source == null || Bounds.Width == 0 || Bounds.Height == 0)
            return;
        var imgW = Source.PixelSize.Width;
        var imgH = Source.PixelSize.Height;
        var ctlW = Bounds.Width;
        var ctlH = Bounds.Height;
        _scale = Math.Min(ctlW / imgW, ctlH / imgH);
        // 让图片内容居中
        var viewW = imgW * _scale;
        var viewH = imgH * _scale;
        _pan = new Point((ctlW - viewW) / 2, (ctlH - viewH) / 2);
    }

    // Click event for externally-handled image opening action
    public event EventHandler<RoutedEventArgs>? OpenImageClicked;

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (Source == null) return;
        var oldScale = _scale;
        var pos = e.GetPosition(this);
        _scale *= Math.Pow(1.1, e.Delta.Y);
        _scale = Math.Clamp(_scale, 0.1, 20);
        // 缩放时以鼠标位置为中心惯性调整平移
        var offset = pos - _pan;
        var newOffset = offset * (_scale / oldScale);
        _pan += offset - newOffset;
        InvalidateVisual();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _lastDragPoint = e.GetPosition(this);
            _isDragging = true;
            Cursor = new Cursor(StandardCursorType.DragMove);
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var current = e.GetPosition(this);
            _pan += current - _lastDragPoint;
            _lastDragPoint = current;
            InvalidateVisual();
        }
        else
        {
            QueryAndShowGray(e.GetPosition(this));
        }
    }

    private void OnPointerReleased(object? sender, PointerEventArgs e)
    {
        _isDragging = false;
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ImageKeyProperty)
        {
            if (_lastImageKey == null || _lastImageKey != ImageKey) ResetZoomAndPan();
            _lastImageKey = ImageKey;
        }

        if (change.Property == SourceProperty)
        {
            _backgroundPanel?.IsVisible = Source == null;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        // 渲染图片内容
        if (Source != null)
        {
            var rect = new Rect(_pan, new Size(Source.PixelSize.Width * _scale, Source.PixelSize.Height * _scale));
            context.DrawImage(Source, new Rect(0, 0, Source.PixelSize.Width, Source.PixelSize.Height), rect);
        }
    }

    private void QueryAndShowGray(Point controlPt)
    {
        if (Source == null) return;
        // 获得图片坐标
        var imgPt = (controlPt - _pan) / _scale;
        var x = (int)Math.Floor(imgPt.X);
        var y = (int)Math.Floor(imgPt.Y);
        if (x < 0 || y < 0 || x >= Source.PixelSize.Width || y >= Source.PixelSize.Height)
        {
            _grayInfoPanel?.IsVisible = false;
            return;
        }

        // 访问像素
        try
        {
            using var lockedBuffer = Source.Lock();
            var stride = lockedBuffer.RowBytes;
            var pixOffset = y * stride + x * 4;
            var pixel = new byte[4];
            Marshal.Copy(lockedBuffer.Address + pixOffset, pixel, 0, 4);
            var r = pixel[2];
            var g = pixel[1];
            var b = pixel[0];
            var gray = (int)Math.Round(r * 0.2126 + g * 0.7152 + b * 0.0722);
            _grayValue?.Text = gray.ToString();
            _grayInfoPanel?.IsVisible = true;
        }
        catch
        {
            _grayInfoPanel?.IsVisible = false;
        }
    }
}