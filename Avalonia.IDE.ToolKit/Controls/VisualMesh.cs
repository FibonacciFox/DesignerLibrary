using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Avalonia.IDE.ToolKit.Controls
{
    /// <summary>
    /// Enum defining the drawing mode for the grid.
    /// Перечисление, определяющее режим рисования для сетки.
    /// </summary>
    public enum GridDrawMode
    {
        Lines,  // Линии
        Dots    // Точки
    }

    /// <summary>
    /// Custom control that draws a grid with configurable properties such as size, color, and drawing mode.
    /// Пользовательский контрол, который рисует сетку с настраиваемыми свойствами, такими как размер, цвет и режим рисования.
    /// </summary>
    public class VisualMesh : TemplatedControl
    {
        /// <summary>
        /// Defines the X-axis grid size property.
        /// Определяет свойство размера сетки по оси X.
        /// </summary>
        public static readonly StyledProperty<int> MeshSizeXProperty =
            AvaloniaProperty.Register<VisualMesh, int>(nameof(MeshSizeX), 8);

        /// <summary>
        /// Defines the Y-axis grid size property.
        /// Определяет свойство размера сетки по оси Y.
        /// </summary>
        public static readonly StyledProperty<int> MeshSizeYProperty =
            AvaloniaProperty.Register<VisualMesh, int>(nameof(MeshSizeY), 8);

        /// <summary>
        /// Defines the drawing mode property for the grid (lines or dots).
        /// Определяет свойство режима рисования для сетки (линии или точки).
        /// </summary>
        public static readonly StyledProperty<GridDrawMode> DrawModeProperty =
            AvaloniaProperty.Register<VisualMesh, GridDrawMode>(nameof(DrawMode), GridDrawMode.Dots);

        /// <summary>
        /// Defines the grid element size property (used for both lines and dots).
        /// Определяет свойство размера элемента сетки (используется как для линий, так и для точек).
        /// </summary>
        public static readonly StyledProperty<double> MeshThicknessProperty =
            AvaloniaProperty.Register<VisualMesh, double>(nameof(MeshThickness), 1.0);

        /// <summary>
        /// Defines the grid color property.
        /// Определяет свойство цвета сетки.
        /// </summary>
        public static readonly StyledProperty<IBrush> MeshBrushProperty =
            AvaloniaProperty.Register<VisualMesh, IBrush>(nameof(MeshBrush), Brushes.Black);

        /// <summary>
        /// Gets or sets the X-axis grid size.
        /// Получает или задает размер сетки по оси X.
        /// </summary>
        public int MeshSizeX
        {
            get => GetValue(MeshSizeXProperty);
            set => SetValue(MeshSizeXProperty, value);
        }

        /// <summary>
        /// Gets or sets the Y-axis grid size.
        /// Получает или задает размер сетки по оси Y.
        /// </summary>
        public int MeshSizeY
        {
            get => GetValue(MeshSizeYProperty);
            set => SetValue(MeshSizeYProperty, value);
        }

        /// <summary>
        /// Gets or sets the drawing mode (lines or dots).
        /// Получает или задает режим рисования (линии или точки).
        /// </summary>
        public GridDrawMode DrawMode
        {
            get => GetValue(DrawModeProperty);
            set => SetValue(DrawModeProperty, value);
        }

        /// <summary>
        /// Gets or sets the grid element size.
        /// Получает или задает размер элемента сетки.
        /// </summary>
        public double MeshThickness
        {
            get => GetValue(MeshThicknessProperty);
            set => SetValue(MeshThicknessProperty, value);
        }

        /// <summary>
        /// Gets or sets the grid color.
        /// Получает или задает цвет сетки.
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
            // Подписываемся на изменения свойств, чтобы перерисовывать контрол при их изменении
            MeshSizeXProperty.Changed.AddClassHandler<VisualMesh>((o, e) => o.InvalidateVisual());
            MeshSizeYProperty.Changed.AddClassHandler<VisualMesh>((o, e) => o.InvalidateVisual());
            DrawModeProperty.Changed.AddClassHandler<VisualMesh>((o, e) => o.InvalidateVisual());
            MeshThicknessProperty.Changed.AddClassHandler<VisualMesh>((o, e) => o.InvalidateVisual());
            MeshBrushProperty.Changed.AddClassHandler<VisualMesh>((o, e) => o.InvalidateVisual());
        }

        /// <summary>
        /// Renders the grid based on the defined properties.
        /// Отрисовывает сетку на основе заданных свойств.
        /// </summary>
        /// <param name="context">Drawing context.</param>
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var bounds = new Rect(Bounds.Size);
            var renderScaling = VisualRoot?.RenderScaling ?? 1.0;
            
            // Создаем стиль штриховки
            var dashStyle = new DashStyle(new double[] { 1, 5 }, 0); // Пунктирная линия с чередованием 5 пикселя

            var gridSizeXInPixels = new PixelSize((int)Math.Round(MeshSizeX * renderScaling), 0);
            var gridSizeYInPixels = new PixelSize(0, (int)Math.Round(MeshSizeY * renderScaling));
            var gridElementSizeInPixels = MeshThickness * renderScaling;

            for (int x = 0; x < bounds.Width; x += gridSizeXInPixels.Width)
            {
                for (int y = 0; y < bounds.Height; y += gridSizeYInPixels.Height)
                {
                    if (DrawMode == GridDrawMode.Lines)
                    {
                        var pen = new Pen(MeshBrush, gridElementSizeInPixels){DashStyle = dashStyle};
                        context.DrawLine(pen, new Point(x, 0), new Point(x, bounds.Height));
                        context.DrawLine(pen, new Point(0, y), new Point(bounds.Width, y));
                    }
                    else if (DrawMode == GridDrawMode.Dots)
                    {
                        context.FillRectangle(MeshBrush, new Rect(new Point(x, y), new Size(gridElementSizeInPixels, gridElementSizeInPixels)));
                    }
                }
            }
        }
    }
}
