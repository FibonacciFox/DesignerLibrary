using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.IDE.ToolKit.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace DesignerLibrary;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Func<Control>> _controlFactory;

    public MainWindow()
    {
        InitializeComponent();
        var scroll = new ScrollViewer();
        var t = scroll.Offset;
        /*Content = new ScalingContainer(){
            AllowAutoHide = false ,  
            HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            VerticalContentAlignment = VerticalAlignment.Top,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Width = 640,
            Height = 480,
            BorderThickness= new Thickness(3,3,3,3),
            ScaleFactor = 1,
            Content = new TextBlock(){Text = "DDDDDDDDDDDDD", Width = 60 , Height = 90}
        };*/
        
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
            { "Image", () => new Image { Source = new Bitmap("avares://DesignerPanel/Assets/image.png"), Width = 100, Height = 100 } },
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
                Canvas.SetLeft(newControl, 50);
                Canvas.SetTop(newControl, 50);

                DesignPanel.Content = newControl;
                //DesignPanel.CanvasPanel.Children.Add(new VisualEditorDecoratorItem(newControl, DesignPanel.CanvasPanel));
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
