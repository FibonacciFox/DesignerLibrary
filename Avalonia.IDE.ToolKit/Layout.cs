using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using System;

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
/// Если значения <c>X</c> или <c>Y</c> не заданы (равны <see cref="double.NaN"/>), позиционирование не применяется.
/// </summary>
public static class Layout
{
    public static readonly AttachedProperty<double?> XProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, double?>(
            "X",
            typeof(Layout),
            defaultValue: double.NaN,
            inherits: false,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly AttachedProperty<double?> YProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, double?>(
            "Y",
            typeof(Layout),
            defaultValue: double.NaN,
            inherits: false,
            defaultBindingMode: BindingMode.TwoWay);

    private static readonly AttachedProperty<bool> IsBoundsSubscribedProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, bool>(
            "IsBoundsSubscribed",
            typeof(Layout),
            defaultValue: false);

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

    private static void OnAnyPositionChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is Control control)
        {
            ApplyPosition(control);

            // Подписка на выравнивание при первом использовании
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

    private static void OnAlignmentChanged(Control control)
    {
        var x = GetX(control);
        var y = GetY(control);

        if (x.HasValue && !double.IsNaN(x.Value) && control.HorizontalAlignment != HorizontalAlignment.Left)
        {
            SetX(control, double.NaN);
        }

        if (y.HasValue && !double.IsNaN(y.Value) && control.VerticalAlignment != VerticalAlignment.Top)
        {
            SetY(control, double.NaN);
        }

        ApplyPosition(control);
    }

    private static void ApplyPosition(Control control)
    {
        var x = control.GetValue(XProperty).GetValueOrDefault();
        var y = control.GetValue(YProperty).GetValueOrDefault();

        var hasX = !double.IsNaN(x);
        var hasY = !double.IsNaN(y);

        if (!hasX && !hasY)
        {
            control.RenderTransform = null;
            return;
        }

        control.HorizontalAlignment = HorizontalAlignment.Left;
        control.VerticalAlignment = VerticalAlignment.Top;

        control.RenderTransform = new TranslateTransform(hasX ? x : 0, hasY ? y : 0);

        if (!control.GetValue(IsBoundsSubscribedProperty))
        {
            control.SetValue(IsBoundsSubscribedProperty, true);

            control.GetPropertyChangedObservable(Control.BoundsProperty).Subscribe(_ =>
            {
                var bounds = control.Bounds;
                var offset = control.RenderTransform as TranslateTransform;

                var actualX = bounds.X + (offset?.X ?? 0);
                var actualY = bounds.Y + (offset?.Y ?? 0);

                var currentX = control.GetValue(XProperty).GetValueOrDefault();
                var currentY = control.GetValue(YProperty).GetValueOrDefault();

                if (Math.Abs(currentX - actualX) > 0.5)
                    SetX(control, actualX);

                if (Math.Abs(currentY - actualY) > 0.5)
                    SetY(control, actualY);

                Console.WriteLine($"[Layout] Bounds changed: Name={control.Name}, X={actualX}, Y={actualY}");
            });
        }
    }

    public static double? GetX(AvaloniaObject obj) => obj.GetValue(XProperty);
    public static void SetX(AvaloniaObject obj, double? value) => obj.SetValue(XProperty, value);

    public static double? GetY(AvaloniaObject obj) => obj.GetValue(YProperty);
    public static void SetY(AvaloniaObject obj, double? value) => obj.SetValue(YProperty, value);
}
