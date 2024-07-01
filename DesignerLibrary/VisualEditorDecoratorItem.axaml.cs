using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace DesignerLibrary
{
    public partial class VisualEditorDecoratorItem : UserControl
    {
        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<VisualEditorDecoratorItem, bool>(nameof(IsSelected), defaultValue: false);

        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        private Control _targetControl;
        private Panel _layer;
        private bool _isResizing;
        private bool _isDragging;
        private PointerPoint _startSizePoint;
        private PointerPoint _startDragPoint;

        public VisualEditorDecoratorItem()
        {
            InitializeComponent();
        }

        public VisualEditorDecoratorItem(Control targetControl, Panel layer)
        {
            InitializeComponent();
            _targetControl = targetControl;
            _layer = layer;

            AddPointerHandlers();
            _targetControl.LayoutUpdated += TargetControlOnLayoutUpdated;
            AddAnchorHandlers();
        }

        private void AddPointerHandlers()
        {
            AddHandler(PointerPressedEvent, OnPointerPressed);
            AddHandler(PointerReleasedEvent, OnPointerReleased);
            AddHandler(PointerMovedEvent, OnPointerMoved);
        }

        private void AddAnchorHandlers()
        {
            var anchors = new[]
            {
                TopLeftAnchor, LeftCenterAnchor, BottomLeftAnchor,
                TopRightAnchor, RightCenterAnchor, BottomRightAnchor,
                TopCenterAnchor, BottomCenterAnchor
            };

            foreach (var anchor in anchors)
            {
                anchor.AddHandler(PointerPressedEvent, AnchorOnPointerPressed, RoutingStrategies.Tunnel);
                anchor.AddHandler(PointerMovedEvent, AnchorOnPointerMoved, RoutingStrategies.Tunnel);
                anchor.AddHandler(PointerReleasedEvent, AnchorOnPointerReleased, RoutingStrategies.Tunnel);
            }
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            DeselectAllControls();

            ZIndex = 1;
            IsSelected = true;
            _startDragPoint = e.GetCurrentPoint((Visual?)_targetControl.Parent);
            _isDragging = true;
        }

        private void DeselectAllControls()
        {
            foreach (var child in _layer.Children)
            {
                if (child is VisualEditorDecoratorItem decorator)
                {
                    decorator.IsSelected = false;
                    decorator.ZIndex = 0;
                }
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isResizing = false;
            _isDragging = false;
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                MoveControl(e);
            }
        }

        private void MoveControl(PointerEventArgs e)
        {
            var point = e.GetCurrentPoint((Visual?)_targetControl.Parent);
            var deltaX = point.Position.X - _startDragPoint.Position.X;
            var deltaY = point.Position.Y - _startDragPoint.Position.Y;

            Canvas.SetLeft(_targetControl, Canvas.GetLeft(_targetControl) + deltaX);
            Canvas.SetTop(_targetControl, Canvas.GetTop(_targetControl) + deltaY);

            _startDragPoint = point;
            e.Handled = true;
        }

        private void AnchorOnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _isResizing = true;
            _startSizePoint = e.GetCurrentPoint((Visual?)_targetControl.Parent);
        }

        private void AnchorOnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isResizing = false;
        }

        private void AnchorOnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_isResizing) return;

            var point = e.GetCurrentPoint((Visual?)_targetControl.Parent);
            var deltaX = point.Position.X - _startSizePoint.Position.X;
            var deltaY = point.Position.Y - _startSizePoint.Position.Y;

            AdjustControlSizeAndPosition(sender as Control, deltaX, deltaY);
            _startSizePoint = point;
        }

        private void AdjustControlSizeAndPosition(Control? anchor, double deltaX, double deltaY)
        {
            double newWidth = _targetControl.Width;
            double newHeight = _targetControl.Height;

            if (anchor == TopLeftAnchor || anchor == LeftCenterAnchor || anchor == BottomLeftAnchor)
            {
                newWidth = Math.Max(0, (_targetControl.Width.Equals(double.NaN) ? _targetControl.DesiredSize.Width : _targetControl.Width) - deltaX);
                Canvas.SetLeft(_targetControl, Canvas.GetLeft(_targetControl) + deltaX);
            }

            if (anchor == TopLeftAnchor || anchor == TopCenterAnchor || anchor == TopRightAnchor)
            {
                newHeight = Math.Max(0, (_targetControl.Height.Equals(double.NaN) ? _targetControl.DesiredSize.Height : _targetControl.Height) - deltaY);
                Canvas.SetTop(_targetControl, Canvas.GetTop(_targetControl) + deltaY);
            }

            if (anchor == BottomLeftAnchor || anchor == BottomCenterAnchor || anchor == BottomRightAnchor)
            {
                newHeight = Math.Max(0, (_targetControl.Height.Equals(double.NaN) ? _targetControl.DesiredSize.Height : _targetControl.Height) + deltaY);
            }

            if (anchor == TopRightAnchor || anchor == RightCenterAnchor || anchor == BottomRightAnchor)
            {
                newWidth = Math.Max(0, (_targetControl.Width.Equals(double.NaN) ? _targetControl.DesiredSize.Width : _targetControl.Width) + deltaX);
            }

            _targetControl.Width = newWidth;
            _targetControl.Height = newHeight;
        }

        private void TargetControlOnLayoutUpdated(object? sender, EventArgs e)
        {
            Width = _targetControl.Bounds.Width;
            Height = _targetControl.Bounds.Height;

            var relativePositionToParent = _targetControl.TranslatePoint(new Point(0, 0), _layer);
            if (relativePositionToParent.HasValue)
            {
                Canvas.SetLeft(this, relativePositionToParent.Value.X);
                Canvas.SetTop(this, relativePositionToParent.Value.Y);
            }
        }
    }
}
