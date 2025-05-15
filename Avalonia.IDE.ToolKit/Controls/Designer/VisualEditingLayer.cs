using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.IDE.ToolKit.Controls.Primitives;
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
            // Используем туннельную маршрутизацию для обработки клика до того, как он попадёт в дочерние элементы
            AddHandler(PointerPressedEvent, OnCanvasPointerPressed, RoutingStrategies.Tunnel);
        }
    }

    /// <summary>
    /// Обрабатывает клик по пустому месту — снимает выделение со всех элементов.
    /// </summary>
    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Игнорируем клики по VisualEditingItem или его потомкам
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
            {
                item.IsSelected = false;
            }
        }
    }

    /// <summary>
    /// Добавляет контрол в слой и оборачивает его в <see cref="VisualEditingItem"/>.
    /// Устанавливает Layout.X/Y и размеры по умолчанию, если они отсутствуют.
    /// </summary>
    public void AttachItem(Control attachedControl)
    {
        if (_canvas == null)
            return;

        var layerItem = new VisualEditingItem
        {
            BorderBrush = Brushes.DarkSlateGray,
            Background = Brushes.Transparent,
            BorderThickness = 1,
            StepSizeByX = 8,
            StepSizeByY = 8,
            AttachedControl = attachedControl,
            Focusable = true
        };

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
        if (sender is VisualEditingItem item && e.Key == Key.Delete && item.IsSelected)
        {
            if (item.AttachedControl?.Parent is MeshPanel parent)
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

}
