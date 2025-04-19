using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.IDE.ToolKit;
using Avalonia.Layout;

namespace DesignerLibrary
{
    public partial class TestWindowVisualEditingLayer : Window
    {
        private bool _isDragging;
        private Point _dragStart;
        private Control? _dragTarget;

        public TestWindowVisualEditingLayer()
        {
            InitializeComponent();
            this.AttachDevTools();

            UpdateUIFromPanel();

            // Подписка на изменение Layout.X/Y и выравнивания
            DragPanel.PropertyChanged += (_, args) =>
            {
                if (args.Property == Layout.XProperty || args.Property == Layout.YProperty)
                {
                    UpdateXYInputs();
                }
                else if (args.Property == Layoutable.HorizontalAlignmentProperty)
                {
                    HAlignCombo.SelectedIndex = (int)DragPanel.HorizontalAlignment;
                }
                else if (args.Property == Layoutable.VerticalAlignmentProperty)
                {
                    VAlignCombo.SelectedIndex = (int)DragPanel.VerticalAlignment;
                }
            };
        }

        private void UpdateXYInputs()
        {
            XInputBox.Text = (Layout.GetX(DragPanel) ?? 0).ToString("F0");
            YInputBox.Text = (Layout.GetY(DragPanel) ?? 0).ToString("F0");
        }

        private void UpdateUIFromPanel()
        {
            UpdateXYInputs();
            HAlignCombo.SelectedIndex = (int)DragPanel.HorizontalAlignment;
            VAlignCombo.SelectedIndex = (int)DragPanel.VerticalAlignment;
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            DragPanel.HorizontalAlignment = HorizontalAlignment.Left;
            DragPanel.VerticalAlignment = VerticalAlignment.Top;

            Layout.SetX(DragPanel, 50);
            Layout.SetY(DragPanel, 30);

            Console.WriteLine($"[Reset] → X={Layout.GetX(DragPanel)}, Y={Layout.GetY(DragPanel)}");
        }

        private void OnDragStart(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Control control &&
                e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
            {
                _isDragging = true;
                _dragTarget = control;
                _dragStart = e.GetPosition(Panel1);

                e.Pointer.Capture(control);
            }
        }

        private void OnDragMove(object? sender, PointerEventArgs e)
        {
            if (_isDragging && _dragTarget != null)
            {
                var current = e.GetPosition(Panel1);
                var delta = current - _dragStart;

                double x = Layout.GetX(_dragTarget) ?? 0;
                double y = Layout.GetY(_dragTarget) ?? 0;

                Layout.SetX(_dragTarget, x + delta.X);
                Layout.SetY(_dragTarget, y + delta.Y);

                _dragStart = current;
            }
        }

        private void OnDragEnd(object? sender, PointerReleasedEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                e.Pointer.Capture(null);
                _dragTarget = null;

                Console.WriteLine($"[Drop] → X={Layout.GetX(DragPanel)}, Y={Layout.GetY(DragPanel)}");
            }
        }

        private void OnXBoxChanged(object? sender, RoutedEventArgs e)
        {
            if (double.TryParse(XInputBox.Text, out var x))
                Layout.SetX(DragPanel, x);
        }

        private void OnYBoxChanged(object? sender, RoutedEventArgs e)
        {
            if (double.TryParse(YInputBox.Text, out var y))
                Layout.SetY(DragPanel, y);
        }

        private void OnHAlignChanged(object? sender, SelectionChangedEventArgs e)
        {
            DragPanel.HorizontalAlignment = (HorizontalAlignment)HAlignCombo.SelectedIndex;
        }

        private void OnVAlignChanged(object? sender, SelectionChangedEventArgs e)
        {
            DragPanel.VerticalAlignment = (VerticalAlignment)VAlignCombo.SelectedIndex;
        }
    }
}
