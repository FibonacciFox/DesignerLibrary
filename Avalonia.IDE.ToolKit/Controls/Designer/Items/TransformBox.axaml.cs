using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.IDE.ToolKit.Controls.Designer.Layers;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Avalonia.IDE.ToolKit.Controls.Designer.Items;

/// <summary>
/// Контрол для визуального редактирования размеров и положения другого контрола.
/// </summary>
[PseudoClasses(":selected", ":pressed", ":active", ":drag", ":resize")]
public class TransformBox : SelectableInteractionLayerItem
{
    public static readonly StyledProperty<Size> GridStepProperty =
        AvaloniaProperty.Register<TransformBox, Size>(nameof(GridStep), new Size(8, 8));
    
    public static readonly StyledProperty<Control> TargetProperty =
        AvaloniaProperty.Register<TransformBox, Control>(nameof(Target));
    
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<TransformBox, bool>(nameof(IsActive));

    public static readonly StyledProperty<int> AnchorSizeProperty =
        AvaloniaProperty.Register<TransformBox, int>(nameof(AnchorSize), 6);

    public new static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<TransformBox, double>(nameof(BorderThickness)); 

    public Size GridStep
    {
        get => GetValue(GridStepProperty);
        set => SetValue(GridStepProperty, value);
    }
    
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public Control Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public new double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public int AnchorSize
    {
        get => GetValue(AnchorSizeProperty);
        set => SetValue(AnchorSizeProperty, value);
    }

    private double _targetPosX, _targetPosY, _targetWidth, _targetHeight;
    
