using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace DesignerLibrary;

public partial class UiDesignerTest : Window
{
    public UiDesignerTest()
    {
        InitializeComponent();
        
        Dispatcher.UIThread.Post(() =>
        {
            UiDesignerControl.EditingLayer.AttachItem(TestPanel);
            UiDesignerControl.EditingLayer.AttachItem(Button1);
            
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
        //     UiDesignerControl.EditingLayer.AttachItem(TestPanel);
        // };
        

    }
}