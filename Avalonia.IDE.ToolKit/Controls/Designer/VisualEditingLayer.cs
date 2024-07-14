using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
            
        }

        public void AddItem(Control attachedControl)
        {
            var veLayerItem = new VisualEditingLayerItem()
            {
                BorderBrush = Brushes.DarkSlateGray,
                Background = Brushes.Transparent,
                BorderThickness = 1,
                IsSelected=true,
                StepSizeByX =8,
                StepSizeByY=8,
                AttachedControl = attachedControl
            };
            
            _canvas.Children.Add(veLayerItem);
            veLayerItem.Tapped += veLayerItem_Tapped;
        }
        
        private void veLayerItem_Tapped(object sender, RoutedEventArgs e)
        {
            if (sender is VisualEditingLayerItem control)
            {
                Panel removeControl = control.AttachedControl.Parent as Panel;
                removeControl.Children.Remove(control.AttachedControl);
                
                _canvas.Children.Remove(control);
            }
        }
    }
}
