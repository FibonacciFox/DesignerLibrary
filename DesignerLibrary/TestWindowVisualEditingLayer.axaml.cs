using System;
using Avalonia.Controls;
using Avalonia;
using Avalonia.IDE.ToolKit;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace DesignerLibrary
{
    public partial class TestWindowVisualEditingLayer : Window
    {

        public TestWindowVisualEditingLayer()
        {
            InitializeComponent();
            this.AttachDevTools();

        }


        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
           Layout.SetX(TextBox1, 300);
           Layout.SetY(TextBox1, 300);
           
           TextBox1.HorizontalAlignment = HorizontalAlignment.Center;
        }
    }
}
