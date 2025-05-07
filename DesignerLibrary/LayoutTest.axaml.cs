using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
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
            //UiDesignerControl.EditingLayer.AttachItem(TestPanel);
            
        }, DispatcherPriority.Loaded);
        
        // UiDesignerControl.Loaded += (_, _) =>
        // {
        //     UiDesignerControl.EditingLayer.AttachItem(TestPanel);
        // };
        
        
        TestPanel.LayoutUpdated += UiDesignerControlOnLayoutUpdated;
        
        

    }

    private int i = 0;
    private void UiDesignerControlOnLayoutUpdated(object? sender, EventArgs e)
    {
        Console.WriteLine($"TestPanelUpdated: {i++}");
    }

    private void Button_OnClick_AutoWidth(object? sender, RoutedEventArgs e)
    {
        TestPanel.Width = double.NaN;
        TestPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
    }

    private void Button_OnClick_AutoHeight_Auto(object? sender, RoutedEventArgs e)
    {
        TestPanel.Height = double.NaN;
        TestPanel.VerticalAlignment = VerticalAlignment.Stretch;
    }
}