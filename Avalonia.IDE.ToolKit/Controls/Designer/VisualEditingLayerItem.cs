using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Avalonia.IDE.ToolKit.Controls.Designer;

public enum AnchorType
{
    None,
    TopLeft,
    TopCenter,
    TopRight,
    RightCenter,
    BottomRight,
    BottomCenter,
    BottomLeft,
    LeftCenter
}

public class VisualEditingLayerItem : TemplatedControl, ISelectable
{
    private const double AnchorPadding = 6;

    public static readonly StyledProperty<double> StepSizeByXProperty =
        AvaloniaProperty.Register<VisualEditingLayerItem, double>(nameof(StepSizeByX), AnchorPadding);

    public static readonly StyledProperty<double> StepSizeByYProperty =
        AvaloniaProperty.Register<VisualEditingLayerItem, double>(nameof(StepSizeByY), AnchorPadding);

    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<ListBoxItem>();

    public static readonly StyledProperty<Control> AttachedControlProperty =
        AvaloniaProperty.Register<VisualEditingLayerItem, Control>(nameof(AttachedControl));

    public new static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<VisualEditingLayerItem, double>(nameof(BorderThickness));

    static VisualEditingLayerItem()
    {
        AttachedControlProperty.Changed.AddClassHandler<VisualEditingLayerItem>((x, e) => x.OnAttachedControlChanged(e));
    }

    private IDisposable? _xSub, _ySub, _widthSub, _heightSub;

    private AnchorType _currentAnchorType = AnchorType.None;
    private Control? _currentAnchor;
    private Rectangle? _partCustomBorder;
    private ContentPresenter? _partContent;

    private bool _isResizing;
    private PointerPoint _startPoint;
    private double _originalWidth;
    private double _originalHeight;
    private double _originalLeft;
    private double _originalTop;

    private bool _isDragging;
    private PointerPoint _dragStartPoint;
    private Point _originalPosition;

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

    public Control AttachedControl
    {
        get => GetValue(AttachedControlProperty);
        set => SetValue(AttachedControlProperty, value);
    }

    public new double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    private void OnAttachedControlChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _xSub?.Dispose();
        _ySub?.Dispose();
        _widthSub?.Dispose();
        _heightSub?.Dispose();

