using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace DesignerLibrary;

public partial class VisualEditor : UserControl
{
    public VisualEditor()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, OnKeyUp, RoutingStrategies.Tunnel);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            foreach (var child in CanvasPanel.Children)
            {
                if (child is VisualEditorDecoratorItem decorator)
                {
                    decorator.ZIndex = 0;
                }
            }
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            foreach (var child in CanvasPanel.Children)
            {
                if (child is VisualEditorDecoratorItem decorator && decorator.IsSelected)
                {
                    decorator.ZIndex = 1;
                }
            }
        }
    }
}