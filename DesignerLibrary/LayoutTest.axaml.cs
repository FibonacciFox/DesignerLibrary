using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace DesignerLibrary;

public partial class LayoutTest : Window
{
    public LayoutTest()
    {
        InitializeComponent();
        
        Dispatcher.UIThread.Post(() =>
        {
            UiDesignerControl.EditingLayer.AttachItem(DisignerLayer);
           // UiDesignerControl.EditingLayer.AttachItem(Button1);
            
        }, DispatcherPriority.Loaded);
        
        // UiDesignerControl.Loaded += (_, _) =>
        // {
        //     UiDesignerControl.EditingLayer.AttachItem(TestPanel);
        // };
        

    }

    private void Button_OnClick_AutoWidth(object? sender, RoutedEventArgs e)
    {
        TestPanel.Width = double.NaN;
    }

    private void Button_OnClick_AutoHeight_Auto(object? sender, RoutedEventArgs e)
    {
        TestPanel.Height = double.NaN;
    }
}