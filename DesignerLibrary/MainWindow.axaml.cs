using System;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using Avalonia.IDE.ToolKit.Controls;
using Avalonia.IDE.ToolKit.Controls.Designer;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;

namespace DesignerLibrary;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Func<Control>> _controlFactory;

    public MainWindow()
    {
        InitializeComponent();

        ScaleBox.SelectionChanged += (sender, args) =>
        {
            if (ScaleBox.SelectedIndex == 0)
            {
                DesignPanel.ScaleFactor = 1;
            }
            
            if (ScaleBox.SelectedIndex == 1)
            {
                DesignPanel.ScaleFactor = 1.25;
            }
            
            if (ScaleBox.SelectedIndex == 2)
            {
                DesignPanel.ScaleFactor = 1.5;
            }
            
            if (ScaleBox.SelectedIndex == 3)
            {
                DesignPanel.ScaleFactor = 1.75;
            }
            
            if (ScaleBox.SelectedIndex == 4)
            {
                DesignPanel.ScaleFactor = 2;
            }
        };
        
        _controlFactory = new Dictionary<string, Func<Control>>
        {
            { "Button", () => new Button { Content = "New Button" } },
            { "TextBox", () => new TextBox { Text = "New TextBox" } },
            { "Label", () => new TextBlock { Text = "New Label" } },
            { "StackPanel", () => new StackPanel { Background = Brushes.LightGray, Width = 100, Height = 100 } },
            { "DockPanel", () => new DockPanel { Background = Brushes.LightGray, Width = 100, Height = 100 } },
            { "Canvas", () => new Canvas { Background = Brushes.LightGray, Width = 100, Height = 100 } },
            { "Grid", () => new Grid { Background = Brushes.LightGray, Width = 100, Height = 100 } },
            { "ComboBox", () => new ComboBox { ItemsSource = new[] { "Item1", "Item2" } } },
            { "CheckBox", () => new CheckBox { Content = "New CheckBox" } },
            { "RadioButton", () => new RadioButton { Content = "New RadioButton" } },
            { "Slider", () => new Slider() },
            { "ProgressBar", () => new ProgressBar() },
            { "Image", () => new Image { Source = new Bitmap(AssetLoader.Open(new Uri("avares://DesignerLibrary/Assets/Logo.png"))), Width = 100, Height = 100 } },
            { "Calendar", () => new Calendar { Width = 100, Height = 100 } }
        };
        
    }

    private void ControlsListView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is TextBlock selectedItem)
        {
            if (_controlFactory.TryGetValue(selectedItem.Tag.ToString(), out var controlFactory))
            {
                var newControl = controlFactory();
                newControl.HorizontalAlignment = HorizontalAlignment.Center;
                newControl.VerticalAlignment = VerticalAlignment.Center;
                
                DisignerLayer.Children.Add(newControl);
                
                Canvas.SetTop(newControl, 100);
                Canvas.SetLeft(newControl, 100);

               var veLayerItem = new VisualEditingLayerItem()
               {
                   BorderBrush = Brushes.DarkSlateGray,
                   Background = Brushes.Transparent,
                   BorderThickness = 1,
                   IsSelected=true,
                   StepSizeByX = 8,
                   StepSizeByY=8,
                   AttachedControl = newControl
               };
               
               AdornerLayer1.Children.Add(veLayerItem);
            }
            
            listBox.SelectedItem = null;
        }
    }

    private void DesignPanel_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var scalingContainer = (ScalingContainer)sender;
        Title = $"{scalingContainer.Name} - {scalingContainer.Offset} -- {scalingContainer.Viewport.ToString()}";
    }
}
