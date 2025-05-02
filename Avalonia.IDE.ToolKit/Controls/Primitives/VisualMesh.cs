using Avalonia.Controls;
using Avalonia.Media;

namespace Avalonia.IDE.ToolKit.Controls.Primitives;

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
/// Контрол, рисующий визуальную сетку.
/// Используется как базовый класс для панелей, собственных контролов и редакторов.
/// </summary>
public class VisualMesh : Control
{
    /// <summary>
    /// Размер ячейки сетки (ширина и высота).
    /// </summary>
    public static readonly StyledProperty<Size> MeshSizeProperty =
        AvaloniaProperty.Register<VisualMesh, Size>(nameof(MeshSize), new Size(8, 8));

    /// <summary>
    /// Режим отрисовки сетки (точки или пунктирные линии).
    /// </summary>
    public static readonly StyledProperty<GridDrawMode> DrawModeProperty =
        AvaloniaProperty.Register<VisualMesh, GridDrawMode>(nameof(DrawMode), GridDrawMode.Dots);

    /// <summary>
    /// Толщина линий или размер точек сетки.
    /// </summary>
    public static readonly StyledProperty<double> MeshThicknessProperty =
        AvaloniaProperty.Register<VisualMesh, double>(nameof(MeshThickness), 1.0);

    /// <summary>
    /// Кисть для отрисовки сетки.
    /// </summary>
    public static readonly StyledProperty<IBrush> MeshBrushProperty =
        AvaloniaProperty.Register<VisualMesh, IBrush>(nameof(MeshBrush), Brushes.Black);

    /// <summary>
    /// Фоновая кисть контрола.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<VisualMesh, IBrush?>(nameof(Background));

    static VisualMesh()
    {
        AffectsRender<VisualMesh>(
            MeshSizeProperty,
            DrawModeProperty,
            MeshThicknessProperty,
            MeshBrushProperty,
            BackgroundProperty
        );
    }

    /// <inheritdoc cref="MeshSizeProperty"/>
    public Size MeshSize
    {
        get => GetValue(MeshSizeProperty);
        set => SetValue(MeshSizeProperty, value);
    }

    /// <summary>
    /// Ширина ячейки сетки (обёртка над MeshSize.Width).
    /// </summary>
    public double MeshWidth
    {
        get => MeshSize.Width;
        set => MeshSize = new Size(value, MeshSize.Height);
    }

    /// <summary>
    /// Высота ячейки сетки (обёртка над MeshSize.Height).
    /// </summary>
    public double MeshHeight
    {
        get => MeshSize.Height;
        set => MeshSize = new Size(MeshSize.Width, value);
    }

    /// <inheritdoc cref="DrawModeProperty"/>
    public GridDrawMode DrawMode
    {
        get => GetValue(DrawModeProperty);
        set => SetValue(DrawModeProperty, value);
    }

    /// <inheritdoc cref="MeshThicknessProperty"/>
    public double MeshThickness
    {
        get => GetValue(MeshThicknessProperty);
        set => SetValue(MeshThicknessProperty, value);
    }

    /// <inheritdoc cref="MeshBrushProperty"/>
    public IBrush MeshBrush
    {
        get => GetValue(MeshBrushProperty);
        set => SetValue(MeshBrushProperty, value);
    }

    /// <inheritdoc cref="BackgroundProperty"/>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Отрисовывает сетку и фон.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(Bounds.Size);

        // Заливка фона
        if (Background is { } bg)
            context.FillRectangle(bg, bounds);

        if (MeshSize.Width <= 0 || MeshSize.Height <= 0 || MeshThickness <= 0)
            return;

        var scale = VisualRoot?.RenderScaling ?? 1.0;
        var stepX = MeshSize.Width * scale;
        var stepY = MeshSize.Height * scale;
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

        base.Render(context);
    }
}
