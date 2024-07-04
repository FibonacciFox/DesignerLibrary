using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalonia.IDE.ToolKit.Controls;

public class VisualEditingLayerItem : TemplatedControl, ISelectable
{
    public static readonly StyledProperty<double> StepSizeByXProperty =
        AvaloniaProperty.Register<VisualEditingLayerItem, double>(nameof(StepSizeByX), 8);

    public static readonly StyledProperty<double> StepSizeByYProperty =
        AvaloniaProperty.Register<VisualEditingLayerItem, double>(nameof(StepSizeByY), 8);

    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<ListBoxItem>();

    public static readonly StyledProperty<Control> ControlledControlProperty =
        AvaloniaProperty.Register<VisualEditingLayerItem, Control>(nameof(ControlledControl));

    public VisualEditingLayerItem()
    {
        Width = 300;
        Height = 300;
        IsSelected = true;
    }

    public double StepSizeByX
    {
        get => GetValue(StepSizeByXProperty);
        set => SetValue(StepSizeByXProperty, value);
    }

    public double StepSizeByY
    {
        get => GetValue(StepSizeByYProperty);
        set => SetValue(StepSizeByYProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public Control ControlledControl
    {
        get => GetValue(ControlledControlProperty);
        init => SetValue(ControlledControlProperty, value);
    }

    private bool _isResizing;
    private PointerPoint _startPoint;
    private double _originalWidth;
    private double _originalHeight;
    private double _originalLeft;
    private double _originalTop;
    private Control? _currentAnchor;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var anchors = new[]
        {
            e.NameScope.Find<Control>("TopLeftAnchor"),
            e.NameScope.Find<Control>("TopRightAnchor"),
            e.NameScope.Find<Control>("BottomLeftAnchor"),
            e.NameScope.Find<Control>("BottomRightAnchor"),
            e.NameScope.Find<Control>("TopCenterAnchor"),
            e.NameScope.Find<Control>("BottomCenterAnchor"),
            e.NameScope.Find<Control>("LeftCenterAnchor"),
            e.NameScope.Find<Control>("RightCenterAnchor"),
        };

        foreach (var anchor in anchors)
        {
            if (anchor != null)
            {
                anchor.AddHandler(PointerPressedEvent, AnchorOnPointerPressed, RoutingStrategies.Tunnel);
                anchor.AddHandler(PointerMovedEvent, AnchorOnPointerMoved, RoutingStrategies.Tunnel);
                anchor.AddHandler(PointerReleasedEvent, AnchorOnPointerReleased, RoutingStrategies.Tunnel);
            }
        }
    }

    private void AnchorOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isResizing = true;
        _startPoint = e.GetCurrentPoint((Visual?)Parent);
        _currentAnchor = sender as Control;
        e.Pointer.Capture(_currentAnchor);

        _originalWidth = Width;
        _originalHeight = Height;
        _originalLeft = Canvas.GetLeft(this);
        _originalTop = Canvas.GetTop(this);
    }

    private void AnchorOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isResizing = false;
        _currentAnchor = null;
        e.Pointer.Capture(null);
    }

    private void AnchorOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isResizing && _currentAnchor != null)
        {
            var currentPoint = e.GetCurrentPoint((Visual?)Parent);
            var deltaX = currentPoint.Position.X - _startPoint.Position.X;
            var deltaY = currentPoint.Position.Y - _startPoint.Position.Y;

            double newWidth = _originalWidth;
            double newHeight = _originalHeight;
            double newLeft = _originalLeft;
            double newTop = _originalTop;

            if (_currentAnchor.Name == "TopLeftAnchor" || _currentAnchor.Name == "LeftCenterAnchor" || _currentAnchor.Name == "BottomLeftAnchor")
            {
                newWidth = Math.Max(StepSizeByX, _originalWidth - deltaX);
                newLeft = _originalLeft + deltaX;
            }

            if (_currentAnchor.Name == "TopLeftAnchor" || _currentAnchor.Name == "TopCenterAnchor" || _currentAnchor.Name == "TopRightAnchor")
            {
                newHeight = Math.Max(StepSizeByY, _originalHeight - deltaY);
                newTop = _originalTop + deltaY;
            }

            if (_currentAnchor.Name == "BottomLeftAnchor" || _currentAnchor.Name == "BottomCenterAnchor" || _currentAnchor.Name == "BottomRightAnchor")
            {
                newHeight = Math.Max(StepSizeByY, _originalHeight + deltaY);
            }

            if (_currentAnchor.Name == "TopRightAnchor" || _currentAnchor.Name == "RightCenterAnchor" || _currentAnchor.Name == "BottomRightAnchor")
            {
                newWidth = Math.Max(StepSizeByX, _originalWidth + deltaX);
            }

            // Snap to the nearest grid
            newWidth = SnapToGrid(newWidth, StepSizeByX);
            newHeight = SnapToGrid(newHeight, StepSizeByY);

            // Update element dimensions
            Width = newWidth;
            Height = newHeight;

            // Adjust position after snapping to grid
            if (_currentAnchor.Name == "TopLeftAnchor" || _currentAnchor.Name == "LeftCenterAnchor" || _currentAnchor.Name == "BottomLeftAnchor")
            {
                newLeft = _originalLeft + (_originalWidth - newWidth);
                newLeft = SnapToGrid(newLeft, StepSizeByX);
            }

            if (_currentAnchor.Name == "TopLeftAnchor" || _currentAnchor.Name == "TopCenterAnchor" || _currentAnchor.Name == "TopRightAnchor")
            {
                newTop = _originalTop + (_originalHeight - newHeight);
                newTop = SnapToGrid(newTop, StepSizeByY);
            }

            // Update element position
            Canvas.SetLeft(this, newLeft);
            Canvas.SetTop(this, newTop);

            e.Handled = true;
        }
    }

    private double SnapToGrid(double value, double gridSize)
    {
        return Math.Round(value / gridSize) * gridSize;
    }
}
