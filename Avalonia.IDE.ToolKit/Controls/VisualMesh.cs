using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Avalonia.IDE.ToolKit.Controls;

/// <summary>
/// Способы отрисовки сетки.
/// </summary>
public enum GridDrawMode
{
    /// <summary>
    /// Пунктирные линии.
    /// </summary>
    Lines,

    /// <summary>
    /// Точки.
    /// </summary>
    Dots
}

/// <summary>
/// Контрол для отрисовки визуальной сетки.
/// </summary>
public class VisualMesh : TemplatedControl
{
    public static readonly StyledProperty<int> MeshSizeXProperty =
        AvaloniaProperty.Register<VisualMesh, int>(nameof(MeshSizeX), 8);

    public static readonly StyledProperty<int> MeshSizeYProperty =
        AvaloniaProperty.Register<VisualMesh, int>(nameof(MeshSizeY), 8);

    public static readonly StyledProperty<GridDrawMode> DrawModeProperty =
        AvaloniaProperty.Register<VisualMesh, GridDrawMode>(nameof(DrawMode), GridDrawMode.Dots);

    public static readonly StyledProperty<double> MeshThicknessProperty =
        AvaloniaProperty.Register<VisualMesh, double>(nameof(MeshThickness), 1.0);

    public static readonly StyledProperty<IBrush> MeshBrushProperty =
        AvaloniaProperty.Register<VisualMesh, IBrush>(nameof(MeshBrush), Brushes.Black);

    static VisualMesh()
    {
        AffectsRender<VisualMesh>(
            MeshSizeXProperty,
            MeshSizeYProperty,
            DrawModeProperty,
            MeshThicknessProperty,
            MeshBrushProperty,
            BackgroundProperty // Чтобы фон тоже перерисовывался при изменении
        );
    }

    /// <summary>
    /// Ширина ячейки сетки.
    /// </summary>
    public int MeshSizeX
    {
        get => GetValue(MeshSizeXProperty);
        set => SetValue(MeshSizeXProperty, value);
    }

    /// <summary>
    /// Высота ячейки сетки.
    /// </summary>
    public int MeshSizeY
    {
        get => GetValue(MeshSizeYProperty);
        set => SetValue(MeshSizeYProperty, value);
    }

    /// <summary>
    /// Режим отрисовки сетки.
    /// </summary>
    public GridDrawMode DrawMode
    {
        get => GetValue(DrawModeProperty);
        set => SetValue(DrawModeProperty, value);
    }

    /// <summary>
    /// Толщина линий или размер точек.
    /// </summary>
    public double MeshThickness
    {
        get => GetValue(MeshThicknessProperty);
        set => SetValue(MeshThicknessProperty, value);
    }

    /// <summary>
    /// Кисть для рисования сетки.
    /// </summary>
    public IBrush MeshBrush
    {
        get => GetValue(MeshBrushProperty);
        set => SetValue(MeshBrushProperty, value);
    }

    /// <summary>
    /// Отрисовывает сетку и фон.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = new Rect(Bounds.Size);

        // Заливаем фон
        if (Background != null)
        {
            context.FillRectangle(Background, bounds);
        }

        if (MeshSizeX <= 0 || MeshSizeY <= 0 || MeshThickness <= 0)
            return;

        var scale = VisualRoot?.RenderScaling ?? 1.0;
        int stepX = (int)Math.Round(MeshSizeX * scale);
        int stepY = (int)Math.Round(MeshSizeY * scale);
        double thickness = MeshThickness * scale;

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
}
