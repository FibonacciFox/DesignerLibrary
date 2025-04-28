using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace DesignerLibrary;

public partial class Test : Window
{
    public Test()
    {
        InitializeComponent();
        
        Dispatcher.UIThread.Post(() =>
        {
            UiDesignerControl.EditingLayer.AddItem(TestPanel);
        }, DispatcherPriority.Loaded);
        
        // UiDesignerControl.Loaded += (_, _) =>
        // {
        //     UiDesignerControl.EditingLayer.AddItem(TestPanel);
        // };
    }
}