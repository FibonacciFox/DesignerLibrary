using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Threading;

namespace Avalonia.IDE.ToolKit;

/// <summary>
/// Предоставляет attached-свойства <c>Layout.X</c> и <c>Layout.Y</c>
/// для управления позиционированием элементов внутри различных контейнеров.
/// При размещении в <see cref="Canvas"/> используются <c>Canvas.Left</c> и <c>Canvas.Top</c>,
/// в остальных случаях применяется <see cref="TranslateTransform"/> при <c>HorizontalAlignment.Left</c> и <c>VerticalAlignment.Top</c>.
/// </summary>
public static class Layout
{
    /// <summary>
    /// Задаёт или получает координату X.
    /// В <see cref="Canvas"/> применяет <see cref="Canvas.LeftProperty"/>.
    /// </summary>
    public static readonly AttachedProperty<double?> XProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, double?>(
            "X", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Задаёт или получает координату Y.
    /// В <see cref="Canvas"/> применяет <see cref="Canvas.TopProperty"/>.
    /// </summary>
    public static readonly AttachedProperty<double?> YProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, double?>(
            "Y", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.TwoWay);

    private static readonly AttachedProperty<bool> IsSubscribedProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, bool>(
            "IsSubscribed", typeof(Layout), false);

    static Layout()
    {
        XProperty.Changed.Subscribe(e => OnPositionChanged(e, isX: true));
        YProperty.Changed.Subscribe(e => OnPositionChanged(e, isX: false));
    }

    /// <summary>
    /// Обрабатывает изменение значения <c>X</c> или <c>Y</c>.
    /// </summary>
    /// <param name="e">Информация об изменении свойства.</param>
    /// <param name="isX">True для оси X, false — для Y.</param>
    private static void OnPositionChanged(AvaloniaPropertyChangedEventArgs e, bool isX)
    {
        if (e.Sender is Control control)
        {
            EnsureInitialized(control);
            ApplyAxis(control, isX);
        }
    }

    /// <summary>
    /// Инициализирует подписку на изменения выравнивания или Canvas-свойств.
    /// </summary>
    /// <param name="control">Контрол, к которому применяется позиционирование.</param>
    private static void EnsureInitialized(Control control)
    {
        if (!control.GetValue(IsSubscribedProperty))
        {
            control.SetValue(IsSubscribedProperty, true);

            if (IsInsideCanvas(control))
            {
                control.GetPropertyChangedObservable(Canvas.LeftProperty)
                       .Subscribe(_ => SyncFromCanvas(control, isX: true));
                control.GetPropertyChangedObservable(Canvas.TopProperty)
                       .Subscribe(_ => SyncFromCanvas(control, isX: false));
            }
            else
            {
                control.GetPropertyChangedObservable(Layoutable.HorizontalAlignmentProperty)
                       .Subscribe(_ => OnAlignmentChanged(control, isX: true));
                control.GetPropertyChangedObservable(Layoutable.VerticalAlignmentProperty)
                       .Subscribe(_ => OnAlignmentChanged(control, isX: false));
            }

            if (control.GetVisualRoot() == null)
                control.AttachedToVisualTree += FirstLayoutInit;
            else
                FirstLayoutInit(control, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Синхронизирует значение X или Y из Canvas.Left/Top в Layout.X/Y.
    /// </summary>
    /// <param name="control">Целевой контрол.</param>
    /// <param name="isX">True для X, false — для Y.</param>
    private static void SyncFromCanvas(Control control, bool isX)
    {
        if (!IsInsideCanvas(control))
            return;

        if (isX)
            SetX(control, Canvas.GetLeft(control));
        else
            SetY(control, Canvas.GetTop(control));
    }

    /// <summary>
    /// Обрабатывает изменение выравнивания и синхронизирует координаты.
    /// </summary>
    /// <param name="control">Контрол, выравнивание которого изменилось.</param>
    /// <param name="isX">True для X, false — для Y.</param>
    private static void OnAlignmentChanged(Control control, bool isX)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var pos = GetVisualPosition(control);
            var offset = control.RenderTransform as TranslateTransform ?? new TranslateTransform();

            if (isX)
            {
                if (control.HorizontalAlignment != HorizontalAlignment.Left)
                    SetX(control, pos?.X + offset.X ?? 0);
                else
                    SetX(control, 0);
            }
            else
            {
                if (control.VerticalAlignment != VerticalAlignment.Top)
                    SetY(control, pos?.Y + offset.Y ?? 0);
                else
                    SetY(control, 0);
            }

            ApplyAxis(control, isX);
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Применяет значение X или Y к контролу, в зависимости от типа контейнера.
    /// </summary>
    /// <param name="control">Целевой контрол.</param>
    /// <param name="isX">True для X, false — для Y.</param>
    private static void ApplyAxis(Control control, bool isX)
    {
        var x = GetX(control) ?? 0;
        var y = GetY(control) ?? 0;

        if (IsInsideCanvas(control))
        {
            if (isX) Canvas.SetLeft(control, x);
            else Canvas.SetTop(control, y);

            control.RenderTransform = null;
        }
        else
        {
            var transform = control.RenderTransform as TranslateTransform ?? new TranslateTransform();

            if (isX)
                transform.X = control.HorizontalAlignment == HorizontalAlignment.Left ? x : 0;
            else
                transform.Y = control.VerticalAlignment == VerticalAlignment.Top ? y : 0;

            control.RenderTransform = transform;
        }
    }

    /// <summary>
    /// Выполняет начальную установку координат после первого layout-прохода.
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

            if (pos != null && !IsInsideCanvas(control))
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
    /// Возвращает визуальную позицию контрола внутри родителя.
    /// </summary>
    /// <param name="control">Контрол для вычисления позиции.</param>
    /// <returns>Точка начала контрола относительно родителя.</returns>
    private static Point? GetVisualPosition(Control control)
    {
        var parent = control.GetVisualParent();
        return parent is { } visualParent
            ? control.TranslatePoint(new Point(0, 0), visualParent)
            : null;
    }

    /// <summary>
    /// Определяет, находится ли контрол внутри <see cref="Canvas"/>.
    /// </summary>
    /// <param name="control">Контрол для проверки.</param>
    /// <returns>True, если родитель — Canvas.</returns>
    private static bool IsInsideCanvas(Control control)
    {
        return control.GetVisualParent() is Canvas;
    }

    /// <summary>
    /// Получает значение Layout.X.
    /// </summary>
    /// <param name="obj">Объект, к которому привязано свойство.</param>
    /// <returns>Текущее значение X.</returns>
    public static double? GetX(AvaloniaObject obj) => obj.GetValue(XProperty);

    /// <summary>
    /// Устанавливает значение Layout.X.
    /// </summary>
    /// <param name="obj">Объект, к которому привязано свойство.</param>
    /// <param name="value">Новое значение X.</param>
    public static void SetX(AvaloniaObject obj, double? value) => obj.SetValue(XProperty, value);

    /// <summary>
    /// Получает значение Layout.Y.
    /// </summary>
    /// <param name="obj">Объект, к которому привязано свойство.</param>
    /// <returns>Текущее значение Y.</returns>
    public static double? GetY(AvaloniaObject obj) => obj.GetValue(YProperty);

    /// <summary>
    /// Устанавливает значение Layout.Y.
    /// </summary>
    /// <param name="obj">Объект, к которому привязано свойство.</param>
    /// <param name="value">Новое значение Y.</param>
    public static void SetY(AvaloniaObject obj, double? value) => obj.SetValue(YProperty, value);
}
