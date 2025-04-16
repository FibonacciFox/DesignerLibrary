using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.IDE.ToolKit;

/// <summary>
/// Предоставляет attached-свойства для управления абсолютным позиционированием контролов в Avalonia UI.
///
/// Вместо использования <see cref="Canvas"/>, данное решение позволяет применять смещения
/// по осям X и Y к любому <see cref="Control"/>, независимо от типа layout-контейнера,
/// при этом сохраняя возможности адаптивной верстки.
///
/// Для смещения используется <see cref="TranslateTransform"/>, и выравнивание устанавливается
/// в <see cref="HorizontalAlignment.Left"/> и <see cref="VerticalAlignment.Top"/>, чтобы трансформация
/// была привязана к левому верхнему углу.
///
/// Если значения <c>X</c> или <c>Y</c> не заданы (равны <c>null</c>), позиционирование не применяется.
/// </summary>
public static class Layout
{
    /// <summary>
    /// AttachedProperty для задания смещения по оси X.
    /// Значение <c>null</c> означает, что позиционирование не применяется.
    /// </summary>
    public static readonly AttachedProperty<double?> XProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, double?>(
            "X",
            typeof(Layout),
            defaultValue: null,
            inherits: false,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// AttachedProperty для задания смещения по оси Y.
    /// Значение <c>null</c> означает, что позиционирование не применяется.
    /// </summary>
    public static readonly AttachedProperty<double?> YProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, double?>(
            "Y",
            typeof(Layout),
            defaultValue: null,
            inherits: false,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Вспомогательное свойство, указывающее, подписан ли контрол на изменения <see cref="Control.Bounds"/>.
    /// Используется для предотвращения дублирующих подписок.
    /// </summary>
    private static readonly AttachedProperty<bool> IsBoundsSubscribedProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, bool>(
            "IsBoundsSubscribed",
            typeof(Layout),
            defaultValue: false);

    static Layout()
    {
        // Реакция на изменение X и Y
        XProperty.Changed.Subscribe(OnAnyPositionChanged);
        YProperty.Changed.Subscribe(OnAnyPositionChanged);

        // Реакция на изменение выравнивания
        Control.HorizontalAlignmentProperty.Changed.Subscribe(OnAlignmentChanged);
        Control.VerticalAlignmentProperty.Changed.Subscribe(OnAlignmentChanged);
    }

    /// <summary>
    /// Обрабатывает изменение свойства <c>X</c> или <c>Y</c>.
    /// </summary>
    private static void OnAnyPositionChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is Control control)
        {
            ApplyPosition(control);
        }
    }

    /// <summary>
    /// Обрабатывает изменение выравнивания.
    /// При выравнивании, отличном от Left/Top, позиционирование сбрасывается.
    /// </summary>
    private static void OnAlignmentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is Control control)
        {
            if (control.HorizontalAlignment != HorizontalAlignment.Left ||
                control.VerticalAlignment != VerticalAlignment.Top)
            {
                SetX(control, null);
                SetY(control, null);
                control.RenderTransform = null;
                return;
            }

            ApplyPosition(control);
        }
    }

    /// <summary>
    /// Применяет визуальное смещение к контролу на основе заданных <c>X</c> и <c>Y</c>.
    /// Также подписывается на <see cref="Control.BoundsProperty"/> для синхронизации координат.
    /// </summary>
    /// <param name="control">Целевой контрол.</param>
    private static void ApplyPosition(Control control)
    {
        var xNullable = control.GetValue(XProperty);
        var yNullable = control.GetValue(YProperty);

        if (xNullable is not double x || yNullable is not double y)
        {
            control.RenderTransform = null;
            return;
        }

        control.HorizontalAlignment = HorizontalAlignment.Left;
        control.VerticalAlignment = VerticalAlignment.Top;

        control.RenderTransform = new TranslateTransform(x, y);

        if (!control.GetValue(IsBoundsSubscribedProperty))
        {
            control.SetValue(IsBoundsSubscribedProperty, true);

            control.GetPropertyChangedObservable(Control.BoundsProperty).Subscribe(_ =>
            {
                var bounds = control.Bounds;
                var offset = control.RenderTransform as TranslateTransform;

                var actualX = bounds.X + (offset?.X ?? 0);
                var actualY = bounds.Y + (offset?.Y ?? 0);

                if (Math.Abs(control.GetValue(XProperty) ?? 0 - actualX) > 0.5 ||
                    Math.Abs(control.GetValue(YProperty) ?? 0 - actualY) > 0.5)
                {
                    SetX(control, actualX);
                    SetY(control, actualY);
                    Console.WriteLine($"[Layout] Bounds changed: Name={control.Name}, X={actualX}, Y={actualY}");
                }
            });
        }
    }

    /// <summary>
    /// Получает значение <see cref="XProperty"/>.
    /// </summary>
    public static double? GetX(AvaloniaObject obj) => obj.GetValue(XProperty);

    /// <summary>
    /// Устанавливает значение <see cref="XProperty"/>.
    /// </summary>
    public static void SetX(AvaloniaObject obj, double? value) => obj.SetValue(XProperty, value);

    /// <summary>
    /// Получает значение <see cref="YProperty"/>.
    /// </summary>
    public static double? GetY(AvaloniaObject obj) => obj.GetValue(YProperty);

    /// <summary>
    /// Устанавливает значение <see cref="YProperty"/>.
    /// </summary>
    public static void SetY(AvaloniaObject obj, double? value) => obj.SetValue(YProperty, value);
}
