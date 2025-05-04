using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Headless.XUnit;
using Avalonia.IDE.ToolKit.Tests;
using Avalonia.Layout;

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
    
    [AvaloniaFact]
    public void Layout_ResetsPosition_OnAlignmentChange()
    {
        var window = new Window { Width = 300, Height = 300 };
        var panel = new Panel { Width = 250, Height = 250, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        window.Content = panel;
        window.Show();

        Layout.SetX(panel, 50);
        typeof(Layout).GetMethod("EnsureInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { panel });

        panel.HorizontalAlignment = HorizontalAlignment.Right;
        var transform = panel.RenderTransform as TranslateTransform;
        Assert.Equal(0, transform!.X, 1); // При Center X должен сброситься
    }
}