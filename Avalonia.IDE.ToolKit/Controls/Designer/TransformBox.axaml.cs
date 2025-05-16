using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;

namespace Avalonia.IDE.ToolKit.Controls.Designer;

/// <summary>
/// Контрол для визуального редактирования размеров и положения другого контрола.
/// </summary>
[PseudoClasses(":selected", ":drag", ":resize")]
public class TransformBox : TemplatedControl, ISelectable
{
    public static readonly StyledProperty<Size> GridStepProperty =
        AvaloniaProperty.Register<TransformBox, Size>(nameof(GridStep), new Size(8, 8));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<TransformBox>();

    public static readonly StyledProperty<Control> AttachedControlProperty =
        AvaloniaProperty.Register<TransformBox, Control>(nameof(AttachedControl));

    public static readonly StyledProperty<int> AnchorSizeProperty =
        AvaloniaProperty.Register<TransformBox, int>(nameof(AnchorSize), 6);

    public new static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<TransformBox, double>(nameof(BorderThickness));

    public Size GridStep
    {
        get => GetValue(GridStepProperty);
        set => SetValue(GridStepProperty, value);
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
                thumb.DragStarted += (_, _) => PseudoClasses.Set(":resize", true);
                thumb.DragDelta += (_, ev) => HandleResize(anchorType, ev.Vector);
                thumb.DragCompleted += (_, _) => PseudoClasses.Set(":resize", false);
            }
            else if (thumb.Name == "PART_MoveThumb")
            {
                thumb.DragStarted += (_, _) => PseudoClasses.Set(":drag", true);
                thumb.DragDelta += (_, ev) => HandleMove(ev.Vector);
                thumb.DragCompleted += (_, _) => PseudoClasses.Set(":drag", false);
            }
        }
    }

    private void HandleResize(AnchorType anchor, Vector delta)
    {
        var dx = SnapToGrid(delta.X, GridStep.Width);
        var dy = SnapToGrid(delta.Y, GridStep.Height);

        var newWidth = Width;
        var newHeight = Height;
        var newX = Layout.GetX(this);
        var newY = Layout.GetY(this);

        if (anchor.ToString().Contains("Left"))
        {
            newWidth = Math.Max(GridStep.Width, Width - dx);
            newX += dx;
        }
        if (anchor.ToString().Contains("Right"))
        {
            newWidth = Math.Max(GridStep.Width, Width + dx);
        }
        if (anchor.ToString().Contains("Top"))
        {
            newHeight = Math.Max(GridStep.Height, Height - dy);
            newY += dy;
        }
        if (anchor.ToString().Contains("Bottom"))
        {
            newHeight = Math.Max(GridStep.Height, Height + dy);
        }

        Width = newWidth;
        Height = newHeight;
        Layout.SetX(this, newX);
        Layout.SetY(this, newY);
    }

    private void HandleMove(Vector delta)
    {
        var dx = SnapToGrid(delta.X, GridStep.Width);
        var dy = SnapToGrid(delta.Y, GridStep.Height);

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
