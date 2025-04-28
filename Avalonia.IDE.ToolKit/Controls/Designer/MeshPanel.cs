using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Avalonia.IDE.ToolKit.Controls.Designer;

/// <summary>
/// Панель с поддержкой отрисовки сетки.
/// </summary>
public class MeshPanel : Control, IChildIndexProvider
{
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<MeshPanel>();

    private EventHandler<ChildIndexChangedEventArgs>? _childIndexChanged;

    static MeshPanel()
    {
        AffectsRender<MeshPanel>(
            BackgroundProperty,
            MeshSizeXProperty,
            MeshSizeYProperty,
            MeshBrushProperty,
            MeshThicknessProperty,
            DrawModeProperty
        );
    }

    public MeshPanel()
    {
        Children.CollectionChanged += ChildrenChanged;
    }

    [Content]
    public Avalonia.Controls.Controls Children { get; } = new();

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public static readonly StyledProperty<int> MeshSizeXProperty =
        AvaloniaProperty.Register<MeshPanel, int>(nameof(MeshSizeX), 8);

    public static readonly StyledProperty<int> MeshSizeYProperty =
        AvaloniaProperty.Register<MeshPanel, int>(nameof(MeshSizeY), 8);

    public static readonly StyledProperty<double> MeshThicknessProperty =
        AvaloniaProperty.Register<MeshPanel, double>(nameof(MeshThickness), 1.0);

    public static readonly StyledProperty<IBrush> MeshBrushProperty =
        AvaloniaProperty.Register<MeshPanel, IBrush>(nameof(MeshBrush), Brushes.Black);

    public static readonly StyledProperty<GridDrawMode> DrawModeProperty =
        AvaloniaProperty.Register<MeshPanel, GridDrawMode>(nameof(DrawMode), GridDrawMode.Dots);

    public int MeshSizeX
    {
        get => GetValue(MeshSizeXProperty);
        set => SetValue(MeshSizeXProperty, value);
    }

    public int MeshSizeY
    {
        get => GetValue(MeshSizeYProperty);
        set => SetValue(MeshSizeYProperty, value);
    }

    public double MeshThickness
    {
        get => GetValue(MeshThicknessProperty);
        set => SetValue(MeshThicknessProperty, value);
    }

    public IBrush MeshBrush
    {
        get => GetValue(MeshBrushProperty);
        set => SetValue(MeshBrushProperty, value);
    }

    public GridDrawMode DrawMode
    {
        get => GetValue(DrawModeProperty);
        set => SetValue(DrawModeProperty, value);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(Bounds.Size);

        if (Background is { } bg)
            context.FillRectangle(bg, bounds);

        DrawMesh(context, bounds);

        base.Render(context);
    }

    private void DrawMesh(DrawingContext context, Rect bounds)
    {
        if (MeshSizeX <= 0 || MeshSizeY <= 0 || MeshThickness <= 0 || MeshBrush == null)
            return;

        var scale = VisualRoot?.RenderScaling ?? 1.0;
        var stepX = MeshSizeX * scale;
        var stepY = MeshSizeY * scale;
        var thickness = MeshThickness * scale;

        if (DrawMode == GridDrawMode.Lines)
        {
            var pen = new Pen(MeshBrush, thickness)
            {
                DashStyle = new DashStyle(new double[] { 1, 5 }, 0)
            };

            for (double x = 0.5; x <= bounds.Width; x += stepX)
                context.DrawLine(pen, new Point(x, 0), new Point(x, bounds.Height));

            for (double y = 0.5; y <= bounds.Height; y += stepY)
                context.DrawLine(pen, new Point(0, y), new Point(bounds.Width, y));
        }
        else if (DrawMode == GridDrawMode.Dots)
        {
            var dotSize = new Size(thickness, thickness);

            for (double x = 0; x <= bounds.Width; x += stepX)
            {
                for (double y = 0; y <= bounds.Height; y += stepY)
                {
                    context.FillRectangle(MeshBrush, new Rect(new Point(x, y), dotSize));
                }
            }
        }
    }

    protected virtual void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                LogicalChildren.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Control>().ToList());
                VisualChildren.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Visual>());
                break;
            case NotifyCollectionChangedAction.Move:
                LogicalChildren.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                VisualChildren.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                LogicalChildren.RemoveAll(e.OldItems!.OfType<Control>().ToList());
                VisualChildren.RemoveAll(e.OldItems!.OfType<Visual>());
                break;
            case NotifyCollectionChangedAction.Replace:
                for (int i = 0; i < e.OldItems!.Count; i++)
                {
                    var index = i + e.OldStartingIndex;
                    var child = (Control)e.NewItems![i]!;
                    LogicalChildren[index] = child;
                    VisualChildren[index] = child;
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                throw new NotSupportedException("Reset not supported.");
        }

        _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.ChildIndexesReset);
        InvalidateMeasure();
    }

    int IChildIndexProvider.GetChildIndex(ILogical child)
    {
        return child is Control c ? Children.IndexOf(c) : -1;
    }

    bool IChildIndexProvider.TryGetTotalCount(out int count)
    {
        count = Children.Count;
        return true;
    }

    event EventHandler<ChildIndexChangedEventArgs>? IChildIndexProvider.ChildIndexChanged
    {
        add
        {
            if (_childIndexChanged == null)
                Children.PropertyChanged += ChildrenPropertyChanged;

            _childIndexChanged += value;
        }
        remove
        {
            _childIndexChanged -= value;

            if (_childIndexChanged == null)
                Children.PropertyChanged -= ChildrenPropertyChanged;
        }
    }

    private void ChildrenPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Children.Count) || e.PropertyName is null)
            _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.TotalCountChanged);
    }
}
