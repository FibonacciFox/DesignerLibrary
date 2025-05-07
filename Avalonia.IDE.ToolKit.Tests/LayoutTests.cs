using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Headless.XUnit;
using Avalonia.IDE.ToolKit.Tests;
using Avalonia.Layout;
using Avalonia.Threading;

[assembly: AvaloniaTestApplication(typeof(LayoutTests))]

namespace Avalonia.IDE.ToolKit.Tests;

/// <summary>
/// Тесты для класса <see cref="Layout"/> с использованием headless-окружения Avalonia.
/// </summary>
public class LayoutTests
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());

    /// <summary>
    /// Проверяет, что <see cref="Layout"/> немедленно применяет начальную позицию.
    /// </summary>
    [AvaloniaFact]
    public void Layout_AppliesInitialPosition_Immediately()
    {
        // Создаем Window для полноценного визуального дерева
        var window = new Window { Width = 300, Height = 300 };
        var panel = new Panel { Width = 250, Height = 250, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        window.Content = panel;

        // Установка начальных координат
        Layout.SetX(panel, 50);
        Layout.SetY(panel, 40);

        // Вызов приватного статического метода EnsureInitialized через рефлексию
        typeof(Layout).GetMethod("EnsureInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { panel });

        // Проверка применения координат через TranslateTransform
        var transform = panel.RenderTransform as TranslateTransform;
        Assert.NotNull(transform);
        Assert.Equal(50, transform.X, 1); // Точность до 1 единицы из-за возможных округлений
        Assert.Equal(40, transform.Y, 1);
    }

    /// <summary>
    /// Проверяет, что <see cref="Layout"/> применяет позицию в Canvas.
    /// </summary>
    [AvaloniaFact]
    public void Layout_AppliesPosition_InCanvas()
    {
        // Создаем Window для полноценного визуального дерева
        var window = new Window { Width = 300, Height = 300 };
        var canvas = new Canvas();
        var panel = new Panel { Width = 250, Height = 250 };
        canvas.Children.Add(panel);
        window.Content = canvas;

        // Установка начальных координат
        Layout.SetX(panel, 50);
        Layout.SetY(panel, 40);

        // Вызов приватного статического метода EnsureInitialized через рефлексию
        typeof(Layout).GetMethod("EnsureInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { panel });

        // Проверка применения координат в Canvas
        Assert.Equal(50, Canvas.GetLeft(panel), 1);
        Assert.Equal(40, Canvas.GetTop(panel), 1);
    }

    /// <summary>
    /// Проверяет динамическое изменение позиции <see cref="Layout"/>.
    /// </summary>
    [AvaloniaFact]
    public void Layout_AppliesDynamicPosition_Change()
    {
        // Создаем Window для полноценного визуального дерева
        var window = new Window { Width = 300, Height = 300 };
        var panel = new Panel { Width = 250, Height = 250, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        window.Content = panel;

        // Открываем Window в headless-режиме для инициализации визуального дерева
        window.Show();

        // Установка начальных координат и инициализация
        Layout.SetX(panel, 50);
        Layout.SetY(panel, 40);
        typeof(Layout).GetMethod("EnsureInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { panel });

        // Динамическое изменение координаты X
        Layout.SetX(panel, 100);

        // Проверка применения новой координаты через TranslateTransform
        var transform = panel.RenderTransform as TranslateTransform;
        Assert.NotNull(transform);
        Assert.Equal(100, transform.X, 1); // Точность до 1 единицы из-за возможных округлений
        Assert.Equal(40, transform.Y, 1);
    }

    /// <summary>
    /// Проверяет сброс позиции при изменении <see cref="HorizontalAlignment"/> в <see cref="Layout"/> на Left.
    /// </summary>
    [AvaloniaFact]
    public void Layout_ResetsPosition_OnAlignmentChange1()
    {
        // Создаем Window для полноценного визуального дерева
        var window = new Window { Width = 300, Height = 300 };
        var panel = new Panel { Width = 250, Height = 250, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        window.Content = panel;
        window.Show();
        
        // Установка начальной позиции
        Layout.SetX(panel, 100);

        // Изменение выравнивания на Right
        panel.HorizontalAlignment = HorizontalAlignment.Right;

        // Изменение выравнивания обратно на Left
        panel.HorizontalAlignment = HorizontalAlignment.Left;

        // Дожидаемся выполнения асинхронных операций в Dispatcher
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);

        // Проверка сброса позиции
        var transform = panel.RenderTransform as TranslateTransform;
        Assert.NotNull(transform);
        Assert.Equal(0, transform.X, 1); // При Left X должен сброситься до 0
    }

    /// <summary>
    /// Проверяет сброс позиции при изменении <see cref="VerticalAlignment"/> в <see cref="Layout"/> на Top.
    /// </summary>
    [AvaloniaFact]
    public void Layout_ResetsPosition_OnVerticalAlignmentChange()
    {
        var window = new Window { Width = 300, Height = 300 };
        var panel = new Panel { Width = 250, Height = 250, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Bottom };
        window.Content = panel;
        window.Show();
        
        Layout.SetY(panel, 100);
        panel.VerticalAlignment = VerticalAlignment.Top;
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);

        var transform = panel.RenderTransform as TranslateTransform;
        Assert.NotNull(transform);
        Assert.Equal(0, transform.Y, 1); // При Top Y должен сброситься до 0
    }

    /// <summary>
    /// Проверяет поведение <see cref="Layout"/> при отсутствии начальных значений X и Y.
    /// </summary>
    [AvaloniaFact]
    public void Layout_HandlesMissingInitialValues()
    {
        var window = new Window { Width = 300, Height = 300 };
        var panel = new Panel { Width = 250, Height = 250, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        window.Content = panel;
        window.Show();

        typeof(Layout).GetMethod("EnsureInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { panel });

        var transform = panel.RenderTransform as TranslateTransform;
        Assert.Null(transform); // Transform не должен создаваться, если X и Y не заданы
    }

    /// <summary>
    /// Проверяет синхронизацию <see cref="Layout"/> из Canvas.Left в Layout.X.
    /// </summary>
    [AvaloniaFact]
    public void Layout_SyncsFromCanvas_Left()
    {
        var window = new Window { Width = 300, Height = 300 };
        var canvas = new Canvas();
        var panel = new Panel { Width = 250, Height = 250 };
        canvas.Children.Add(panel);
        window.Content = canvas;
        window.Show();

        Layout.SetX(panel, 50);
        Canvas.SetLeft(panel, 75);
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);

        Assert.Equal(75, Layout.GetX(panel), 1); // Layout.X должен синхронизироваться с Canvas.Left
        Assert.Equal(75, Canvas.GetLeft(panel), 1);
    }

    /// <summary>
    /// Проверяет очистку подписок при отключении из визуального дерева.
    /// </summary>
    [AvaloniaFact]
    public void Layout_CleansSubscriptions_OnDetach()
    {
        var window = new Window { Width = 300, Height = 300 };
        var panel = new Panel { Width = 250, Height = 250, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        window.Content = panel;
        window.Show();

        Layout.SetX(panel, 50);

        // Отключаем panel из визуального дерева
        window.Content = null;
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);

        var subscriptions = panel.GetValue(typeof(Layout).GetField("SubscriptionsProperty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.GetValue(null) as AvaloniaProperty<CompositeDisposable?> ?? throw new InvalidOperationException());
        Assert.Null(subscriptions); // Подписки должны быть очищены
    }

    /// <summary>
    /// Проверяет инициализацию позиции после подключения к визуальному дереву.
    /// </summary>
    [AvaloniaFact]
    public void Layout_InitializesPosition_OnAttachToVisualTree()
    {
        var window = new Window { Width = 300, Height = 300 };
        var panel = new Panel { Width = 250, Height = 250, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        
        Layout.SetX(panel, 50);
        window.Content = panel;
        window.Show();
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);

        Assert.Equal(50, Layout.GetX(panel), 1); // Layout.X должен сохраниться
        var transform = panel.RenderTransform as TranslateTransform;
        Assert.NotNull(transform);
        Assert.Equal(50, transform.X, 1); // Transform.X должен быть 50, так как HorizontalAlignment != Left
    }

    /// <summary>
    /// Проверяет асинхронное поведение <see cref="Layout"/> при изменении выравнивания.
    /// </summary>
    [AvaloniaFact]
    public void Layout_AppliesAlignmentChange_Asynchronously()
    {
        var window = new Window { Width = 300, Height = 300 };
        var panel = new Panel { Width = 250, Height = 250, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        window.Content = panel;
        window.Show();

        Layout.SetX(panel, 50);
        
        panel.HorizontalAlignment = HorizontalAlignment.Right;

        // Проверяем до выполнения асинхронных операций
        var transform = panel.RenderTransform as TranslateTransform;
        Assert.Equal(50, transform.X, 1); // Изменение еще не применено

        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);
        Assert.Equal(0, transform.X, 1); // После выполнения Dispatcher изменений применены
    }

    /// <summary>
    /// Проверяет работу <see cref="Layout"/> внутри StackPanel.
    /// </summary>
    [AvaloniaFact]
    public void Layout_WorksInsideStackPanel()
    {
        var window = new Window { Width = 300, Height = 300 };
        var stackPanel = new StackPanel();
        var panel = new Panel { Width = 250, Height = 250, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        stackPanel.Children.Add(panel);
        window.Content = stackPanel;
        window.Show();

        Layout.SetX(panel, 50);
        Layout.SetY(panel, 40);

        var transform = panel.RenderTransform as TranslateTransform;
        Assert.NotNull(transform);
        Assert.Equal(50, transform.X, 1);
        Assert.Equal(40, transform.Y, 1);
    }

    /// <summary>
    /// Проверяет перерасчет позиции X при изменении ширины контрола.
    /// </summary>
    [AvaloniaFact]
    public void Layout_RecalculatesPosition_OnWidthChange()
    {
        var window = new Window { Width = 300, Height = 300 };
        var panel = new Panel { Width = 250, Height = 250, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top };
        window.Content = panel;
        window.Show();

        Layout.SetX(panel, 50);
        typeof(Layout).GetMethod("EnsureInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { panel });

        // Изменяем ширину контрола
        panel.Width = 200;
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);

        var transform = panel.RenderTransform as TranslateTransform;
        Assert.NotNull(transform);
        Assert.Equal(0, transform.X, 1); // При HorizontalAlignment.Right X должен сброситься до 0
    }

    /// <summary>
    /// Проверяет перерасчет позиции Y при изменении высоты контрола.
    /// </summary>
    [AvaloniaFact]
    public void Layout_RecalculatesPosition_OnHeightChange()
    {
        var window = new Window { Width = 300, Height = 300 };
        var panel = new Panel { Width = 250, Height = 250, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Bottom };
        window.Content = panel;
        window.Show();

        Layout.SetY(panel, 40);
        typeof(Layout).GetMethod("EnsureInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { panel });

        // Изменяем высоту контрола
        panel.Height = 200;
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);

        var transform = panel.RenderTransform as TranslateTransform;
        Assert.NotNull(transform);
        Assert.Equal(0, transform.Y, 1); // При VerticalAlignment.Bottom Y должен сброситься до 0
    }
}