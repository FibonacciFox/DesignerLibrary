using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Avalonia.IDE.ToolKit.Controls.Designer;

public class ResizableBorder : TemplatedControl
{
    public static readonly StyledProperty<Control?> ContentProperty =
        AvaloniaProperty.Register<ResizableBorder, Control?>(nameof(Content));

    [Content]
    public Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    private bool _isDragging;
    private Point _dragStart;
    private TranslateTransform _transform = new();
    private bool _isResizing;
    private string? _resizeDirection;
    private Point _resizeStart;
    private Size _originalSize;
    private TranslateTransform _startTransform;

    public ResizableBorder()
    {
        RenderTransform = _transform;

        AddHandler(PointerPressedEvent, OnDragStart, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, OnDragMove, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnDragEnd, RoutingStrategies.Tunnel);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        string[] anchors =
        {
            "PART_TopLeft", "PART_Top", "PART_TopRight",
            "PART_Right", "PART_BottomRight", "PART_Bottom",
            "PART_BottomLeft", "PART_Left"
        };

        foreach (var name in anchors)
        {
            if (e.NameScope.Find<Control>(name) is { } anchor)
            {
                anchor.Tag = name;
                anchor.AddHandler(PointerPressedEvent, OnResizeStart, RoutingStrategies.Tunnel);
                anchor.AddHandler(PointerMovedEvent, OnResizeMove, RoutingStrategies.Tunnel);
                anchor.AddHandler(PointerReleasedEvent, OnResizeEnd, RoutingStrategies.Tunnel);
            }
        }
    }

    private void OnDragStart(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source == this && !_isResizing && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _dragStart = e.GetPosition(this);
            Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Handled = true;
        }
    }

    private void OnDragMove(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var pos = e.GetPosition(this);
            var delta = pos - _dragStart;

            _transform = new TranslateTransform(
                _transform.X + delta.X,
                _transform.Y + delta.Y);

            RenderTransform = _transform;
            e.Handled = true;
        }
    }

    private void OnDragEnd(object? sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
        Cursor = Cursor.Default;
        e.Handled = true;
    }

    private void OnResizeStart(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control anchor && anchor.Tag is string direction)
        {
            _isResizing = true;
            _resizeDirection = direction;
            _resizeStart = e.GetPosition(this);
            _originalSize = Bounds.Size;
            _startTransform = _transform;
        }
        e.Handled = true;
    }

    private void OnResizeMove(object? sender, PointerEventArgs e)
    {
        if (!_isResizing || _resizeDirection is null)
            return;

        var current = e.GetPosition(this);
        var delta = current - _resizeStart;

        double newWidth = _originalSize.Width;
        double newHeight = _originalSize.Height;
        double offsetX = _startTransform.X;
        double offsetY = _startTransform.Y;

        if (_resizeDirection.Contains("Left"))
        {
            newWidth = Math.Max(0, _originalSize.Width - delta.X);
            offsetX = _startTransform.X + delta.X;
        }
        else if (_resizeDirection.Contains("Right"))
        {
            newWidth = Math.Max(0, _originalSize.Width + delta.X);
        }

        if (_resizeDirection.Contains("Top"))
        {
            newHeight = Math.Max(0, _originalSize.Height - delta.Y);
            offsetY = _startTransform.Y + delta.Y;
        }
        else if (_resizeDirection.Contains("Bottom"))
        {
            newHeight = Math.Max(0, _originalSize.Height + delta.Y);
        }

        Width = newWidth;
        Height = newHeight;
        _transform = new TranslateTransform(offsetX, offsetY);
        RenderTransform = _transform;
        e.Handled = true;
    }

    private void OnResizeEnd(object? sender, PointerReleasedEventArgs e)
    {
        _isResizing = false;
        _resizeDirection = null;
        e.Handled = true;
    }
}
