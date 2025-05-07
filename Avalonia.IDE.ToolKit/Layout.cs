using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Threading;
using Avalonia.IDE.ToolKit.Controls.Designer;

namespace Avalonia.IDE.ToolKit;

/// <summary>
/// Предоставляет attached-свойства <c>Layout.X</c> и <c>Layout.Y</c>
/// для абсолютного позиционирования элементов внутри различных контейнеров.
/// В <see cref="Canvas"/> используются <c>Canvas.Left</c> и <c>Canvas.Top</c>,
/// в остальных случаях применяется <see cref="TranslateTransform"/> при выравнивании Left/Top.
/// Также предоставляет <c>DesignX</c> и <c>DesignY</c> — позицию относительно <see cref="UiDesigner"/>.
/// </summary>
public static class Layout
{
    /// <summary>
    /// Задаёт или получает координату X элемента.
    /// Используется при выравнивании по левому краю или при размещении внутри <see cref="Canvas"/>.
    /// </summary>
    public static readonly AttachedProperty<double> XProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "X", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Задаёт или получает координату Y элемента.
    /// Используется при выравнивании по верхнему краю или при размещении внутри <see cref="Canvas"/>.
    /// </summary>
    public static readonly AttachedProperty<double> YProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "Y", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Получает координату X относительно <see cref="UiDesigner"/>.
    /// Это read-only свойство, вычисляемое при каждом layout-проходе.
    /// </summary>
    public static readonly AttachedProperty<double> DesignXProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "DesignX", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.OneWay);

    /// <summary>
    /// Получает координату Y относительно <see cref="UiDesigner"/>.
    /// Это read-only свойство, вычисляемое при каждом layout-проходе.
    /// </summary>
    public static readonly AttachedProperty<double> DesignYProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "DesignY", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.OneWay);

    static Layout()
    {
        XProperty.Changed.Subscribe(e => OnPositionChanged(e.Sender as Control));
        YProperty.Changed.Subscribe(e => OnPositionChanged(e.Sender as Control));
    }

    /// <summary>
    /// Обрабатывает явное изменение свойства X или Y.
    /// </summary>
    /// <param name="control">Контрол, к которому привязано свойство.</param>
    private static void OnPositionChanged(Control? control)
    {
        if (control == null)
            return;

        control.LayoutUpdated -= OnLayoutUpdated;
        control.LayoutUpdated += OnLayoutUpdated;

        ApplyPosition(control);
        UpdateDesignPosition(control);
    }

    /// <summary>
    /// Обработчик события <see cref="Layoutable.LayoutUpdated"/>.
    /// Выполняет пересчёт <c>X/Y</c> при выравнивании ≠ Left/Top и обновляет <c>DesignX/Y</c>.
    /// </summary>
    /// <param name="sender">Контрол, обновлённый в layout.</param>
    /// <param name="e">Параметры события.</param>
    private static void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is not Control control)
            return;

        if (!IsInsideCanvas(control))
        {
            var parent = control.GetVisualParent();
            if (parent != null)
            {
                var pos = control.TranslatePoint(new Point(0, 0), parent);
                if (pos.HasValue)
                {
                    if (control.HorizontalAlignment != HorizontalAlignment.Left)
                        SetX(control, pos.Value.X);

                    if (control.VerticalAlignment != VerticalAlignment.Top)
                        SetY(control, pos.Value.Y);
                }
            }
        }

        ApplyPosition(control);
        UpdateDesignPosition(control);
    }

    /// <summary>
    /// Применяет значение X/Y к контролу через <see cref="TranslateTransform"/>
    /// или через <see cref="Canvas.Left"/> / <see cref="Canvas.Top"/>.
    /// </summary>
    /// <param name="control">Целевой контрол.</param>
    private static void ApplyPosition(Control control)
    {
        var x = GetX(control);
        var y = GetY(control);

        if (IsInsideCanvas(control))
        {
            if (!double.IsNaN(x)) Canvas.SetLeft(control, x);
            if (!double.IsNaN(y)) Canvas.SetTop(control, y);
            control.RenderTransform = null;
        }
        else
        {
            var transform = control.RenderTransform as TranslateTransform ?? new TranslateTransform();
            transform.X = control.HorizontalAlignment == HorizontalAlignment.Left ? x : 0;
            transform.Y = control.VerticalAlignment == VerticalAlignment.Top ? y : 0;
            control.RenderTransform = transform;
        }
    }

    /// <summary>
    /// Обновляет позицию относительно <see cref="UiDesigner"/> в свойства <c>DesignX</c> и <c>DesignY</c>.
    /// </summary>
    /// <param name="control">Целевой контрол.</param>
    private static void UpdateDesignPosition(Control control)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var designer = control.FindAncestorOfType<UiDesigner>();
            if (designer != null)
            {
                var position = control.TranslatePoint(new Point(0, 0), designer);
                if (position.HasValue)
                {
                    SetDesignX(control, position.Value.X);
                    SetDesignY(control, position.Value.Y);
                    return;
                }
            }

            SetDesignX(control, double.NaN);
            SetDesignY(control, double.NaN);
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Определяет, находится ли контрол внутри <see cref="Canvas"/>.
    /// </summary>
    /// <param name="control">Контрол для проверки.</param>
    /// <returns>True, если родитель — <see cref="Canvas"/>.</returns>
    private static bool IsInsideCanvas(Control control)
    {
        return control.GetVisualParent() is Canvas;
    }

    /// <summary> Получает значение свойства <c>Layout.X</c>. </summary>
    public static double GetX(AvaloniaObject obj) => obj.GetValue(XProperty);

    /// <summary> Устанавливает значение свойства <c>Layout.X</c>. </summary>
    public static void SetX(AvaloniaObject obj, double value) => obj.SetValue(XProperty, value);

    /// <summary> Получает значение свойства <c>Layout.Y</c>. </summary>
    public static double GetY(AvaloniaObject obj) => obj.GetValue(YProperty);

    /// <summary> Устанавливает значение свойства <c>Layout.Y</c>. </summary>
    public static void SetY(AvaloniaObject obj, double value) => obj.SetValue(YProperty, value);

    /// <summary> Получает значение свойства <c>Layout.DesignX</c>. </summary>
    public static double GetDesignX(AvaloniaObject obj) => obj.GetValue(DesignXProperty);

    /// <summary> Получает значение свойства <c>Layout.DesignY</c>. </summary>
    public static double GetDesignY(AvaloniaObject obj) => obj.GetValue(DesignYProperty);

    private static void SetDesignX(AvaloniaObject obj, double value) => obj.SetValue(DesignXProperty, value);
    private static void SetDesignY(AvaloniaObject obj, double value) => obj.SetValue(DesignYProperty, value);
}
