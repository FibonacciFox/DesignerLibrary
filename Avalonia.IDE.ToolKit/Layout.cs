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
/// Предоставляет attached-свойства Layout.X и Layout.Y для позиционирования элементов с независимой логикой по осям.
/// X применяется, если HorizontalAlignment = Left; Y — если VerticalAlignment = Top.
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

    private static readonly AttachedProperty<bool> IsAlignmentSubscribedProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, bool>(
            "IsAlignmentSubscribed",
            typeof(Layout),
            defaultValue: false);

    static Layout()
    {
        XProperty.Changed.Subscribe(e => OnPositionChanged(e, isX: true));
        YProperty.Changed.Subscribe(e => OnPositionChanged(e, isX: false));
    }

    private static void OnPositionChanged(AvaloniaPropertyChangedEventArgs e, bool isX)
    {
        if (e.Sender is Control control)
        {
            EnsureFirstLayoutInitialized(control);
            ApplyAxis(control, isX);

            if (!control.GetValue(IsAlignmentSubscribedProperty))
            {
                control.SetValue(IsAlignmentSubscribedProperty, true);

                control.GetPropertyChangedObservable(Layoutable.HorizontalAlignmentProperty)
                    .Subscribe(_ => OnAlignmentChanged(control, isX: true));

                control.GetPropertyChangedObservable(Layoutable.VerticalAlignmentProperty)
                    .Subscribe(_ => OnAlignmentChanged(control, isX: false));
            }
        }
    }

    private static void OnAlignmentChanged(Control control, bool isX)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var offset = control.RenderTransform as TranslateTransform ?? new TranslateTransform();

            if (isX && control.HorizontalAlignment != HorizontalAlignment.Left)
            {
                var pos = GetVisualPosition(control);
                if (pos != null)
                    SetX(control, pos.Value.X + offset.X);
            }

            if (!isX && control.VerticalAlignment != VerticalAlignment.Top)
            {
                var pos = GetVisualPosition(control);
                if (pos != null)
                    SetY(control, pos.Value.Y + offset.Y);
            }

            // Сброс и повторное применение только нужной оси
            var current = control.RenderTransform as TranslateTransform ?? new TranslateTransform();
            var newTransform = new TranslateTransform
            {
                X = control.HorizontalAlignment == HorizontalAlignment.Left ? GetX(control) ?? 0 : 0,
                Y = control.VerticalAlignment == VerticalAlignment.Top ? GetY(control) ?? 0 : 0
            };

            control.RenderTransform = newTransform;
        }, DispatcherPriority.Loaded);
    }

    private static void ApplyAxis(Control control, bool isX)
    {
        var transform = control.RenderTransform as TranslateTransform ?? new TranslateTransform();

        if (isX)
        {
            var x = GetX(control) ?? 0;
            transform.X = control.HorizontalAlignment == HorizontalAlignment.Left ? x : 0;
        }
        else
        {
            var y = GetY(control) ?? 0;
            transform.Y = control.VerticalAlignment == VerticalAlignment.Top ? y : 0;
        }

        control.RenderTransform = transform;
    }

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

    private static void FirstLayoutInit(object? sender, EventArgs e)
    {
        if (sender is not Control control)
            return;

        control.AttachedToVisualTree -= FirstLayoutInit;

        void OnLayoutReady(object? _, EventArgs __)
        {
            control.LayoutUpdated -= OnLayoutReady;

            var pos = GetVisualPosition(control);
            if (pos != null)
            {
                if (control.HorizontalAlignment != HorizontalAlignment.Left)
                    SetX(control, pos.Value.X);

                if (control.VerticalAlignment != VerticalAlignment.Top)
                    SetY(control, pos.Value.Y);
            }

            ApplyAxis(control, isX: true);
            ApplyAxis(control, isX: false);
        }

        control.LayoutUpdated += OnLayoutReady;
    }

    private static Point? GetVisualPosition(Control control)
    {
        var parent = control.GetVisualParent();
        return parent is { } visualParent
            ? control.TranslatePoint(new Point(0, 0), visualParent)
            : null;
    }

    public static double? GetX(AvaloniaObject obj) => obj.GetValue(XProperty);
    public static void SetX(AvaloniaObject obj, double? value) => obj.SetValue(XProperty, value);

    public static double? GetY(AvaloniaObject obj) => obj.GetValue(YProperty);
    public static void SetY(AvaloniaObject obj, double? value) => obj.SetValue(YProperty, value);
}
