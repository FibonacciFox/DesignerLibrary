using System;
using Avalonia;
using Avalonia.Controls;
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
            UiDesignerControl.EditingLayer.AddItem(Button1);
            
            var root = UiDesignerControl;
            var positionA = Button1.TranslatePoint(new Point(0, 0), root);
            var positionB = UiDesignerControl.EditingLayer.TranslatePoint(new Point(0, 0), root);
            if (positionA != null && positionB != null)
            {
                var relativePosition = positionA.Value - positionB.Value;
                Console.WriteLine(relativePosition);
            }
            
        }, DispatcherPriority.Loaded);
        
        // UiDesignerControl.Loaded += (_, _) =>
        // {
        //     UiDesignerControl.EditingLayer.AddItem(TestPanel);
        // };
        

    }
}