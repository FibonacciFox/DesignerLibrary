using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;

namespace DesignerLibrary;

/// <summary>
/// Вспомогательный класс для работы с перечислениями в XAML.
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// Возвращает все значения перечисления <see cref="HorizontalAlignment"/>.
    /// </summary>
    public static HorizontalAlignment[]? HorizontalAlignments => Enum.GetValues(typeof(HorizontalAlignment)) as HorizontalAlignment[];
    
    /// <summary>
    /// Возвращает все значения перечисления <see cref="HorizontalAlignment"/>.
    /// </summary>
    public static VerticalAlignment[]? VerticalAlignments => Enum.GetValues(typeof(VerticalAlignment)) as VerticalAlignment[];
}

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