        if (e.NewValue is Control newControl)
        {
            if (this.GetVisualRoot() == null)
                AttachedToVisualTree += OnFirstAttach;
            else
                ApplyInitialLayout();

            _widthSub = newControl.GetObservable(WidthProperty)
                .Subscribe(w => Width = w + AnchorPadding * 2);
            _heightSub = newControl.GetObservable(HeightProperty)
                .Subscribe(h => Height = h + AnchorPadding * 2);
            _xSub = newControl.GetObservable(Layout.XProperty)
                .Subscribe(x => Layout.SetX(this, (x ?? 0) - AnchorPadding));
            _ySub = newControl.GetObservable(Layout.YProperty)
                .Subscribe(y => Layout.SetY(this, (y ?? 0) - AnchorPadding));
        }
    }

    private void OnFirstAttach(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AttachedToVisualTree -= OnFirstAttach;
        ApplyInitialLayout();
    }

    private void ApplyInitialLayout()
    {
        Width = AttachedControl.Width + AnchorPadding * 2;
        Height = AttachedControl.Height + AnchorPadding * 2;

        Layout.SetX(this, (Layout.GetX(AttachedControl) ?? 0) - AnchorPadding);
        Layout.SetY(this, (Layout.GetY(AttachedControl) ?? 0) - AnchorPadding);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var anchorNames = new[]
        {
            "TopLeftAnchor", "TopCenterAnchor", "TopRightAnchor",
            "RightCenterAnchor", "BottomRightAnchor", "BottomCenterAnchor",
            "BottomLeftAnchor", "LeftCenterAnchor"
        };

        foreach (var name in anchorNames)
        {
            var anchor = e.NameScope.Find<Control>(name);
            if (anchor != null)
                SubscribeAnchorEvents(anchor);
        }

        _partCustomBorder = e.NameScope.Find<Rectangle>("PART_CustomBorder");
        _partContent = e.NameScope.Find<ContentPresenter>("PART_Content");

        if (_partContent != null)
        {
            _partContent.AddHandler(PointerPressedEvent, OnContentDragStart, RoutingStrategies.Tunnel);
            _partContent.AddHandler(PointerMovedEvent, OnContentDragMove, RoutingStrategies.Tunnel);
            _partContent.AddHandler(PointerReleasedEvent, OnContentDragEnd, RoutingStrategies.Tunnel);
        }
    }

    private void SubscribeAnchorEvents(Control anchor)
    {
        anchor.AddHandler(PointerPressedEvent, AnchorOnPointerPressed, RoutingStrategies.Tunnel);
        anchor.AddHandler(PointerMovedEvent, AnchorOnPointerMoved, RoutingStrategies.Tunnel);
        anchor.AddHandler(PointerReleasedEvent, AnchorOnPointerReleased, RoutingStrategies.Tunnel);
    }

    private void AnchorOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _isResizing = true;
        _startPoint = e.GetCurrentPoint((Visual?)Parent);
        _currentAnchor = sender as Control;

        _currentAnchorType = Enum.TryParse<AnchorType>(_currentAnchor?.Tag?.ToString(), out var parsed)
            ? parsed
            : AnchorType.None;

        e.Pointer.Capture(_currentAnchor);

        _originalWidth = Width;
        _originalHeight = Height;
        _originalLeft = Layout.GetX(this) ?? 0;
        _originalTop = Layout.GetY(this) ?? 0;

        _partCustomBorder?.SetValue(IsVisibleProperty, true);
    }

    private void AnchorOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isResizing && _currentAnchorType != AnchorType.None)
        {
            var current = e.GetCurrentPoint((Visual?)Parent);
            var dx = current.Position.X - _startPoint.Position.X;
            var dy = current.Position.Y - _startPoint.Position.Y;

            var (w, h, l, t) = CalculateNewDimensions(dx, dy);

            Width = w;
            Height = h;
            Layout.SetX(this, l);
            Layout.SetY(this, t);

            e.Handled = true;
        }
    }

    private void AnchorOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isResizing)
            return;

        _isResizing = false;
        _currentAnchor = null;
        _currentAnchorType = AnchorType.None;

        UpdateAttachedControlBounds();

        _partCustomBorder?.SetValue(IsVisibleProperty, false);
        e.Pointer.Capture(null);
    }

    private void OnContentDragStart(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || _isResizing)
            return;

        _isDragging = true;
        _dragStartPoint = e.GetCurrentPoint((Visual?)Parent);
        _originalPosition = new Point(Layout.GetX(this) ?? 0, Layout.GetY(this) ?? 0);

        e.Pointer.Capture((IInputElement)sender!);
    }

    private void OnContentDragMove(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var current = e.GetCurrentPoint((Visual?)Parent);
            var dx = current.Position.X - _dragStartPoint.Position.X;
            var dy = current.Position.Y - _dragStartPoint.Position.Y;

            var newX = SnapToGrid(_originalPosition.X + dx, StepSizeByX);
            var newY = SnapToGrid(_originalPosition.Y + dy, StepSizeByY);

            Layout.SetX(this, newX);
            Layout.SetY(this, newY);

            _partCustomBorder?.SetValue(IsVisibleProperty, true);
            e.Handled = true;
        }
    }

    private void OnContentDragEnd(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            UpdateAttachedControlBounds();

            _partCustomBorder?.SetValue(IsVisibleProperty, false);
            e.Pointer.Capture(null);
        }
    }

    private (double newWidth, double newHeight, double newLeft, double newTop) CalculateNewDimensions(double dx, double dy)
    {
        double w = _originalWidth;
        double h = _originalHeight;
        double l = _originalLeft;
        double t = _originalTop;

        if (_currentAnchorType is AnchorType.TopLeft or AnchorType.LeftCenter or AnchorType.BottomLeft)
        {
            w = Math.Max(StepSizeByX, _originalWidth - dx);
            l = _originalLeft + dx;
        }

        if (_currentAnchorType is AnchorType.TopLeft or AnchorType.TopCenter or AnchorType.TopRight)
        {
            h = Math.Max(StepSizeByY, _originalHeight - dy);
            t = _originalTop + dy;
        }

        if (_currentAnchorType is AnchorType.BottomLeft or AnchorType.BottomCenter or AnchorType.BottomRight)
        {
            h = Math.Max(StepSizeByY, _originalHeight + dy);
        }

        if (_currentAnchorType is AnchorType.TopRight or AnchorType.RightCenter or AnchorType.BottomRight)
        {
            w = Math.Max(StepSizeByX, _originalWidth + dx);
        }

        w = SnapToGrid(w, StepSizeByX);
        h = SnapToGrid(h, StepSizeByY);

        if (_currentAnchorType.ToString().Contains("Left"))
        {
            l = _originalLeft + (_originalWidth - w);
            l = SnapToGrid(l, StepSizeByX);
        }

        if (_currentAnchorType.ToString().Contains("Top"))
        {
            t = _originalTop + (_originalHeight - h);
            t = SnapToGrid(t, StepSizeByY);
        }

        return (w, h, l, t);
    }

    public void UpdateAttachedControlBounds()
    {
        AttachedControl.Width = Width - AnchorPadding * 2;
        AttachedControl.Height = Height - AnchorPadding * 2;

        Layout.SetX(AttachedControl, (Layout.GetX(this) ?? 0) + AnchorPadding);
        Layout.SetY(AttachedControl, (Layout.GetY(this) ?? 0) + AnchorPadding);
    }

    private double SnapToGrid(double value, double gridSize)
    {
        return Math.Round(value / gridSize) * gridSize;
    }
}
