using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DesignerLibrary
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
    public partial class MyCustomControl : UserControl
    {
        /// <summary>
        /// Defines the X-axis grid size property.
        /// Определяет свойство размера сетки по оси X.
        /// </summary>
        public static readonly StyledProperty<int> GridSizeXProperty =
            AvaloniaProperty.Register<MyCustomControl, int>(nameof(GridSizeX), 8);

        /// <summary>
        /// Defines the Y-axis grid size property.
        /// Определяет свойство размера сетки по оси Y.
        /// </summary>
        public static readonly StyledProperty<int> GridSizeYProperty =
            AvaloniaProperty.Register<MyCustomControl, int>(nameof(GridSizeY), 8);

        /// <summary>
        /// Defines the drawing mode property for the grid (lines or dots).
        /// Определяет свойство режима рисования для сетки (линии или точки).
        /// </summary>
        public static readonly StyledProperty<GridDrawMode> DrawModeProperty =
            AvaloniaProperty.Register<MyCustomControl, GridDrawMode>(nameof(DrawMode), GridDrawMode.Dots);

        /// <summary>
        /// Defines the grid element size property (used for both lines and dots).
        /// Определяет свойство размера элемента сетки (используется как для линий, так и для точек).
        /// </summary>
        public static readonly StyledProperty<double> GridElementSizeProperty =
            AvaloniaProperty.Register<MyCustomControl, double>(nameof(GridElementSize), 1.0);

        /// <summary>
        /// Defines the grid color property.
        /// Определяет свойство цвета сетки.
        /// </summary>
        public static readonly StyledProperty<IBrush> GridColorProperty =
            AvaloniaProperty.Register<MyCustomControl, IBrush>(nameof(GridColor), Brushes.Black);

        /// <summary>
        /// Gets or sets the X-axis grid size.
        /// Получает или задает размер сетки по оси X.
        /// </summary>
        public int GridSizeX
        {
            get => GetValue(GridSizeXProperty);
            set => SetValue(GridSizeXProperty, value);
        }

        /// <summary>
        /// Gets or sets the Y-axis grid size.
        /// Получает или задает размер сетки по оси Y.
        /// </summary>
        public int GridSizeY
        {
            get => GetValue(GridSizeYProperty);
            set => SetValue(GridSizeYProperty, value);
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
        public double GridElementSize
        {
            get => GetValue(GridElementSizeProperty);
            set => SetValue(GridElementSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the grid color.
        /// Получает или задает цвет сетки.
        /// </summary>
        public IBrush GridColor
        {
            get => GetValue(GridColorProperty);
            set => SetValue(GridColorProperty, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyCustomControl"/> class.
        /// Инициализирует новый экземпляр класса <see cref="MyCustomControl"/>.
        /// </summary>
        public MyCustomControl()
        {
            InitializeComponent();
            
            PropertyChanged += (sender, args) =>
            {
                InvalidateVisual();
            };
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
            

            var gridSizeXInPixels = new PixelSize((int)Math.Round(GridSizeX * renderScaling), 0);
            var gridSizeYInPixels = new PixelSize(0, (int)Math.Round(GridSizeY * renderScaling));
            var gridElementSizeInPixels = GridElementSize * renderScaling;

            for (int x = 0; x < bounds.Width; x += gridSizeXInPixels.Width)
            {
                for (int y = 0; y < bounds.Height; y += gridSizeYInPixels.Height)
                {
                    if (DrawMode == GridDrawMode.Lines)
                    {
                        var pen = new Pen(GridColor, gridElementSizeInPixels);
                        context.DrawLine(pen, new Point(x, 0), new Point(x, bounds.Height));
                        context.DrawLine(pen, new Point(0, y), new Point(bounds.Width, y));
                    }
                    else if (DrawMode == GridDrawMode.Dots)
                    {
                        context.FillRectangle(GridColor, new Rect(new Point(x, y), new Size(gridElementSizeInPixels, gridElementSizeInPixels)));
                    }
                }
            }
        }
    }
}
