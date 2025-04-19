using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Avalonia.IDE.ToolKit.Controls
{
    /// <summary>
    /// Specifies the rendering mode for the grid.
    /// Определяет режим отрисовки сетки.
    /// </summary>
    public enum GridDrawMode
    {
        /// <summary>
        /// Draw the grid using dashed lines.
        /// Отображать сетку с помощью пунктирных линий.
        /// </summary>
        Lines,

        /// <summary>
        /// Draw the grid using dots.
        /// Отображать сетку точками.
        /// </summary>
        Dots
    }

    /// <summary>
    /// Represents a custom control that renders a configurable visual grid.
    /// Пользовательский контрол для отрисовки настраиваемой визуальной сетки.
    /// </summary>
    public class VisualMesh : TemplatedControl
    {
        /// <summary>
        /// Identifies the <see cref="MeshSizeX"/> property.
        /// Идентификатор свойства <see cref="MeshSizeX"/>.
        /// </summary>
        public static readonly StyledProperty<int> MeshSizeXProperty =
            AvaloniaProperty.Register<VisualMesh, int>(nameof(MeshSizeX), 8);

        /// <summary>
        /// Identifies the <see cref="MeshSizeY"/> property.
        /// Идентификатор свойства <see cref="MeshSizeY"/>.
        /// </summary>
        public static readonly StyledProperty<int> MeshSizeYProperty =
            AvaloniaProperty.Register<VisualMesh, int>(nameof(MeshSizeY), 8);

        /// <summary>
        /// Identifies the <see cref="DrawMode"/> property.
        /// Идентификатор свойства <see cref="DrawMode"/>.
        /// </summary>
        public static readonly StyledProperty<GridDrawMode> DrawModeProperty =
            AvaloniaProperty.Register<VisualMesh, GridDrawMode>(nameof(DrawMode), GridDrawMode.Dots);

        /// <summary>
        /// Identifies the <see cref="MeshThickness"/> property.
        /// Идентификатор свойства <see cref="MeshThickness"/>.
        /// </summary>
        public static readonly StyledProperty<double> MeshThicknessProperty =
            AvaloniaProperty.Register<VisualMesh, double>(nameof(MeshThickness), 1.0);

        /// <summary>
        /// Identifies the <see cref="MeshBrush"/> property.
        /// Идентификатор свойства <see cref="MeshBrush"/>.
        /// </summary>
        public static readonly StyledProperty<IBrush> MeshBrushProperty =
            AvaloniaProperty.Register<VisualMesh, IBrush>(nameof(MeshBrush), Brushes.Black);

        /// <summary>
        /// Gets or sets the grid cell width in pixels.
        /// Получает или задает ширину ячейки сетки в пикселях.
        /// </summary>
        public int MeshSizeX
        {
            get => GetValue(MeshSizeXProperty);
            set => SetValue(MeshSizeXProperty, value);
        }

        /// <summary>
        /// Gets or sets the grid cell height in pixels.
        /// Получает или задает высоту ячейки сетки в пикселях.
        /// </summary>
        public int MeshSizeY
        {
            get => GetValue(MeshSizeYProperty);
            set => SetValue(MeshSizeYProperty, value);
        }

        /// <summary>
        /// Gets or sets the rendering mode for the grid (lines or dots).
        /// Получает или задает режим отрисовки сетки (линии или точки).
        /// </summary>
        public GridDrawMode DrawMode
        {
            get => GetValue(DrawModeProperty);
            set => SetValue(DrawModeProperty, value);
        }

        /// <summary>
        /// Gets or sets the thickness of grid lines or dot size.
        /// Получает или задает толщину линий сетки или размер точки.
        /// </summary>
        public double MeshThickness
        {
            get => GetValue(MeshThicknessProperty);
            set => SetValue(MeshThicknessProperty, value);
        }

        /// <summary>
        /// Gets or sets the brush used to draw the grid.
        /// Получает или задает кисть, используемую для отрисовки сетки.
        /// </summary>
        public IBrush MeshBrush
        {
            get => GetValue(MeshBrushProperty);
            set => SetValue(MeshBrushProperty, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualMesh"/> class.
        /// Инициализирует новый экземпляр класса <see cref="VisualMesh"/>.
        /// </summary>
        public VisualMesh()
        {
            foreach (var prop in new AvaloniaProperty[]
            {
                MeshSizeXProperty, MeshSizeYProperty,
                DrawModeProperty, MeshThicknessProperty,
                MeshBrushProperty
            })
            {
                prop.Changed.AddClassHandler<VisualMesh>((o, _) => o.InvalidateVisual());
            }
        }

        /// <summary>
        /// Renders the grid using the specified drawing context.
        /// Отрисовывает сетку, используя указанный контекст рисования.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (MeshSizeX <= 0 || MeshSizeY <= 0 || MeshThickness <= 0)
                return;

            var bounds = new Rect(Bounds.Size);
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
}
