using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Avalonia.IDE.ToolKit.Controls
{
    /// <summary>
    /// A control that allows content to be scaled and scrolled.
    /// Контрол, который позволяет масштабировать и прокручивать содержимое.
    /// </summary>
    public class ScalingContainer : TemplatedControl, IScrollable
    {
        public static readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<ScalingContainer, object>(nameof(Content));
        
        public static readonly StyledProperty<double> ScaleFactorProperty =
            AvaloniaProperty.Register<ScalingContainer, double>(nameof(ScaleFactor), 1.0, coerce: CoerceScaleFactor);
        
        public static readonly StyledProperty<Size> ExtentProperty =
            AvaloniaProperty.Register<ScalingContainer, Size>(nameof(Extent));
        
        public static readonly StyledProperty<Vector> OffsetProperty =
            ScrollViewer.OffsetProperty.AddOwner<ScalingContainer>();
        
        public static readonly StyledProperty<Size> ViewportProperty =
            AvaloniaProperty.Register<ScalingContainer, Size>(nameof(Viewport));
        
        public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
            ContentControl.HorizontalContentAlignmentProperty.AddOwner<ScalingContainer>();
        
        public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
            ContentControl.VerticalContentAlignmentProperty.AddOwner<ScalingContainer>();
        
        public static readonly AttachedProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
            ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner<ScalingContainer>();
        
        public static readonly AttachedProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
            ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner<ScalingContainer>();
        
        public static readonly AttachedProperty<bool> AllowAutoHideProperty =
            ScrollViewer.AllowAutoHideProperty.AddOwner<ScalingContainer>();
        
        public static readonly RoutedEvent<ScrollChangedEventArgs> ScrollChangedEvent =
            RoutedEvent.Register<ScalingContainer, ScrollChangedEventArgs>(nameof(ScrollChanged), RoutingStrategies.Bubble);
        
        static ScalingContainer()
        {
            ScaleFactorProperty.Changed.AddClassHandler<ScalingContainer>((x, e) => x.OnScaleFactorChanged(e));
        }
        
        /// <summary>
        /// Called when the ScaleFactor property changes.
        /// Вызывается при изменении свойства ScaleFactor.
        /// </summary>
        /// <param name="e">The event data. Данные события.</param>
        private void OnScaleFactorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            ScaleContent((double)(e.NewValue ?? throw new InvalidOperationException()));
        }
        
        /// <summary>
        /// Scales the content by the specified factor.
        /// Масштабирует содержимое на указанный коэффициент.
        /// </summary>
        /// <param name="scaleFactor">The scale factor. Коэффициент масштаба.</param>
        private void ScaleContent(double scaleFactor)
        {
            if (_partLayoutTransform == null)
            {
                return;
            }

            var scaleTransform = new ScaleTransform(scaleFactor, scaleFactor);
            _partLayoutTransform.LayoutTransform = scaleTransform;
            _partLayoutTransform.InvalidateMeasure();
        }
        
        [Content]
        public object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty!, value);
        }
        
        /// <summary>
        /// Gets or sets the horizontal scrollbar visibility.
        /// Получает или задает видимость горизонтальной полосы прокрутки.
        /// </summary>
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => GetValue(HorizontalScrollBarVisibilityProperty);
            set => SetValue(HorizontalScrollBarVisibilityProperty, value);
        }

        /// <summary>
        /// Gets or sets the vertical scrollbar visibility.
        /// Получает или задает видимость вертикальной полосы прокрутки.
        /// </summary>
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => GetValue(VerticalScrollBarVisibilityProperty);
            set => SetValue(VerticalScrollBarVisibilityProperty, value);
        }
        
        /// <summary>
        /// Gets or sets the horizontal alignment of the content within the control.
        /// Получает или задает горизонтальное выравнивание содержимого внутри контрола.
        /// </summary>
        public HorizontalAlignment HorizontalContentAlignment
        {
            get => GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value);
        }
        
        /// <summary>
        /// Gets or sets the vertical alignment of the content within the control.
        /// Получает или задает вертикальное выравнивание содержимого внутри контрола.
        /// </summary>
        public VerticalAlignment VerticalContentAlignment
        {
            get => GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }
        
        /// <summary>
        /// Gets the extent of the scrollable content.
        /// Получает размер прокручиваемого содержимого.
        /// </summary>
        public Size Extent => GetValue(ExtentProperty);
        
        /// <summary>
        /// Gets or sets the current scroll offset.
        /// Получает или задает текущее смещение прокрутки.
        /// </summary>
        public Vector Offset
        {
            get => GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }
        
        /// <summary>
        /// Gets the size of the viewport on the scrollable content.
        /// Получает размер области просмотра прокручиваемого содержимого.
        /// </summary>
        public Size Viewport => GetValue(ViewportProperty);
        
        /// <summary>
        /// Gets a value that indicates whether scrollbars can hide itself when user is not interacting with it.
        /// Получает значение, указывающее, могут ли полосы прокрутки скрываться, когда пользователь с ними не взаимодействует.
        /// </summary>
        public bool AllowAutoHide
        {
            get => GetValue(AllowAutoHideProperty);
            set => SetValue(AllowAutoHideProperty, value);
        }
        
        /// <summary>
        /// Gets or sets the scale factor.
        /// Получает или задает коэффициент масштаба.
        /// </summary>
        public double ScaleFactor
        {
            get => GetValue(ScaleFactorProperty);
            set => SetValue(ScaleFactorProperty, value);
        }
        
        private static double CoerceScaleFactor(AvaloniaObject sender, double value)
        {
            // Limit the scale factor between 1 and 10 to prevent excessive scaling.
            // Ограничиваем коэффициент масштаба в пределах от 1 до 10 для предотвращения чрезмерного масштабирования.
            return Math.Max(1.0, Math.Min(value, 10.0));
        }

        /// <summary>
        /// Occurs when the scroll changes.
        /// Происходит при изменении прокрутки.
        /// </summary>
        public event EventHandler<ScrollChangedEventArgs> ScrollChanged
        {
            add => AddHandler(ScrollChangedEvent, value);
            remove => RemoveHandler(ScrollChangedEvent, value);
        }

        /// <summary>
        /// Raises the ScrollChanged event.
        /// Вызывает событие ScrollChanged.
        /// </summary>
        /// <param name="e">The event data. Данные события.</param>
        protected virtual void OnScrollChanged(ScrollChangedEventArgs e)
        {
            RaiseEvent(e);
        }

        /// <summary>
        /// Handles the ScrollViewer's ScrollChanged event.
        /// Обрабатывает событие ScrollChanged от ScrollViewer.
        /// </summary>
        /// <param name="sender">The event sender. Отправитель события.</param>
        /// <param name="e">The event data. Данные события.</param>
        private void ScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            var scrollChangedEventArgs = new ScrollChangedEventArgs(
                ScrollChangedEvent,
                e.ExtentDelta,
                e.OffsetDelta,
                e.ViewportDelta);
            OnScrollChanged(scrollChangedEventArgs);
        }

        /// <summary>
        /// Applies the control's template.
        /// Применяет шаблон контрола.
        /// </summary>
        /// <param name="e">The event data. Данные события.</param>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            
            _partLayoutTransform = e.NameScope.Find<LayoutTransformControl>("PART_LayoutTransform");
            _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");

            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }

            // Apply the scale only after the template has been applied.
            // Применяем масштаб только после того, как шаблон был применен.
            ScaleContent(ScaleFactor);
        }

        /// <summary>
        /// Called when the control is detached from the visual tree.
        /// Вызывается при отсоединении контрола от визуального дерева.
        /// </summary>
        /// <param name="e">The event data. Данные события.</param>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }
        }

        private LayoutTransformControl? _partLayoutTransform;
        private ScrollViewer? _scrollViewer;
    }
}
