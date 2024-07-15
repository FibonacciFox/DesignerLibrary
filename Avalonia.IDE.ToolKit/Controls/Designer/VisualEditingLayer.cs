using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Avalonia.IDE.ToolKit.Controls.Designer
{
    public class VisualEditingLayer : TemplatedControl
    {
        private Canvas? _canvas;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _canvas = e.NameScope.Find<Canvas>("PART_Canvas");
            
            // Добавляем обработчик события для очистки выделенных элементов
            if (_canvas != null)
            {
                AddHandler(PointerPressedEvent, OnCanvasPointerPressed, RoutingStrategies.Bubble);
            }
            
        }
        

        private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is VisualEditingLayer)
            {
                ClearSelectedItems();
                // Здесь можно использовать методы логирования вместо Console.WriteLine
                // Logger.LogInfo($"Canvas pressed: {sender}");
            }
        }

        public void ClearSelectedItems()
        {
            if (_canvas == null) return;

            foreach (var child in _canvas.Children)
            {
                if (child is VisualEditingLayerItem layerItem)
                {
                    layerItem.IsSelected = false;
                    layerItem.ZIndex = 0;
                }
            }
        }

        public void AddItem(Control attachedControl)
        {
            if (_canvas == null || attachedControl == null) return;

            var veLayerItem = new VisualEditingLayerItem
            {
                BorderBrush = Brushes.DarkSlateGray,
                Background = Brushes.Transparent,
                BorderThickness = 1,
                IsSelected = true,
                StepSizeByX = 8,
                StepSizeByY = 8,
                AttachedControl = attachedControl,
                Width = attachedControl.Bounds.Width,
                Height = attachedControl.Bounds.Height,
                Focusable = true
            };

            Canvas.SetTop(veLayerItem, attachedControl.Bounds.Top);
            Canvas.SetLeft(veLayerItem, attachedControl.Bounds.Left);

            _canvas.Children.Add(veLayerItem);
            veLayerItem.Focus();

            veLayerItem.AddHandler(PointerPressedEvent, OnLayerItemPointerPressed, RoutingStrategies.Bubble);
            veLayerItem.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Bubble);
        }
        
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            var layerItem = sender as VisualEditingLayerItem;
            if (e.Key == Key.Delete)
            {
                if (layerItem.IsSelected)
                {
                    Canvas attachedControlParent = layerItem.AttachedControl.Parent as Canvas;
                    attachedControlParent.Children.Remove(layerItem.AttachedControl);
                    _canvas.Children.Remove(layerItem);
                }
            }
        }

        private void OnLayerItemPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is VisualEditingLayerItem layerItem)
            {
                // Logger.LogInfo($"Layer item pressed: {sender}");
                ClearSelectedItems();
                layerItem.ZIndex = 1;
                layerItem.IsSelected = true;
            }

            e.Handled = true;
        }
    }
}
