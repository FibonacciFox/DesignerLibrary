using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Avalonia.IDE.ToolKit.Controls.Designer
{
    public class VisualEditingLayer : TemplatedControl
    {
        public static readonly StyledProperty<Control> TemplateRootLayerContentProperty =
            AvaloniaProperty.Register<VisualEditingLayer, Control>(nameof(TemplateRootLayerContent));

        private Canvas? _canvas;

        public Control TemplateRootLayerContent
        {
            get => GetValue(TemplateRootLayerContentProperty);
            set => SetValue(TemplateRootLayerContentProperty, value);
        }
        
        
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _canvas = e.NameScope.Find<Canvas>("PART_Canvas");

            if (TemplateRootLayerContent is Button)
            {
                
                    var veLayerItem = new VisualEditingLayerItem()
                    {
                        BorderBrush = Brushes.DarkSlateGray,
                        Background = Brushes.Transparent,
                        BorderThickness = 1,
                        IsSelected=true,
                        StepSizeByX = 8,
                        StepSizeByY=8,
                        AttachedControl = TemplateRootLayerContent
                    };
                    
                    _canvas.Children.Add(veLayerItem);

            }

            
        }
        
    }
}
