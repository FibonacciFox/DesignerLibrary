using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Avalonia.IDE.ToolKit;

/// <summary>
/// Предоставляет attached-свойства <c>Layout.X</c> и <c>Layout.Y</c> для позиционирования элементов в пространстве.
/// Поддерживает независимое управление положением по осям X и Y. Координаты применяются через <see cref="TranslateTransform"/>
/// при условии, что выравнивание установлено в <see cref="HorizontalAlignment.Left"/> или <see cref="VerticalAlignment.Top"/> соответственно.
/// </summary>
public static class Layout
{
    /// <summary>
    /// Задаёт или получает абсолютную координату X относительно родителя.
    /// Применяется к элементу только при <see cref="HorizontalAlignment.Left"/>.
    /// </summary>
    public static readonly AttachedProperty<double?> XProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, double?>(
            "X", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Задаёт или получает абсолютную координату Y относительно родителя.
    /// Применяется к элементу только при <see cref="VerticalAlignment.Top"/>.
    /// </summary>
    public static readonly AttachedProperty<double?> YProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, double?>(
            "Y", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.TwoWay);

    private static readonly AttachedProperty<bool> IsAlignmentSubscribedProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, bool>(
            "IsAlignmentSubscribed", typeof(Layout), false);

    static Layout()
    {
        XProperty.Changed.Subscribe(e => OnPositionChanged(e, isX: true));
        YProperty.Changed.Subscribe(e => OnPositionChanged(e, isX: false));
    }

    /// <summary>
    /// Обрабатывает изменение значения <c>X</c> или <c>Y</c>.
    /// Подписывается на изменение выравнивания и применяет позицию.
    /// </summary>
    /// <param name="e">Событие изменения свойства.</param>
    /// <param name="isX">Определяет, обрабатывается ли ось X (true) или Y (false).</param>
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

    /// <summary>
    /// Обрабатывает изменение <see cref="HorizontalAlignment"/> или <see cref="VerticalAlignment"/>.
    /// При возврате к <c>Left</c> или <c>Top</c> позиция сбрасывается в (0).
    /// </summary>
    /// <param name="control">Контрол, выравнивание которого изменилось.</param>
    /// <param name="isX">True — если выравнивание по оси X; False — по Y.</param>
    private static void OnAlignmentChanged(Control control, bool isX)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var offset = control.RenderTransform as TranslateTransform ?? new TranslateTransform();
            var pos = GetVisualPosition(control);

            if (isX)
            {
                if (control.HorizontalAlignment != HorizontalAlignment.Left)
                {
                    if (pos != null)
                        SetX(control, pos.Value.X + offset.X);
                }
                else
                {
                    SetX(control, 0); // Сброс X при возврате в Left
                }
            }
            else
            {
                if (control.VerticalAlignment != VerticalAlignment.Top)
                {
                    if (pos != null)
                        SetY(control, pos.Value.Y + offset.Y);
                }
                else
                {
                    SetY(control, 0); // Сброс Y при возврате в Top
                }
            }

            ApplyAxis(control, isX);
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Применяет <see cref="TranslateTransform"/> к соответствующей оси.
    /// </summary>
    /// <param name="control">Контрол, к которому применяется трансформация.</param>
    /// <param name="isX">True — если применять по X, иначе — по Y.</param>
    private static void ApplyAxis(Control control, bool isX)
    {
        var transform = control.RenderTransform as TranslateTransform ?? new TranslateTransform();

        if (isX)
        {
            transform.X = control.HorizontalAlignment == HorizontalAlignment.Left
                ? GetX(control) ?? 0
                : 0;
        }
        else
        {
            transform.Y = control.VerticalAlignment == VerticalAlignment.Top
                ? GetY(control) ?? 0
                : 0;
        }

        control.RenderTransform = transform;
    }

    /// <summary>
    /// Запускает отложенную инициализацию координат после первого layout-прохода.
    /// </summary>
    /// <param name="control">Контрол для инициализации.</param>
    private static void EnsureFirstLayoutInitialized(Control control)
    {
        if (control.GetVisualRoot() == null)
            control.AttachedToVisualTree += FirstLayoutInit;
        else
            FirstLayoutInit(control, EventArgs.Empty);
    }

    /// <summary>
    /// Выполняет захват текущей позиции после завершения layout.
    /// </summary>
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

    /// <summary>
    /// Получает позицию контрола относительно его визуального родителя.
    /// </summary>
    /// <param name="control">Контрол, позицию которого нужно получить.</param>
    /// <returns>Позиция в пространстве родителя, или null если родитель не найден.</returns>
    private static Point? GetVisualPosition(Control control)
    {
        var parent = control.GetVisualParent();
        return parent is { } visualParent
            ? control.TranslatePoint(new Point(0, 0), visualParent)
            : null;
    }

    /// <summary>
    /// Получает значение свойства Layout.X.
    /// </summary>
    /// <param name="obj">Объект, с которого получить значение.</param>
    /// <returns>Текущее значение X.</returns>
    public static double? GetX(AvaloniaObject obj) => obj.GetValue(XProperty);

    /// <summary>
    /// Устанавливает значение свойства Layout.X.
    /// </summary>
    /// <param name="obj">Объект, которому задать значение.</param>
    /// <param name="value">Новое значение X.</param>
    public static void SetX(AvaloniaObject obj, double? value) => obj.SetValue(XProperty, value);

    /// <summary>
    /// Получает значение свойства Layout.Y.
    /// </summary>
    /// <param name="obj">Объект, с которого получить значение.</param>
    /// <returns>Текущее значение Y.</returns>
    public static double? GetY(AvaloniaObject obj) => obj.GetValue(YProperty);

    /// <summary>
    /// Устанавливает значение свойства Layout.Y.
    /// </summary>
    /// <param name="obj">Объект, которому задать значение.</param>
    /// <param name="value">Новое значение Y.</param>
    public static void SetY(AvaloniaObject obj, double? value) => obj.SetValue(YProperty, value);
}
