using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;

namespace Avalonia.IDE.ToolKit.Controls.Designer
{
    public class VisualEditingLayerItem : TemplatedControl, ISelectable
    {
        public static readonly StyledProperty<double> StepSizeByXProperty =
            AvaloniaProperty.Register<VisualEditingLayerItem, double>(nameof(StepSizeByX), 8);

        public static readonly StyledProperty<double> StepSizeByYProperty =
            AvaloniaProperty.Register<VisualEditingLayerItem, double>(nameof(StepSizeByY), 8);

        public static readonly StyledProperty<bool> IsSelectedProperty =
            SelectingItemsControl.IsSelectedProperty.AddOwner<ListBoxItem>();

        public static readonly StyledProperty<Control> AttachedControlProperty =
            AvaloniaProperty.Register<VisualEditingLayerItem, Control>(nameof(AttachedControl));
        
        public new static readonly StyledProperty<double> BorderThicknessProperty =
            AvaloniaProperty.Register<VisualEditingLayerItem, double>(nameof(BorderThickness));
        
        static VisualEditingLayerItem()
        {
            AttachedControlProperty.Changed.AddClassHandler<VisualEditingLayerItem>((x, e) => x.OnAttachedControlChanged(e));
        }
        
        private IDisposable? _boundsSubscription;
        
        private void OnAttachedControlChanged(AvaloniaPropertyChangedEventArgs attachedControlChangedEventArgs)
        {
            // Отписка от предыдущего контрола, если он существует
            _boundsSubscription?.Dispose();
            
            // Подписка на изменение Bounds нового контрола
            if (attachedControlChangedEventArgs.NewValue is Control newControl)
            {
                _boundsSubscription = newControl.GetObservable(BoundsProperty)
                    .Subscribe(new AnonymousObserver<Rect>(OnAttachedControlBoundsChanged));
            }
        }
        
        // Обработчик изменений Bounds AttachedControl
        private void OnAttachedControlBoundsChanged(Rect bounds)
        {
            Width = bounds.Width;
            Height = bounds.Height;
            
            var relativePositionToParent = AttachedControl.TranslatePoint(new Point(0, 0), (Parent as Visual)!);
            
            if (relativePositionToParent.HasValue)
            {
                Canvas.SetLeft(this, relativePositionToParent.Value.X);
                Canvas.SetTop(this, relativePositionToParent.Value.Y);

                Console.WriteLine($"{relativePositionToParent.Value.X}-{relativePositionToParent.Value.Y}");
            }
        }
        
        /// <summary>
        /// Gets or sets the thickness of the control's border.
        /// </summary>
        public new double BorderThickness
        {
            get => GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

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

        private bool _isResizing;
        private PointerPoint _startPoint;
        private double _originalWidth;
        private double _originalHeight;
        private double _originalLeft;
        private double _originalTop;
        private Control? _currentAnchor;
        private Rectangle? _partCustomBorder;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            
            var anchorNames = new[]
            {
                "TopLeftAnchor", "TopRightAnchor", "BottomLeftAnchor", "BottomRightAnchor",
                "TopCenterAnchor", "BottomCenterAnchor", "LeftCenterAnchor", "RightCenterAnchor"
            };

            foreach (var name in anchorNames)
            {
                var anchor = e.NameScope.Find<Control>(name);
                if (anchor != null)
                {
                    SubscribeAnchorEvents(anchor);
                }
            }
            
            _partCustomBorder = e.NameScope.Find<Rectangle>("PART_CustomBorder");
        }

        private void SubscribeAnchorEvents(Control anchor)
        {
            anchor.AddHandler(PointerPressedEvent, AnchorOnPointerPressed, RoutingStrategies.Tunnel);
            anchor.AddHandler(PointerMovedEvent, AnchorOnPointerMoved, RoutingStrategies.Tunnel);
            anchor.AddHandler(PointerReleasedEvent, AnchorOnPointerReleased, RoutingStrategies.Tunnel);
        }

        private void AnchorOnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _isResizing = true;
            _startPoint = e.GetCurrentPoint((Visual?)Parent);
            _currentAnchor = sender as Control;
            e.Pointer.Capture(_currentAnchor);

            _originalWidth = Width;
            _originalHeight = Height;
            _originalLeft = Canvas.GetLeft(this);
            _originalTop = Canvas.GetTop(this);
            
            _partCustomBorder?.Classes.Add("Resize");
        }

        private void AnchorOnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isResizing = false;
            _currentAnchor = null;

            // Обновление размеров и позиции AttachedControl
            UpdateAttachedControlBounds();
            
            e.Pointer.Capture(null);
            
            _partCustomBorder?.Classes.Remove("Resize");
        }

        private void AnchorOnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isResizing && _currentAnchor != null)
            {
                var currentPoint = e.GetCurrentPoint((Visual?)Parent);
                var deltaX = currentPoint.Position.X - _startPoint.Position.X;
                var deltaY = currentPoint.Position.Y - _startPoint.Position.Y;

                var (newWidth, newHeight, newLeft, newTop) = CalculateNewDimensions(deltaX, deltaY);

                // Обновление размеров и позиции элемента
                Width = newWidth;
                Height = newHeight;
                Canvas.SetLeft(this, newLeft);
                Canvas.SetTop(this, newTop);

                e.Handled = true;
            }
        }

        private (double newWidth, double newHeight, double newLeft, double newTop) CalculateNewDimensions(double deltaX, double deltaY)
        {
            double newWidth = _originalWidth;
            double newHeight = _originalHeight;
            double newLeft = _originalLeft;
            double newTop = _originalTop;

            if (_currentAnchor.Name.Contains("Left"))
            {
                newWidth = Math.Max(StepSizeByX, _originalWidth - deltaX);
                newLeft = _originalLeft + deltaX;
            }

            if (_currentAnchor.Name.Contains("Top"))
            {
                newHeight = Math.Max(StepSizeByY, _originalHeight - deltaY);
                newTop = _originalTop + deltaY;
            }

            if (_currentAnchor.Name.Contains("Bottom"))
            {
                newHeight = Math.Max(StepSizeByY, _originalHeight + deltaY);
            }

            if (_currentAnchor.Name.Contains("Right"))
            {
                newWidth = Math.Max(StepSizeByX, _originalWidth + deltaX);
            }

            // Привязка к сетке
            newWidth = SnapToGrid(newWidth, StepSizeByX);
            newHeight = SnapToGrid(newHeight, StepSizeByY);

            // Корректировка позиции после привязки к сетке
            if (_currentAnchor.Name.Contains("Left"))
            {
                newLeft = _originalLeft + (_originalWidth - newWidth);
                newLeft = SnapToGrid(newLeft, StepSizeByX);
            }

            if (_currentAnchor.Name.Contains("Top"))
            {
                newTop = _originalTop + (_originalHeight - newHeight);
                newTop = SnapToGrid(newTop, StepSizeByY);
            }

            return (newWidth, newHeight, newLeft, newTop);
        }

        public void UpdateAttachedControlBounds()
        {
            AttachedControl.Width = Width;
            AttachedControl.Height = Height;

            var relativePositionToParent = this.TranslatePoint(new Point(0, 0), (AttachedControl.Parent as Visual)!);

            if (relativePositionToParent.HasValue)
            {
                Canvas.SetLeft(AttachedControl, relativePositionToParent.Value.X);
                Canvas.SetTop(AttachedControl, relativePositionToParent.Value.Y);
            }
        }

        private double SnapToGrid(double value, double gridSize)
        {
            return Math.Round(value / gridSize) * gridSize;
        }
    }
}
