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
/// Контрол, рисующий визуальную сетку для помощи в позиционировании UI-элементов.
/// Используется в конструкторах форм или контролов.
/// </summary>
public class VisualMesh : Control
{
    /// <summary>
    /// Размер ячейки сетки (ширина и высота).
    /// </summary>
    public static readonly StyledProperty<Size> MeshSizeProperty =
        AvaloniaProperty.Register<VisualMesh, Size>(nameof(MeshSize), new Size(8, 8));

    /// <summary>
    /// Смещение сетки по X и Y.
    /// </summary>
    public static readonly StyledProperty<Point> MeshOffsetProperty =
        AvaloniaProperty.Register<VisualMesh, Point>(nameof(MeshOffset), new Point(0, 0));

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
    /// Прозрачность сетки (от 0 до 1).
    /// </summary>
    public static readonly StyledProperty<double> MeshOpacityProperty =
        AvaloniaProperty.Register<VisualMesh, double>(nameof(MeshOpacity), 1.0);

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

    private readonly Pen _pen;
    private GeometryGroup? _cachedDotGeometry;
    private Size _lastBounds;
    private Size _lastMeshSize;
    private Point _lastMeshOffset;
    private double _lastMeshThickness;

    static VisualMesh()
    {
        AffectsRender<VisualMesh>(
            MeshSizeProperty,
            MeshOffsetProperty,
            DrawModeProperty,
            MeshThicknessProperty,
            MeshOpacityProperty,
            MeshBrushProperty,
            BackgroundProperty
        );
    }

    public VisualMesh()
    {
        _pen = new Pen(MeshBrush, MeshThickness)
        {
            DashStyle = new DashStyle(new double[] { 1, 5 }, 0)
        };
    }

    /// <inheritdoc cref="MeshSizeProperty"/>
    public Size MeshSize
    {
        get => GetValue(MeshSizeProperty);
        set
        {
            if (value.Width <= 0 || value.Height <= 0)
                throw new ArgumentException("MeshSize dimensions must be positive.");
            SetValue(MeshSizeProperty, value);
            InvalidateCachedGeometry();
        }
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

    /// <inheritdoc cref="MeshOffsetProperty"/>
    public Point MeshOffset
    {
        get => GetValue(MeshOffsetProperty);
        set
        {
            SetValue(MeshOffsetProperty, value);
            InvalidateCachedGeometry();
        }
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
        set
        {
            if (value <= 0)
                throw new ArgumentException("MeshThickness must be positive.");
            SetValue(MeshThicknessProperty, value);
            _pen.Thickness = value * (VisualRoot?.RenderScaling ?? 1.0);
            InvalidateCachedGeometry();
        }
    }

    /// <inheritdoc cref="MeshOpacityProperty"/>
    public double MeshOpacity
    {
        get => GetValue(MeshOpacityProperty);
        set
        {
            if (value < 0 || value > 1)
                throw new ArgumentException("MeshOpacity must be between 0 and 1.");
            SetValue(MeshOpacityProperty, value);
        }
    }

    /// <inheritdoc cref="MeshBrushProperty"/>
    public IBrush MeshBrush
    {
        get => GetValue(MeshBrushProperty);
        set
        {
            SetValue(MeshBrushProperty, value);
            _pen.Brush = value;
        }
    }

    /// <inheritdoc cref="BackgroundProperty"/>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Сбрасывает кэшированную геометрию для режима Dots.
    /// </summary>
    private void InvalidateCachedGeometry()
    {
        _cachedDotGeometry = null;
    }

    /// <summary>
    /// Возвращает ближайшую точку сетки для заданной позиции.
    /// </summary>
    /// <param name="position">Позиция в координатах контрола.</param>
    /// <returns>Ближайшая точка сетки.</returns>
    public Point GetNearestGridPoint(Point position)
    {
        if (MeshSize.Width <= 0 || MeshSize.Height <= 0)
            return position;

        var scale = VisualRoot?.RenderScaling ?? 1.0;
        var offsetX = MeshOffset.X * scale;
        var offsetY = MeshOffset.Y * scale;
        var stepX = MeshSize.Width * scale;
        var stepY = MeshSize.Height * scale;

        var x = Math.Round((position.X - offsetX) / stepX) * stepX + offsetX;
        var y = Math.Round((position.Y - offsetY) / stepY) * stepY + offsetY;

        return new Point(x, y);
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
        var offsetX = MeshOffset.X * scale;
        var offsetY = MeshOffset.Y * scale;

        // Выравнивание для чёткости
        var pixelOffset = thickness % 2 == 0 ? 0.0 : 0.5;

        // Применение прозрачности
        using (context.PushOpacity(MeshOpacity))
        {
            if (DrawMode == GridDrawMode.Lines)
            {
                for (double x = pixelOffset + offsetX; x <= bounds.Width; x += stepX)
                    context.DrawLine(_pen, new Point(x, 0), new Point(x, bounds.Height));

                for (double y = pixelOffset + offsetY; y <= bounds.Height; y += stepY)
                    context.DrawLine(_pen, new Point(0, y), new Point(bounds.Width, y));
            }
            else if (DrawMode == GridDrawMode.Dots)
            {
                if (_cachedDotGeometry == null || _lastBounds != bounds.Size ||
                    _lastMeshSize != MeshSize || _lastMeshOffset != MeshOffset ||
                    _lastMeshThickness != MeshThickness)
                {
                    _cachedDotGeometry = new GeometryGroup();
                    var dotSize = new Size(thickness, thickness);

                    for (double x = pixelOffset + offsetX; x <= bounds.Width; x += stepX)
                    {
                        for (double y = pixelOffset + offsetY; y <= bounds.Height; y += stepY)
                        {
                            var rect = new Rect(new Point(x - thickness / 2, y - thickness / 2), dotSize);
                            _cachedDotGeometry.Children.Add(new RectangleGeometry(rect));
                        }
                    }

                    _lastBounds = bounds.Size;
                    _lastMeshSize = MeshSize;
                    _lastMeshOffset = MeshOffset;
                    _lastMeshThickness = MeshThickness;
                }

                context.DrawGeometry(MeshBrush, null, _cachedDotGeometry);
            }
        }

        base.Render(context);
    }
}