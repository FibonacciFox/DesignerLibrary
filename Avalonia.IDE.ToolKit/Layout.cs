using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Threading;
using Avalonia.IDE.ToolKit.Controls.Designer;

namespace Avalonia.IDE.ToolKit;

/// <summary>
/// Предоставляет attached-свойства для абсолютного позиционирования элементов как внутри конструктора, так и в рантайме.
///
/// <para><b>X/Y</b> — координаты относительно непосредственного родительского контейнера.</para>
/// <para><b>DesignX/DesignY</b> — координаты относительно <see cref="UiDesigner"/> или корневого окна (<see cref="Window"/>), если UiDesigner отсутствует.</para>
///
/// Используется в визуальных редакторах, поддерживает как Canvas, так и любые другие контейнеры с выравниванием.
/// </summary>
public static class Layout
{
    /// <summary>
    /// Attached-свойство для абсолютной координаты X (по горизонтали) относительно родительского контейнера.
    /// 
    /// <para>В <see cref="Canvas"/> применяется через <see cref="Canvas.Left"/>.</para>
    /// <para>В других контейнерах — через <see cref="TranslateTransform"/>, но только если <see cref="HorizontalAlignment"/> установлен в <see cref="HorizontalAlignment.Left"/>.</para>
    /// </summary>
    public static readonly AttachedProperty<double> XProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "X", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Attached-свойство для абсолютной координаты Y (по вертикали) относительно родительского контейнера.
    /// 
    /// <para>В <see cref="Canvas"/> применяется через <see cref="Canvas.Top"/>.</para>
    /// <para>В других контейнерах — через <see cref="TranslateTransform"/>, но только если <see cref="VerticalAlignment"/> установлен в <see cref="VerticalAlignment.Top"/>.</para>
    /// </summary>
    public static readonly AttachedProperty<double> YProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "Y", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Attached-свойство для координаты X элемента относительно <see cref="UiDesigner"/> или <see cref="Window"/>, если UiDesigner отсутствует.
    /// 
    /// <para>Может быть задано вручную для позиционирования относительно конструктора.</para>
    /// <para>При установке пересчитываются <see cref="X"/> и <see cref="Y"/>.</para>
    /// </summary>
    public static readonly AttachedProperty<double> DesignXProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "DesignX", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Attached-свойство для координаты Y элемента относительно <see cref="UiDesigner"/> или <see cref="Window"/>, если UiDesigner отсутствует.
    /// 
    /// <para>Может быть задано вручную для позиционирования относительно конструктора.</para>
    /// <para>При установке пересчитываются <see cref="X"/> и <see cref="Y"/>.</para>
    /// </summary>
    public static readonly AttachedProperty<double> DesignYProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "DesignY", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.TwoWay);

    static Layout()
    {
        XProperty.Changed.Subscribe(e => OnPositionChanged(e.Sender as Control));
        YProperty.Changed.Subscribe(e => OnPositionChanged(e.Sender as Control));
        DesignXProperty.Changed.Subscribe(e => OnDesignPositionChanged(e.Sender as Control));
        DesignYProperty.Changed.Subscribe(e => OnDesignPositionChanged(e.Sender as Control));
    }

    /// <summary>
    /// Обрабатывает изменение X или Y: применяет позицию и обновляет координаты относительно конструктора.
    /// </summary>
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
    /// Обрабатывает изменение DesignX или DesignY: пересчитывает и задаёт X/Y относительно родителя.
    /// </summary>
    private static void OnDesignPositionChanged(Control? control)
    {
        if (control == null)
            return;

        // Если ещё не прикреплён к визуальному дереву — ждём события
        if (control.GetVisualRoot() is null)
        {
            void Handler(object? s, VisualTreeAttachmentEventArgs e)
            {
                control.AttachedToVisualTree -= Handler;
                // Переносим выполнение на UI-поток — визуальное дерево гарантировано готово
                Dispatcher.UIThread.Post(() => OnDesignPositionChanged(control));
            }

            control.AttachedToVisualTree += Handler;
            return;
        }

        Visual? reference = control.FindAncestorOfType<UiDesigner>() as Visual
                            ?? control.GetVisualRoot() as Visual;

        var parent = control.GetVisualParent();
        if (reference == null || parent == null)
            return;

        var dx = GetDesignX(control);
        var dy = GetDesignY(control);

        if (!double.IsNaN(dx) && !double.IsNaN(dy))
        {
            var local = new Point(dx, dy);
            var translated = reference.TranslatePoint(local, parent);

            if (translated.HasValue)
            {
                SetX(control, translated.Value.X);
                SetY(control, translated.Value.Y);
            }
        }
    }


    /// <summary>
    /// Обрабатывает layout-проход: обновляет X/Y и DesignX/DesignY на основе текущей позиции.
    /// </summary>
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
                    if (control.HorizontalAlignment == HorizontalAlignment.Left)
                        SetX(control, pos.Value.X);

                    if (control.VerticalAlignment == VerticalAlignment.Top)
                        SetY(control, pos.Value.Y);
                }
            }
        }

        ApplyPosition(control);
        UpdateDesignPosition(control);
    }

    /// <summary>
    /// Применяет координаты X и Y через Canvas или TranslateTransform, в зависимости от контейнера и выравнивания.
    /// </summary>
    private static void ApplyPosition(Control control)
    {
        var x = GetX(control);
        var y = GetY(control);

        if (IsInsideCanvas(control))
        {
            if (!double.IsNaN(x)) Canvas.SetLeft(control, x);
            if (!double.IsNaN(y)) Canvas.SetTop(control, y);

            if (control.RenderTransform is TranslateTransform)
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
    /// Обновляет координаты DesignX и DesignY, рассчитывая их относительно <see cref="UiDesigner"/> или <see cref="Window"/>.
    /// </summary>
    private static void UpdateDesignPosition(Control control)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Visual? reference = control.FindAncestorOfType<UiDesigner>() as Visual
                             ?? control.GetVisualRoot() as Visual;

            if (reference != null)
            {
                var position = control.TranslatePoint(new Point(0, 0), reference);
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
    /// Определяет, находится ли элемент в <see cref="Canvas"/>.
    /// </summary>
    private static bool IsInsideCanvas(Control control)
    {
        return control.GetVisualParent() is Canvas;
    }

    /// <summary> Получает значение свойства <see cref="XProperty"/>. </summary>
    public static double GetX(AvaloniaObject obj) => obj.GetValue(XProperty);

    /// <summary> Устанавливает значение свойства <see cref="XProperty"/>. </summary>
    public static void SetX(AvaloniaObject obj, double value) => obj.SetValue(XProperty, value);

    /// <summary> Получает значение свойства <see cref="YProperty"/>. </summary>
    public static double GetY(AvaloniaObject obj) => obj.GetValue(YProperty);

    /// <summary> Устанавливает значение свойства <see cref="YProperty"/>. </summary>
    public static void SetY(AvaloniaObject obj, double value) => obj.SetValue(YProperty, value);

    /// <summary> Получает значение свойства <see cref="DesignXProperty"/>. </summary>
    public static double GetDesignX(AvaloniaObject obj) => obj.GetValue(DesignXProperty);

    /// <summary> Устанавливает значение свойства <see cref="DesignXProperty"/>. </summary>
    public static void SetDesignX(AvaloniaObject obj, double value) => obj.SetValue(DesignXProperty, value);

    /// <summary> Получает значение свойства <see cref="DesignYProperty"/>. </summary>
    public static double GetDesignY(AvaloniaObject obj) => obj.GetValue(DesignYProperty);

    /// <summary> Устанавливает значение свойства <see cref="DesignYProperty"/>. </summary>
    public static void SetDesignY(AvaloniaObject obj, double value) => obj.SetValue(DesignYProperty, value);
}
