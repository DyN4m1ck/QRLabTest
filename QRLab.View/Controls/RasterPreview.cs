using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.View.Controls;

/// <summary>Displays an existing raster. Decoding for display never changes its PNG data.</summary>
public sealed class RasterPreview : Control
{
    public static readonly StyledProperty<RasterImage?> ImageProperty = AvaloniaProperty.Register<RasterPreview, RasterImage?>(nameof(Image));
    public static readonly StyledProperty<bool> AllowZoomProperty = AvaloniaProperty.Register<RasterPreview, bool>(nameof(AllowZoom));
    public static readonly StyledProperty<bool> PanEnabledProperty = AvaloniaProperty.Register<RasterPreview, bool>(nameof(PanEnabled));
    public static readonly StyledProperty<ICommand?> PaintCommandProperty = AvaloniaProperty.Register<RasterPreview, ICommand?>(nameof(PaintCommand));
    public RasterImage? Image { get => GetValue(ImageProperty); set => SetValue(ImageProperty, value); }
    public bool AllowZoom { get => GetValue(AllowZoomProperty); set => SetValue(AllowZoomProperty, value); }
    public bool PanEnabled { get => GetValue(PanEnabledProperty); set => SetValue(PanEnabledProperty, value); }
    public ICommand? PaintCommand { get => GetValue(PaintCommandProperty); set => SetValue(PaintCommandProperty, value); }
    private Bitmap? _displayImage;
    private Point? _dragStart;
    private Vector _pan;
    private double _zoom = 1;
    private int _decodedWidth;
    private bool _attached;

    static RasterPreview() => AffectsRender<RasterPreview>(ImageProperty);
    public RasterPreview() => ClipToBounds = true;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ImageProperty)
        {
            ReleaseImage();
            _zoom = 1; _pan = default;
            InvalidateVisual();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        InvalidateVisual();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _attached = false;
        ReleaseImage();
        base.OnDetachedFromVisualTree(e);
    }

    private void ReleaseImage()
    {
        _displayImage?.Dispose(); _displayImage = null; _decodedWidth = 0;
    }

    private Rect Destination(RasterImage image)
    {
        var scale = Math.Min(Bounds.Width / image.Width, Bounds.Height / image.Height) * _zoom;
        var size = new Size(image.Width * scale, image.Height * scale);
        return new Rect((Bounds.Width - size.Width) / 2 + _pan.X, (Bounds.Height - size.Height) / 2 + _pan.Y, size.Width, size.Height);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (!_attached || Image is not { } image || Bounds.Width <= 0 || Bounds.Height <= 0) return;
        var destination = Destination(image);
        var width = Math.Clamp((int)Math.Ceiling(destination.Width * 2), 1, image.Width);
        if (_displayImage is null || width > _decodedWidth)
        {
            ReleaseImage();
            using var stream = image.OpenRead();
            _displayImage = Bitmap.DecodeToWidth(stream, width, BitmapInterpolationMode.HighQuality);
            _decodedWidth = width;
        }
        context.DrawImage(_displayImage, new Rect(_displayImage.Size), destination);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || Image is not { } image) return;
        if (PanEnabled) { _dragStart = e.GetPosition(this); e.Pointer.Capture(this); return; }
        var target = Destination(image);
        var position = e.GetPosition(this);
        var point = new PreviewPoint((position.X - target.X) / target.Width, (position.Y - target.Y) / target.Height);
        if (point.X is >= 0 and <= 1 && point.Y is >= 0 and <= 1 && PaintCommand is { } command && command.CanExecute(point))
            command.Execute(point);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragStart is not { } previous) return;
        var position = e.GetPosition(this); _pan += position - previous; _dragStart = position; InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e); _dragStart = null; e.Pointer.Capture(null);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (!AllowZoom) { base.OnPointerWheelChanged(e); return; }
        _zoom = Math.Clamp(_zoom + e.Delta.Y * .1, .5, 3); InvalidateVisual(); e.Handled = true;
    }
}