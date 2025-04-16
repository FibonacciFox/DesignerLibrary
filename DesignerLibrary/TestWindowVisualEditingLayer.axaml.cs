using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.IDE.ToolKit;

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
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            Layout.SetX(DragPanel, 50);
            Layout.SetY(DragPanel, 30);
            Console.WriteLine($"New position: {Layout.GetX(DragPanel)}, {Layout.GetY(DragPanel)}");
        }

        private void OnDragStart(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Control control &&
                e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
            {
                _isDragging = true;
                _dragTarget = control;
                _dragStart = e.GetPosition(Panel1);

                e.Pointer.Capture(control); // ✅ фикс: захват указателя
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

                if (x != null && y != null)
                {
                    Layout.SetX(_dragTarget, x + delta.X);
                    Layout.SetY(_dragTarget, y + delta.Y);
                    _dragStart = current;
                }
            }
        }

        private void OnDragEnd(object? sender, PointerReleasedEventArgs e)
        {
            if (_isDragging && _dragTarget != null)
            {
                _isDragging = false;

                e.Pointer.Capture(null); // ✅ фикс: освобождение указателя
                _dragTarget = null;
                
                Console.WriteLine($"New position: {Layout.GetX(DragPanel)}, {Layout.GetY(DragPanel)}");
            }
        }
    }
}
