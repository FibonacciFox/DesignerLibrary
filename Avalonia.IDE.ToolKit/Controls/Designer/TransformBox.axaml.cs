using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;

namespace Avalonia.IDE.ToolKit.Controls.Designer;

/// <summary>
/// Контрол для визуального редактирования размеров и положения другого контрола.
/// </summary>
[PseudoClasses(":selected")]
public class TransformBox : TemplatedControl
{
    public static readonly StyledProperty<double> StepSizeByXProperty =
        AvaloniaProperty.Register<TransformBox, double>(nameof(StepSizeByX), 8);

    public static readonly StyledProperty<double> StepSizeByYProperty =
        AvaloniaProperty.Register<TransformBox, double>(nameof(StepSizeByY), 8);

    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<TransformBox>();

    public static readonly StyledProperty<Control> AttachedControlProperty =
        AvaloniaProperty.Register<TransformBox, Control>(nameof(AttachedControl));

    public static readonly StyledProperty<int> AnchorSizeProperty =
        AvaloniaProperty.Register<TransformBox, int>(nameof(AnchorSize), 6);

    public new static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<TransformBox, double>(nameof(BorderThickness));

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

    public int AnchorSize
    {
        get => GetValue(AnchorSizeProperty);
        set => SetValue(AnchorSizeProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        foreach (var thumb in this.GetTemplateChildren().OfType<Thumb>())
        {
            if (thumb.Classes.Contains("Anchor") && thumb.Tag is string tag &&
                Enum.TryParse<AnchorType>(tag, out var anchorType))
            {
                thumb.DragDelta += (s, ev) => HandleResize(anchorType, ev.Vector);
            }
            else if (thumb.Name == "PART_MoveThumb")
            {
                thumb.DragDelta += (s, ev) => HandleMove(ev.Vector);
            }
        }
    }

    private void HandleResize(AnchorType anchor, Vector delta)
    {
        var dx = SnapToGrid(delta.X, StepSizeByX);
        var dy = SnapToGrid(delta.Y, StepSizeByY);

        var newWidth = Width;
        var newHeight = Height;
        var newX = Layout.GetX(this);
        var newY = Layout.GetY(this);

        if (anchor.ToString().Contains("Left"))
        {
            newWidth = Math.Max(StepSizeByX, Width - dx);
            newX += dx;
        }
        if (anchor.ToString().Contains("Right"))
        {
            newWidth = Math.Max(StepSizeByX, Width + dx);
        }
        if (anchor.ToString().Contains("Top"))
        {
            newHeight = Math.Max(StepSizeByY, Height - dy);
            newY += dy;
        }
        if (anchor.ToString().Contains("Bottom"))
        {
            newHeight = Math.Max(StepSizeByY, Height + dy);
        }

        Width = newWidth;
        Height = newHeight;
        Layout.SetX(this, newX);
        Layout.SetY(this, newY);
    }

    private void HandleMove(Vector delta)
    {
        var dx = SnapToGrid(delta.X, StepSizeByX);
        var dy = SnapToGrid(delta.Y, StepSizeByY);

        var x = Layout.GetX(this);
        var y = Layout.GetY(this);

        Layout.SetX(this, x + dx);
        Layout.SetY(this, y + dy);
    }

    private double SnapToGrid(double value, double gridSize)
    {
        return Math.Round(value / gridSize) * gridSize;
    }
}