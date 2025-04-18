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

            // Инициализация значений из текущего положения
            XInputBox.Text = (Layout.GetX(DragPanel) ?? 0).ToString("F0");
            YInputBox.Text = (Layout.GetY(DragPanel) ?? 0).ToString("F0");

            HAlignCombo.SelectedIndex = (int)DragPanel.HorizontalAlignment;
            VAlignCombo.SelectedIndex = (int)DragPanel.VerticalAlignment;

            // Обновление UI при изменении координат
            
            DragPanel.PropertyChanged += (_, args) =>
            {
                if (args.Property == Layout.XProperty || args.Property == Layout.YProperty)
                {
                    XInputBox.Text = (Layout.GetX(DragPanel) ?? 0).ToString("F0");
                    YInputBox.Text = (Layout.GetY(DragPanel) ?? 0).ToString("F0");
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

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            Layout.SetX(DragPanel, 50);
            Layout.SetY(DragPanel, 30);

            Console.WriteLine($"Reset to: {Layout.GetX(DragPanel)}, {Layout.GetY(DragPanel)}");
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

                double? x = Layout.GetX(_dragTarget);
                double? y = Layout.GetY(_dragTarget);

                if (!double.IsNaN(x.GetValueOrDefault()) && !double.IsNaN(y.GetValueOrDefault()))
                {
                    Layout.SetX(_dragTarget, x.GetValueOrDefault() + delta.X);
                    Layout.SetY(_dragTarget, y.GetValueOrDefault() + delta.Y);
                    _dragStart = current;
                }
            }
        }

        private void OnDragEnd(object? sender, PointerReleasedEventArgs e)
        {
            if (_isDragging && _dragTarget != null)
            {
                _isDragging = false;
                e.Pointer.Capture(null);
                _dragTarget = null;

                Console.WriteLine($"Dropped at: {Layout.GetX(DragPanel)}, {Layout.GetY(DragPanel)}");
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
