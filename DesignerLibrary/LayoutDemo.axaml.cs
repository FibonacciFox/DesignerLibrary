using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;

namespace DesignerLibrary;

public partial class LayoutDemo : Window
{
    public LayoutDemo()
    {
        InitializeComponent();
        
        Dispatcher.UIThread.Post(() =>
        {
            UiDesignerControl.EditingLayer.AttachItem(DisignerLayer);
            UiDesignerControl.EditingLayer.AttachItem(TestPanel);
            UiDesignerControl.EditingLayer.AttachItem(TextBlock1);
            
        }, DispatcherPriority.Loaded);
        
        // UiDesignerControl.Loaded += (_, _) =>
        // {
        //     UiDesignerControl.EditingLayer.AttachItem(TestPanel);
        // };

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