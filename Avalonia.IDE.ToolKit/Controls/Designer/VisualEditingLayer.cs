using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Avalonia.IDE.ToolKit.Controls.Designer;

/// <summary>
/// Слой визуального редактирования, содержащий редактируемые элементы <see cref="VisualEditingLayerItem"/>.
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
            // Используем туннельную маршрутизацию для обработки клика до того, как он попадёт в дочерние элементы
            AddHandler(PointerPressedEvent, OnCanvasPointerPressed, RoutingStrategies.Tunnel);
        }
    }

    /// <summary>
    /// Обрабатывает клик по пустому месту — снимает выделение со всех элементов.
    /// </summary>
    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Игнорируем клики по VisualEditingLayerItem или его потомкам
        if (e.Source is VisualEditingLayerItem ||
            (e.Source as Control)?.FindAncestorOfType<VisualEditingLayerItem>() != null)
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
            if (child is VisualEditingLayerItem item)
            {
                item.IsSelected = false;
            }
        }
    }

    /// <summary>
    /// Добавляет контрол в слой и оборачивает его в <see cref="VisualEditingLayerItem"/>.
    /// Устанавливает Layout.X/Y и размеры по умолчанию, если они отсутствуют.
    /// </summary>
    public void AddItem(Control attachedControl)
    {
        if (_canvas == null || attachedControl == null)
            return;

        // Получаем позицию из Layout.X/Y или устанавливаем по умолчанию
        var x = Layout.GetX(attachedControl) ?? 100;
        var y = Layout.GetY(attachedControl) ?? 100;

        x = SnapToGrid(x, 8); // Привязка к сетке
        y = SnapToGrid(y, 8);

        Layout.SetX(attachedControl, x);
        Layout.SetY(attachedControl, y);

        // Размеры по умолчанию
        if (double.IsNaN(attachedControl.Width) || attachedControl.Width == 0)
            attachedControl.Width = 100;

        if (double.IsNaN(attachedControl.Height) || attachedControl.Height == 0)
            attachedControl.Height = 40;

        var layerItem = new VisualEditingLayerItem
        {
            BorderBrush = Brushes.DarkSlateGray,
            Background = Brushes.Transparent,
            BorderThickness = 1,
            IsSelected = true,
            StepSizeByX = 8,
            StepSizeByY = 8,
            AttachedControl = attachedControl,
            Width = attachedControl.Width,
            Height = attachedControl.Height,
            Focusable = true
        };

        Layout.SetX(layerItem, x);
        Layout.SetY(layerItem, y);

        _canvas.Children.Add(layerItem);
        layerItem.Focus();

        layerItem.AddHandler(PointerPressedEvent, OnLayerItemPointerPressed, RoutingStrategies.Bubble);
        layerItem.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Bubble);
    }

    /// <summary>
    /// Обрабатывает нажатие клавиши — удаляет элемент по клавише Delete.
    /// </summary>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is VisualEditingLayerItem item && e.Key == Key.Delete && item.IsSelected)
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
        if (sender is VisualEditingLayerItem item)
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
