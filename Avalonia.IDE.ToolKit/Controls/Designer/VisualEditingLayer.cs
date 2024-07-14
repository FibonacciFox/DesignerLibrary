using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace Avalonia.IDE.ToolKit.Controls.Designer
{
    public class VisualEditingLayer : TemplatedControl
    {
        public static readonly StyledProperty<object> TemplateRootLayerSourceProperty =
            AvaloniaProperty.Register<VisualEditingLayer, object>(nameof(TemplateRootLayerContent));

        private Canvas? _canvas;
        private Timer _monitorTimer;
        private ObservableCollection<Control> Children { get; } = new();

        public object TemplateRootLayerContent
        {
            get => GetValue(TemplateRootLayerSourceProperty);
            set { SetValue(TemplateRootLayerSourceProperty, value); }
        }

        static VisualEditingLayer()
        {
            TemplateRootLayerSourceProperty.Changed.AddClassHandler<VisualEditingLayer>((x, e) => x.OnTemplateRootLayerSourceChanged(e));
        }

        private void OnTemplateRootLayerSourceChanged(AvaloniaPropertyChangedEventArgs avaloniaPropertyChangedEventArgs)
        {
            //_monitorTimer = new Timer(MonitorTreeChanges!, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.5));
        }


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
                StepSizeByX = 8,
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
