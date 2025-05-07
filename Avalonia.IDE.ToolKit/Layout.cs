using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Threading;
using System.Reactive.Disposables;

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
    /// Получает или задаёт координату X.
    /// В <see cref="Canvas"/> применяется <see cref="Canvas.LeftProperty"/>.
    /// </summary>
    public static readonly AttachedProperty<double> XProperty = AvaloniaProperty.RegisterAttached<Control, double>("X", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.TwoWay);
   
    /// <summary>
    /// Получает или задаёт координату Y.
    /// В <see cref="Canvas"/> применяется <see cref="Canvas.TopProperty"/>.
    /// </summary>
    public static readonly AttachedProperty<double> YProperty = AvaloniaProperty.RegisterAttached<Control, double>("Y", typeof(Layout), double.NaN, inherits: false, defaultBindingMode: BindingMode.TwoWay);

    private static readonly AttachedProperty<CompositeDisposable?> SubscriptionsProperty = AvaloniaProperty.RegisterAttached<AvaloniaObject, CompositeDisposable?>(
            "Subscriptions", typeof(Layout));

    static Layout()
    {
        XProperty.Changed.Subscribe(e => OnPositionChanged(e, isX: true));
        YProperty.Changed.Subscribe(e => OnPositionChanged(e, isX: false));
    }

    /// <summary>
    /// Обрабатывает изменения свойств <c>X</c> или <c>Y</c>.
    /// </summary>
    private static void OnPositionChanged(AvaloniaPropertyChangedEventArgs e, bool isX)
    {
        if (e.Sender is Control control)
        {
            EnsureInitialized(control);
            ApplyAxis(control, isX);
        }
    }

    /// <summary>
    /// Инициализирует подписки на изменения выравнивания, размеров или свойств Canvas.
    /// </summary>
    private static void EnsureInitialized(Control control)
    {
        if (control.GetValue(SubscriptionsProperty) != null)
            return;

        var subscriptions = new CompositeDisposable();
        control.SetValue(SubscriptionsProperty, subscriptions);

        // Обрабатывает отключение для очистки подписок
        control.DetachedFromVisualTree += (_, _) =>
        {
            subscriptions.Dispose();
            control.SetValue(SubscriptionsProperty, null);
        };

        // Подписка на изменения Canvas или выравнивания и размеров
        if (IsInsideCanvas(control))
        {
            subscriptions.Add(control.GetPropertyChangedObservable(Canvas.LeftProperty)
                .Subscribe(_ => SyncFromCanvas(control, isX: true)));
            subscriptions.Add(control.GetPropertyChangedObservable(Canvas.TopProperty)
                .Subscribe(_ => SyncFromCanvas(control, isX: false)));
        }
        else
        {
            subscriptions.Add(control.GetPropertyChangedObservable(Layoutable.HorizontalAlignmentProperty)
                .Subscribe(_ => OnAlignmentChanged(control, isX: true)));
            subscriptions.Add(control.GetPropertyChangedObservable(Layoutable.VerticalAlignmentProperty)
                .Subscribe(_ => OnAlignmentChanged(control, isX: false)));
            subscriptions.Add(control.GetPropertyChangedObservable(Layoutable.WidthProperty)
                .Subscribe(_ => OnSizeChanged(control, isX: true)));
            subscriptions.Add(control.GetPropertyChangedObservable(Layoutable.HeightProperty)
                .Subscribe(_ => OnSizeChanged(control, isX: false)));
        }

        // Немедленное применение начальной позиции, если она задана
        var x = GetX(control);
        var y = GetY(control);
        if (!double.IsNaN(x))
            ApplyAxis(control, isX: true);
        if (!double.IsNaN(y))
            ApplyAxis(control, isX: false);

        // Настройка начального layout
        if (control.GetVisualRoot() == null)
        {
            void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
            {
                control.AttachedToVisualTree -= OnAttached;
                FirstLayoutInit(control);
            }
            control.AttachedToVisualTree += OnAttached;
            subscriptions.Add(Disposable.Create(() => control.AttachedToVisualTree -= OnAttached));
        }
        else
        {
            FirstLayoutInit(control);
        }
    }

    /// <summary>
    /// Синхронизирует значение X или Y из Canvas.Left/Top в Layout.X/Y.
    /// </summary>
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
    /// Обрабатывает изменения выравнивания и синхронизирует координаты.
    /// </summary>
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
    /// Обрабатывает изменения размеров и синхронизирует координаты.
    /// </summary>
    private static void OnSizeChanged(Control control, bool isX)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var pos = GetVisualPosition(control);
            var offset = control.RenderTransform as TranslateTransform ?? new TranslateTransform();

            if (isX && control.HorizontalAlignment == HorizontalAlignment.Right)
            {
                var currentX = GetX(control);
                if (!double.IsNaN(currentX) && pos != null)
                {
                    SetX(control, pos.Value.X + offset.X);
                }
            }
            else if (!isX && control.VerticalAlignment == VerticalAlignment.Bottom)
            {
                var currentY = GetY(control);
                if (!double.IsNaN(currentY) && pos != null)
                {
                    SetY(control, pos.Value.Y + offset.Y);
                }
            }

            ApplyAxis(control, isX);
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Применяет значение X или Y к контролу в зависимости от типа контейнера.
    /// </summary>
    private static void ApplyAxis(Control control, bool isX)
    {
        var x = GetX(control);
        var y = GetY(control);

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
    /// Выполняет начальную настройку координат после первого прохода layout.
    /// </summary>
    private static void FirstLayoutInit(Control control)
    {
        void OnLayoutReady(object? s, EventArgs e)
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
    /// Определяет, находится ли контрол внутри <see cref="Canvas"/>.
    /// </summary>
    private static bool IsInsideCanvas(Control control) => control.GetVisualParent() is Canvas;

    /// <summary>
    /// Получает визуальную позицию контрола относительно родителя.
    /// </summary>
    private static Point? GetVisualPosition(Control control)
    {
        var parent = control.GetVisualParent();
        return parent is { } visualParent
            ? control.TranslatePoint(new Point(0, 0), visualParent)
            : null;
    }

    /// <summary>
    /// Получает значение Layout.X.
    /// </summary>
    public static double GetX(AvaloniaObject obj) => obj.GetValue(XProperty);

    /// <summary>
    /// Устанавливает значение Layout.X.
    /// </summary>
    public static void SetX(AvaloniaObject obj, double value) => obj.SetValue(XProperty, value);

    /// <summary>
    /// Получает значение Layout.Y.
    /// </summary>
    public static double GetY(AvaloniaObject obj) => obj.GetValue(YProperty);

    /// <summary>
    /// Устанавливает значение Layout.Y.
    /// </summary>
    public static void SetY(AvaloniaObject obj, double value) => obj.SetValue(YProperty, value);
}