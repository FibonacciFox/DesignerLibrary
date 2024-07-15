using System;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using Avalonia.Controls.Primitives;
using Avalonia.IDE.ToolKit.Controls;
using Avalonia.IDE.ToolKit.Controls.Designer;
using Avalonia.IDE.ToolKit.Services;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;

namespace DesignerLibrary
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, Func<Control>> _controlFactory;

        public MainWindow()
        {
            InitializeComponent();

            ScaleBox.SelectionChanged += (sender, args) =>
            {
                switch (ScaleBox.SelectedIndex)
                {
                    case 0:
                        DesignPanel.ScaleFactor = 1;
                        break;
                    case 1:
                        DesignPanel.ScaleFactor = 1.25;
                        break;
                    case 2:
                        DesignPanel.ScaleFactor = 1.5;
                        break;
                    case 3:
                        DesignPanel.ScaleFactor = 1.75;
                        break;
                    case 4:
                        DesignPanel.ScaleFactor = 2;
                        break;
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
                { "ProgressBar", () => new ProgressBar() {Width = 100, Height = 40, Value = 20} },
                { "Image", () => new Image { Source = new Bitmap(AssetLoader.Open(new Uri("avares://DesignerLibrary/Assets/Logo.png"))), Width = 100, Height = 100 } },
                { "Calendar", () => new Calendar { } }
            };

            var logicalChildren = new LogicalChildrenMonitorService(DisignerLayer, new Type[] { typeof(AccessText) });

            logicalChildren.ChildAdded += control =>
            {
                VisualLayer.AddItem(control);
                AppendToConsole($"Added: {control.GetType().Name} in {control.Parent.GetType().Name}");
            };

            logicalChildren.ChildRemoved += control =>
            {
                AppendToConsole($"Removed: {control.GetType()}");
            };

            logicalChildren.StartMonitoring();
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

                    Canvas.SetTop(newControl, 100);
                    Canvas.SetLeft(newControl, 100);

                    DisignerLayer.Children.Add(newControl);
                }

                listBox.SelectedItem = null;
            }
        }

        private void DesignPanel_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (sender is ScalingContainer scalingContainer)
            {
                Title = $"{scalingContainer.Name} - {scalingContainer.Offset} -- {scalingContainer.Viewport}";
            }
        }

        private void AppendToConsole(string message)
        {
            ConsoleTextBox.Text += $"{message}{Environment.NewLine}";
            ConsoleTextBox.CaretIndex = ConsoleTextBox.Text.Length;
        }
    }
}
