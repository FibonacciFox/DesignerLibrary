using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Avalonia.IDE.ToolKit.Controls;

public class ScalingContainer : TemplatedControl
{
    public static readonly StyledProperty<object> ContentProperty =
        AvaloniaProperty.Register<ScalingContainer, object>(nameof(Content));
        
    public static readonly StyledProperty<double> ScaleFactorProperty =
        AvaloniaProperty.Register<ScalingContainer, double>(nameof(ScaleFactor), 1.0, coerce: CoerceScaleFactor);
    
    public static readonly AttachedProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
        ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner<ScalingContainer>();
    
    public static readonly AttachedProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner<ScalingContainer>();
    
    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<ScalingContainer>();
    
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<ScalingContainer>();

    
    public static readonly AttachedProperty<bool> AllowAutoHideProperty =
        ScrollViewer.AllowAutoHideProperty.AddOwner<ScalingContainer>();
    
    static ScalingContainer()
    {
        ScaleFactorProperty.Changed.AddClassHandler<ScalingContainer>((x, e) => x.OnScaleFactorChanged(e));
    }
    
    private void OnScaleFactorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        ScaleContent((double)(e.NewValue ?? throw new InvalidOperationException()));
    }
        
    private void ScaleContent(double scaleFactor)
    {
        if (_partLayoutTransform == null)
        {
            return;
        }

        var scaleTransform = new ScaleTransform(scaleFactor, scaleFactor);
        _partLayoutTransform.LayoutTransform = scaleTransform;
        _partLayoutTransform.InvalidateMeasure();
    }
        
    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty!, value);
    }
    
    /// <summary>
    /// Gets or sets the horizontal scrollbar visibility.
    /// </summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical scrollbar visibility.
    /// </summary>
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the horizontal alignment of the content within the control.
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the vertical alignment of the content within the control.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }
    
    /// <summary>
    /// Gets a value that indicates whether scrollbars can hide itself when user is not interacting with it.
    /// </summary>
    public bool AllowAutoHide
    {
        get => GetValue(AllowAutoHideProperty);
        set => SetValue(AllowAutoHideProperty, value);
    }
    
    public double ScaleFactor
    {
        get => GetValue(ScaleFactorProperty);
        set => SetValue(ScaleFactorProperty, value);
    }
        
    private static double CoerceScaleFactor(AvaloniaObject sender, double value)
    {
        // Ограничиваем масштаб в пределах от 1 до 10 для предотвращения слишком сильного увеличения или уменьшения
        return Math.Max(1.0, Math.Min(value, 10.0));
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        _partLayoutTransform = e.NameScope.Find<LayoutTransformControl>("PART_LayoutTransform");
        
        // Применяем масштаб только после того, как шаблон был применен
        ScaleContent(ScaleFactor);
    }
    
    private LayoutTransformControl? _partLayoutTransform;
}
