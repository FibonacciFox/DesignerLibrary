using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Threading;

namespace Avalonia.IDE.ToolKit;

/// <summary>
/// Attached-свойства Layout.X и Layout.Y для управления абсолютным позиционированием.
/// Работает через TranslateTransform при выравнивании Left/Top, и сохраняет фактические координаты
/// при других выравниваниях, используя TranslatePoint.
/// </summary>
public static class Layout
{
    /// <summary>
    /// Смещение по оси X.
    /// </summary>
    public static readonly AttachedProperty<double?> XProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, double?>(
            "X",
            typeof(Layout),
            defaultValue: double.NaN,
            inherits: false,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Смещение по оси Y.
    /// </summary>
    public static readonly AttachedProperty<double?> YProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, double?>(
            "Y",
            typeof(Layout),
            defaultValue: double.NaN,
            inherits: false,
            defaultBindingMode: BindingMode.TwoWay);

    // Флаг, чтобы не подписываться на выравнивание дважды
    private static readonly AttachedProperty<bool> IsAlignmentSubscribedProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, bool>(
            "IsAlignmentSubscribed",
            typeof(Layout),
            defaultValue: false);

    static Layout()
    {
        XProperty.Changed.Subscribe(OnAnyPositionChanged);
        YProperty.Changed.Subscribe(OnAnyPositionChanged);
    }

    /// <summary>
    /// Срабатывает при любом изменении X или Y. Применяет трансформацию и следит за выравниванием.
    /// </summary>
    private static void OnAnyPositionChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is Control control)
        {
            EnsureFirstLayoutInitialized(control);
            ApplyPosition(control);

            if (!control.GetValue(IsAlignmentSubscribedProperty))
            {
                control.SetValue(IsAlignmentSubscribedProperty, true);

                control.GetPropertyChangedObservable(Layoutable.HorizontalAlignmentProperty)
                       .Subscribe(_ => OnAlignmentChanged(control));

                control.GetPropertyChangedObservable(Layoutable.VerticalAlignmentProperty)
                       .Subscribe(_ => OnAlignmentChanged(control));
            }
        }
    }

    /// <summary>
    /// Обрабатывает изменение выравнивания. Обновляет координаты и применяет трансформацию.
    /// </summary>
    private static void OnAlignmentChanged(Control control)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var offset = control.RenderTransform as TranslateTransform;

            if (control.HorizontalAlignment != HorizontalAlignment.Left ||
                control.VerticalAlignment != VerticalAlignment.Top)
            {
                var position = GetVisualPosition(control);
                if (position != null)
                {
                    SetX(control, position.Value.X + (offset?.X ?? 0));
                    SetY(control, position.Value.Y + (offset?.Y ?? 0));
                }
            }

            control.RenderTransform = null;
            ApplyPosition(control);

            Console.WriteLine($"[Layout] Alignment changed → X: {GetX(control)}, Y: {GetY(control)}");
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Применяет TranslateTransform при Left/Top выравнивании.
    /// </summary>
    private static void ApplyPosition(Control control)
    {
        var x = control.GetValue(XProperty).GetValueOrDefault();
        var y = control.GetValue(YProperty).GetValueOrDefault();

        var hasX = !double.IsNaN(x);
        var hasY = !double.IsNaN(y);

        var isAbsolute = control.HorizontalAlignment == HorizontalAlignment.Left &&
                         control.VerticalAlignment == VerticalAlignment.Top;

        if (!isAbsolute)
        {
            control.RenderTransform = null;
            return;
        }

        control.RenderTransform = new TranslateTransform(hasX ? x : 0, hasY ? y : 0);
    }

    /// <summary>
    /// Гарантирует, что позиция будет корректно захвачена после первого layout.
    /// </summary>
    private static void EnsureFirstLayoutInitialized(Control control)
    {
        if (control.GetVisualRoot() == null)
        {
            control.AttachedToVisualTree += FirstLayoutInit;
        }
        else
        {
            FirstLayoutInit(control, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Захватывает позицию контрола после первого layout-прохода.
    /// </summary>
    private static void FirstLayoutInit(object? sender, EventArgs e)
    {
        if (sender is not Control control)
            return;

        control.AttachedToVisualTree -= FirstLayoutInit;

        void OnLayoutReady(object? _, EventArgs __)
        {
            control.LayoutUpdated -= OnLayoutReady;

            if (control.HorizontalAlignment != HorizontalAlignment.Left ||
                control.VerticalAlignment != VerticalAlignment.Top)
            {
                var position = GetVisualPosition(control);
                if (position != null)
                {
                    SetX(control, position.Value.X);
                    SetY(control, position.Value.Y);
                }

                ApplyPosition(control);
            }

            Console.WriteLine($"[Layout] Initial layout → X: {GetX(control)}, Y: {GetY(control)}");
        }

        control.LayoutUpdated += OnLayoutReady;
    }

    /// <summary>
    /// Получает визуальную позицию контрола внутри его родителя.
    /// </summary>
    private static Point? GetVisualPosition(Control control)
    {
        var parent = control.GetVisualParent();
        return parent is Visual visualParent
            ? control.TranslatePoint(new Point(0, 0), visualParent)
            : null;
    }

    /// <summary> Получает значение Layout.X для объекта. </summary>
    public static double? GetX(AvaloniaObject obj) => obj.GetValue(XProperty);

    /// <summary> Устанавливает значение Layout.X для объекта. </summary>
    public static void SetX(AvaloniaObject obj, double? value) => obj.SetValue(XProperty, value);

    /// <summary> Получает значение Layout.Y для объекта. </summary>
    public static double? GetY(AvaloniaObject obj) => obj.GetValue(YProperty);

    /// <summary> Устанавливает значение Layout.Y для объекта. </summary>
    public static void SetY(AvaloniaObject obj, double? value) => obj.SetValue(YProperty, value);
}
