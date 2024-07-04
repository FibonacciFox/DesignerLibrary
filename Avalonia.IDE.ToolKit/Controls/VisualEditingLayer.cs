using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.IDE.ToolKit.Controls
{
    public class VisualEditingLayer : TemplatedControl
    {
        public static readonly StyledProperty<IEnumerable<Control>> ChildrenProperty =
            AvaloniaProperty.Register<VisualEditingLayer, IEnumerable<Control>>(nameof(Children));

        public IEnumerable<Control> Children
        {
            get => GetValue(ChildrenProperty);
            set => SetValue(ChildrenProperty, value);
        }

        private Canvas _canvas;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _canvas = e.NameScope.Find<Canvas>("PART_Canvas");

            if (_canvas != null && Children != null)
            {
                AddChildrenToCanvas(Children);
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ChildrenProperty)
            {
                if (_canvas != null)
                {
                    _canvas.Children.Clear();
                    if (change.NewValue is IEnumerable<Control> newChildren)
                    {
                        AddChildrenToCanvas(newChildren);
                    }
                }
            }
        }

        private void AddChildrenToCanvas(IEnumerable<Control> children)
        {
            foreach (var child in children)
            {
                var item = new VisualEditingLayerItem { ControlledControl = child };
                _canvas.Children.Add(item);
            }
        }
    }
}