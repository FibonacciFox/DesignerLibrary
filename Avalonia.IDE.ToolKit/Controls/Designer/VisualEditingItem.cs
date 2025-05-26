using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.IDE.ToolKit.Controls.Designer.Items;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Avalonia.IDE.ToolKit.Controls.Designer;

/// <summary>
/// Контрол-обёртка для визуального редактирования размеров и положения другого контрола.
/// Поддерживает псевдоклассы:
/// <list type="bullet">
/// <item><c>:selected</c> — когда элемент выбран</item>
/// <item><c>:drag</c> — когда выполняется перемещение</item>
/// <item><c>:resize</c> — когда выполняется изменение размеров</item>
/// </list>
/// </summary>
[PseudoClasses(":selected", ":drag", ":resize")]
public class VisualEditingItem : TemplatedControl, ISelectable
{
    
    public static readonly StyledProperty<double> StepSizeByXProperty =
        AvaloniaProperty.Register<VisualEditingItem, double>(nameof(StepSizeByX), 8);

    public static readonly StyledProperty<double> StepSizeByYProperty =
        AvaloniaProperty.Register<VisualEditingItem, double>(nameof(StepSizeByY), 8);

    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<VisualEditingItem>();

    public static readonly StyledProperty<Control> AttachedControlProperty =
        AvaloniaProperty.Register<VisualEditingItem, Control>(nameof(AttachedControl));
    
    public static readonly StyledProperty<int> AnchorSizeProperty =
        AvaloniaProperty.Register<VisualEditingItem, int>(nameof(AnchorSize), 6);

    public new static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<VisualEditingItem, double>(nameof(BorderThickness));
    
    static VisualEditingItem()
    {
        AttachedControlProperty.Changed.AddClassHandler<VisualEditingItem>((x, e) => x.OnAttachedControlChanged(e));
    }

    private IDisposable? _xSub, _ySub, _widthSub, _heightSub;
    private AnchorType _currentAnchorType = AnchorType.None;
    private Control? _currentAnchor;
    private ContentPresenter? _partContent;

    private bool _isResizing;
    private bool _isDragging;

    private PointerPoint _startPoint;
    private PointerPoint _dragStartPoint;

    private double _originalWidth;
    private double _originalHeight;
    private double _originalLeft;
    private double _originalTop;
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
    
    public  int AnchorSize
    {
        get => GetValue(AnchorSizeProperty);
        set => SetValue(AnchorSizeProperty, value);
    }

    /// <summary>
    /// Обновляет псевдокласс :selected при изменении IsSelected.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSelectedProperty)
        {
            PseudoClasses.Set(":selected", IsSelected);
        }
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

            
            _widthSub = newControl.GetObservable(BoundsProperty)
                .Subscribe(w => UpdateSizeFromWidthAttachedControl());
            _heightSub = newControl.GetObservable(BoundsProperty)
                .Subscribe(h => UpdateSizeFromHeightAttachedControl());

            
            _xSub = newControl.GetObservable(Extensions.Layout.DesignXProperty)
                .Subscribe(x => Extensions.Layout.SetDesignX(this, x - AnchorSize));
            _ySub = newControl.GetObservable(Extensions.Layout.DesignYProperty)
                .Subscribe(y => Extensions.Layout.SetDesignY(this, y - AnchorSize));
        }
    }
    
    private void UpdateSizeFromWidthAttachedControl()
    {
        double width = !double.IsNaN(AttachedControl.Width)
            ? AttachedControl.Width
            : AttachedControl.Bounds.Width;

        Console.WriteLine(AttachedControl.Bounds.Width);
        Width = width + AnchorSize * 2;
        
    }
    
    private void UpdateSizeFromHeightAttachedControl()
    {
        double height = !double.IsNaN(AttachedControl.Height)
            ? AttachedControl.Height
            : AttachedControl.Bounds.Height;
        Console.WriteLine(AttachedControl.Bounds.Height);
        Height = height + AnchorSize * 2;
        
    }

    private void OnFirstAttach(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AttachedToVisualTree -= OnFirstAttach;
        ApplyInitialLayout();
    }

    private void ApplyInitialLayout()
    {
        if (Parent is not Visual)
            return;
      
        Extensions.Layout.SetX(this, Extensions.Layout.GetDesignX(AttachedControl) - AnchorSize);
        Extensions.Layout.SetY(this, Extensions.Layout.GetDesignY(AttachedControl) - AnchorSize);

        UpdateSizeFromWidthAttachedControl();
        UpdateSizeFromHeightAttachedControl();
    }


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _partContent = e.NameScope.Find<ContentPresenter>("PART_Content");

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
        _originalLeft = Extensions.Layout.GetX(this);
        _originalTop = Extensions.Layout.GetY(this);

        PseudoClasses.Set(":resize", true);
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
            Extensions.Layout.SetX(this, l);
            Extensions.Layout.SetY(this, t);

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
        PseudoClasses.Set(":resize", false);
        e.Pointer.Capture(null);
    }

    private void OnContentDragStart(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || _isResizing)
            return;

        _isDragging = true;
        _dragStartPoint = e.GetCurrentPoint((Visual?)Parent);
        _originalPosition = new Point(Extensions.Layout.GetX(this), Extensions.Layout.GetY(this));

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

            Extensions.Layout.SetX(this, newX);
            Extensions.Layout.SetY(this, newY);

            PseudoClasses.Set(":drag", true);
            e.Handled = true;
        }
    }

    private void OnContentDragEnd(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            UpdateAttachedControlBounds();

            PseudoClasses.Set(":drag", false);
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

    /// <summary>
    /// Копирует текущие размеры и позицию в AttachedControl.
    /// </summary>
    public void UpdateAttachedControlBounds()
    {
      
        AttachedControl.Width = Width - AnchorSize * 2;
        AttachedControl.Height = Height - AnchorSize * 2;
        

        Extensions.Layout.SetDesignX(AttachedControl, Extensions.Layout.GetX(this) + AnchorSize);
        Extensions.Layout.SetDesignY(AttachedControl, Extensions.Layout.GetY(this) + AnchorSize);
    }

    private double SnapToGrid(double value, double gridSize)
    {
        return Math.Round(value / gridSize) * gridSize;
    }
}
