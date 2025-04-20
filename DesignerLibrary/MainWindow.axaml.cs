using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.IDE.ToolKit;
using Avalonia.IDE.ToolKit.Controls;
using Avalonia.IDE.ToolKit.Services;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace DesignerLibrary
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, Func<Control>> _controlFactory;

        public MainWindow()
        {
            InitializeComponent();

            ScaleBox.SelectionChanged += (_, _) =>
            {
                DesignPanel.ScaleFactor = ScaleBox.SelectedIndex switch
                {
                    0 => 1,
                    1 => 1.25,
                    2 => 1.5,
                    3 => 1.75,
                    4 => 2,
                    _ => 1
                };
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
                { "ProgressBar", () => new ProgressBar { Width = 100, Height = 40, Value = 20 } },
                { "Image", () => new Image
                    {
                        Source = new Bitmap(AssetLoader.Open(new Uri("avares://DesignerLibrary/Assets/Logo.png"))),
                        Width = 100,
                        Height = 100
                    }
                },
                { "Calendar", () => new Calendar() }
            };

            var logicalChildren = new LogicalChildrenMonitorService(DisignerLayer, new[] { typeof(AccessText) });

            logicalChildren.ChildAdded += control =>
            {
                VisualLayer.AddItem(control);
                AppendToConsole($"Added: {control.GetType().Name} in {control.Parent?.GetType().Name}");
            };

            logicalChildren.ChildRemoved += control =>
            {
                AppendToConsole($"Removed: {control.GetType().Name}");
            };

            logicalChildren.StartMonitoring();
        }

        private void ControlsListView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox || listBox.SelectedItem is not TextBlock selectedItem)
                return;

            if (_controlFactory.TryGetValue(selectedItem.Tag?.ToString() ?? "", out var controlFactory))
            {
                var newControl = controlFactory();

                newControl.HorizontalAlignment = HorizontalAlignment.Left;
                newControl.VerticalAlignment = VerticalAlignment.Top;

                // Получаем размер сетки из UI
                double.TryParse(MeshSizeX.Text, out double gridX);
                double.TryParse(MeshSizeY.Text, out double gridY);

                gridX = gridX <= 0 ? 8 : gridX;
                gridY = gridY <= 0 ? 8 : gridY;

                double x = SnapToGrid(100, gridX);
                double y = SnapToGrid(100, gridY);

                Layout.SetX(newControl, x);
                Layout.SetY(newControl, y);

                if (double.IsNaN(newControl.Width) || newControl.Width == 0)
                    newControl.Width = 100;

                if (double.IsNaN(newControl.Height) || newControl.Height == 0)
                    newControl.Height = 40;

                DisignerLayer.Children.Add(newControl);
            }

            listBox.SelectedItem = null;
        }

        private void DesignPanel_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (sender is ScalingContainer scalingContainer)
            {
                Title = $"{scalingContainer.Name} - Offset: {scalingContainer.Offset} Viewport: {scalingContainer.Viewport}";
            }
        }

        private void AppendToConsole(string message)
        {
            ConsoleTextBox.Text += $"{message}{Environment.NewLine}";
            ConsoleTextBox.CaretIndex = ConsoleTextBox.Text.Length;
        }

        private double SnapToGrid(double value, double gridSize) =>
            Math.Round(value / gridSize) * gridSize;
    }
}
