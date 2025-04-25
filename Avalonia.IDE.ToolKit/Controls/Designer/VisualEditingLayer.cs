using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Avalonia.IDE.ToolKit.Controls.Designer;

/// <summary>
/// Слой визуального редактирования, содержащий редактируемые элементы <see cref="VisualEditingItem"/>.
/// Позволяет размещать, перемещать и удалять элементы в режиме конструктора.
/// </summary>
public class VisualEditingLayer : TemplatedControl
{
    private Canvas? _canvas;

    /// <summary>
    /// Применяет шаблон и инициализирует внутренний Canvas.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _canvas = e.NameScope.Find<Canvas>("PART_Canvas");

        if (_canvas != null)
        {
            AddHandler(PointerPressedEvent, OnCanvasPointerPressed, RoutingStrategies.Tunnel);
        }
    }

    /// <summary>
    /// Обрабатывает клик по пустому месту — снимает выделение со всех элементов.
    /// </summary>
    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is VisualEditingItem ||
            (e.Source as Control)?.FindAncestorOfType<VisualEditingItem>() != null)
            return;

        ClearSelectedItems();
    }

    /// <summary>
    /// Снимает выделение со всех элементов.
    /// </summary>
    public void ClearSelectedItems()
    {
        if (_canvas == null) return;

        foreach (var child in _canvas.Children)
        {
            if (child is VisualEditingItem item)
                item.IsSelected = false;
        }
    }

    /// <summary>
    /// Добавляет контрол в слой и оборачивает его в <see cref="VisualEditingItem"/>.
    /// Автоматически позиционирует элемент, даже если он вложен в панель.
    /// </summary>
    public void AddItem(Control attachedControl)
    {
        if (_canvas == null || attachedControl == null)
            return;

        // Получаем визуальные координаты контрола относительно слоя
        var position = attachedControl.TranslatePoint(new Point(0, 0), this) ?? new Point(100, 100);

        var x = SnapToGrid(position.X, 8);
        var y = SnapToGrid(position.Y, 8);

        Layout.SetX(attachedControl, x);
        Layout.SetY(attachedControl, y);

        if (double.IsNaN(attachedControl.Width) || attachedControl.Width == 0)
            attachedControl.Width = 100;

        if (double.IsNaN(attachedControl.Height) || attachedControl.Height == 0)
            attachedControl.Height = 40;

        var item = new VisualEditingItem
        {
            BorderBrush = Brushes.DarkSlateGray,
            Background = Brushes.Transparent,
            BorderThickness = 1,
            IsSelected = true,
            StepSizeByX = 8,
            StepSizeByY = 8,
            AttachedControl = attachedControl,
            Focusable = true,
            ContextMenu = new ContextMenu
            {
                ItemsSource = new[]
                {
                    new MenuItem { Header = "Копировать" },
                    new MenuItem { Header = "Удалить" },
                    new MenuItem { Header = "Закрепить" },
                    new MenuItem { Header = "На задний план" }
                }
            }
        };

        Layout.SetX(item, x);
        Layout.SetY(item, y);

        _canvas.Children.Add(item);
        item.Focus();

        item.AddHandler(PointerPressedEvent, OnLayerItemPointerPressed, RoutingStrategies.Bubble);
        item.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Bubble);
    }

    /// <summary>
    /// Обрабатывает удаление элемента по клавише Delete.
    /// </summary>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is VisualEditingItem item && e.Key == Key.Delete && item.IsSelected)
        {
            if (item.AttachedControl?.Parent is Panel parent)
                parent.Children.Remove(item.AttachedControl);

            _canvas?.Children.Remove(item);
        }
    }

    /// <summary>
    /// Обрабатывает выделение одного элемента — снимает выделение с остальных.
    /// </summary>
    private void OnLayerItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is VisualEditingItem item)
        {
            ClearSelectedItems();
            item.IsSelected = true;
        }

        e.Handled = true;
    }

    private double SnapToGrid(double value, double gridSize)
    {
        return Math.Round(value / gridSize) * gridSize;
    }
}