    static TransformBox()
    {
        SelectableMixin.Attach<TransformBox>(IsSelectedProperty);
        PressedMixin.Attach<TransformBox>();
        FocusableProperty.OverrideDefaultValue<TransformBox>(true);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
      
        this.GetObservable(TargetProperty).Subscribe(control =>
        {
            if (control != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    Width = control.Bounds.Width + AnchorSize * 2;
                    Height = control.Bounds.Height + AnchorSize * 2;

                    var width =  double.IsNaN(control.Width) ? "Auto" : control.Width.ToString();
                    var height = double.IsNaN(control.Height) ? "Auto" : control.Height.ToString();
                    Console.WriteLine($"Target ready: Bounds.Width:{control.Bounds.Width} Bounds.Height:{control.Bounds.Height} Current Width:{width} Height:{height}");
                    
                    //заглушка для начальной синхронизации позиции
                    Extensions.Layout.SetX(this, Extensions.Layout.GetX(Target) - AnchorSize);
                    Extensions.Layout.SetY(this, Extensions.Layout.GetY(Target) - AnchorSize);
                    
                    AddHandler(PointerPressedEvent, (s, pointerPressedEventArgs) =>
                    {
                        var lastMouseButton = pointerPressedEventArgs.GetCurrentPoint(this).Properties.PointerUpdateKind;
                        Console.WriteLine(lastMouseButton);
                    
                        if (ItemsControl.ItemsControlFromItemContainer(this) is InteractionLayer selecting)
                        {
                            selecting.TrySelectItem(this, pointerPressedEventArgs);
                            Focus();
                        }
                    
                    }, RoutingStrategies.Tunnel);
                    
                }, DispatcherPriority.Loaded);
            }
        });

        foreach (var thumb in this.GetTemplateChildren().OfType<Thumb>())
        {
            if (thumb.Classes.Contains("Anchor") && thumb.Tag is string tag &&
                Enum.TryParse<AnchorType>(tag, out var anchorType))
            {
                thumb.DragStarted += (_, _) =>
                {
                    PseudoClasses.Set(":resize", true);
                    CacheTargetState();
                };

                thumb.DragDelta += (_, ev) => HandleResize(anchorType, ev.Vector);

                thumb.DragCompleted += (_, _) =>
                {
                    PseudoClasses.Set(":resize", false);
                    ApplyTargetSize();
                    ApplyTargetPosition();
                };
            }
            else if (thumb.Name == "PART_MoveThumb")
            {
                thumb.DragStarted += (_, _) =>
                {
                    PseudoClasses.Set(":drag", true);
                    CacheTargetState();
                };

                thumb.DragDelta += (_, ev) => HandleMove(ev.Vector);

                thumb.DragCompleted += (_, _) =>
                {
                    PseudoClasses.Set(":drag", false);
                    ApplyTargetPosition();
                };
            }
        }
    }

    private void CacheTargetState()
    {
        if (Target == null)
            return;

        _targetWidth = Target.Bounds.Width;
        _targetHeight = Target.Bounds.Height;
        _targetPosX = Extensions.Layout.GetX(Target);
        _targetPosY = Extensions.Layout.GetY(Target);
    }


    private void ApplyTargetSize()
    {
        if (Target == null)
            return;
        
        Target.Width = _targetWidth;
        Target.Height = _targetHeight;
    }

    private void ApplyTargetPosition()
    {
        if (Target == null)
            return;
        
        Extensions.Layout.SetX(Target, _targetPosX);
        Extensions.Layout.SetY(Target, _targetPosY);
    }

    //Теперь HandleResize учитывает AnchorSize при проверке минимального размера — и не позволяет TransformBox стать меньше, чем minContentSize + AnchorSize * 2.
    private void HandleResize(AnchorType anchor, Vector delta)
    {
        const double minContentSize = 6;
        var minTotalWidth = minContentSize + AnchorSize * 2;
        var minTotalHeight = minContentSize + AnchorSize * 2;
        
        

        var dx = SnapToGrid(delta.X, GridStep.Width);
        var dy = SnapToGrid(delta.Y, GridStep.Height);

        var newWidth = Width;
        var newHeight = Height;
        var newX = Extensions.Layout.GetX(this);
        var newY = Extensions.Layout.GetY(this);

        if (anchor.ToString().Contains("Left"))
        {
            var widthCandidate = Width - dx;
            if (widthCandidate >= minTotalWidth)
            {
                newWidth = widthCandidate;
                newX += dx;

                _targetWidth = Math.Max(GridStep.Width, _targetWidth - dx);
                _targetPosX += dx;
            }
        }
        if (anchor.ToString().Contains("Right"))
        {
            var widthCandidate = Width + dx;
            if (widthCandidate >= minTotalWidth)
            {
                newWidth = widthCandidate;
                _targetWidth = Math.Max(GridStep.Width, _targetWidth + dx);
            }
        }
        if (anchor.ToString().Contains("Top"))
        {
            var heightCandidate = Height - dy;
            if (heightCandidate >= minTotalHeight)
            {
                newHeight = heightCandidate;
                newY += dy;

                _targetHeight = Math.Max(GridStep.Height, _targetHeight - dy);
                _targetPosY += dy;
            }
        }
        if (anchor.ToString().Contains("Bottom"))
        {
            var heightCandidate = Height + dy;
            if (heightCandidate >= minTotalHeight)
            {
                newHeight = heightCandidate;
                _targetHeight = Math.Max(GridStep.Height, _targetHeight + dy);
            }
        }

        Width = newWidth;
        Height = newHeight;

        Extensions.Layout.SetX(this, newX);
        Extensions.Layout.SetY(this, newY);
    }

    private void HandleMove(Vector delta)
    {
        var dx = SnapToGrid(delta.X, GridStep.Width);
        var dy = SnapToGrid(delta.Y, GridStep.Height);

        var x = Extensions.Layout.GetX(this);
        var y = Extensions.Layout.GetY(this);

        Extensions.Layout.SetX(this, x + dx);
        Extensions.Layout.SetY(this, y + dy);

        _targetPosX += dx;
        _targetPosY += dy;
    }

    private double SnapToGrid(double value, double gridSize)
    {
        return Math.Floor(value / gridSize) * gridSize;
    }

}
