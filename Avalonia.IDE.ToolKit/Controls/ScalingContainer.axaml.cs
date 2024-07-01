using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Avalonia.IDE.ToolKit.Controls;

public class ScalingContainer : TemplatedControl
{
        
    public static readonly StyledProperty<object> ContentProperty =
        AvaloniaProperty.Register<ScalingContainer, object>(nameof(Content));
        
    public static readonly StyledProperty<double> ScaleFactorProperty =
        AvaloniaProperty.Register<ScalingContainer, double>(nameof(ScaleFactor), 1.0, coerce: CoerceScaleFactor);
    
    
    static ScalingContainer()
    {
        ScaleFactorProperty.Changed.AddClassHandler<ScalingContainer>((x, e) => x.OnScaleFactorChanged(e));
    }

    public ScalingContainer()
    {
        ScaleFactorProperty.Changed.AddClassHandler<ScalingContainer>((x, e) => x.OnScaleFactorChanged(e));
    }
    
    private void OnScaleFactorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        ScaleContent((double)(e.NewValue ?? throw new InvalidOperationException()));
    }
        
     
    private void ScaleContent(double scaleFactor)
    {
        var scaleTransform = new ScaleTransform(scaleFactor, scaleFactor);
            
        _partLayoutTransform!.LayoutTransform = scaleTransform;

        // Обновляем размеры контейнера после изменения масштаба
        _partLayoutTransform.InvalidateMeasure();
    }
        
    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty!, value);
    }
        
    private static double CoerceScaleFactor(AvaloniaObject sender, double value)
    {
        // Ограничиваем масштаб в пределах от 1 до 10 для предотвращения слишком сильного увеличения или уменьшения
        return Math.Max(1.0, Math.Min(value, 10.0));
    }
        
    public double ScaleFactor
    {
        get => GetValue(ScaleFactorProperty);
        set => SetValue(ScaleFactorProperty, value);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        _partLayoutTransform = e.NameScope.Find<LayoutTransformControl>("PART_LayoutTransform");
    }
    
    private LayoutTransformControl? _partLayoutTransform;
}