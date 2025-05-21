
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Headless.XUnit;
using Avalonia.IDE.ToolKit.Controls.Designer;
using Avalonia.IDE.ToolKit.Extensions;
using Avalonia.IDE.ToolKit.Tests;
using Avalonia.Layout;
using Avalonia.Threading;

[assembly: AvaloniaTestApplication(typeof(LayoutTests))]

namespace Avalonia.IDE.ToolKit.Tests;

/// <summary>
/// Тесты для класса <see cref="Layout"/> с использованием headless-окружения Avalonia.
/// </summary>
/// <summary>
/// xUnit-тесты для проверки поведения класса <see cref="Layout"/>.
/// </summary>
public class LayoutTests
{
    /// <summary>
    /// Создаёт и конфигурирует Avalonia-приложение для headless-тестирования.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());

    /// <summary>
    /// Проверяет применение X и Y через TranslateTransform внутри Grid.
    /// </summary>
    [AvaloniaFact]
    public async Task Applies_XY_In_Grid()
    {
        var panel = new Panel { Width = 100, Height = 100 };
        Extensions.Layout.SetX(panel, 20);
        Extensions.Layout.SetY(panel, 30);

        var grid = new Grid { Width = 500, Height = 500 };
        grid.Children.Add(panel);

        var window = new Window { Content = grid };
        window.Show();

        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Loaded);

        var transform = panel.RenderTransform as TranslateTransform;
        Assert.NotNull(transform);
        Assert.Equal(20, transform.X, 1);
        Assert.Equal(30, transform.Y, 1);
    }

    /// <summary>
    /// Проверяет применение X и Y через Canvas.Left и Canvas.Top внутри Canvas.
    /// </summary>
    [AvaloniaFact]
    public async Task Applies_XY_In_Canvas()
    {
        var panel = new Panel { Width = 100, Height = 100 };
        Extensions.Layout.SetX(panel, 60);
        Extensions.Layout.SetY(panel, 80);

        var canvas = new Canvas();
        canvas.Children.Add(panel);

        var window = new Window { Content = canvas };
        window.Show();

        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Loaded);

        Assert.Equal(60, Canvas.GetLeft(panel), 1);
        Assert.Equal(80, Canvas.GetTop(panel), 1);
    }

    /// <summary>
    /// Проверяет пересчёт позиции при смене HorizontalAlignment.
    /// </summary>
    [AvaloniaFact]
    public async Task Updates_Position_On_Alignment_Change()
    {
        var panel = new Panel { Width = 100, Height = 100, HorizontalAlignment = HorizontalAlignment.Right };
        Extensions.Layout.SetX(panel, 50);

        var grid = new Grid { Width = 300, Height = 300 };
        grid.Children.Add(panel);

        var window = new Window { Content = grid };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Loaded);

        panel.HorizontalAlignment = HorizontalAlignment.Left;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Loaded);

        var transform = panel.RenderTransform as TranslateTransform;
        Assert.NotNull(transform);
        Assert.Equal(0, transform.X, 1);
    }

    /// <summary>
    /// Проверяет обновление DesignX и DesignY относительно UiDesigner.
    /// </summary>
    [AvaloniaFact]
    public async Task Updates_Design_Position()
    {
        var designer = new UiDesigner { Width = 500, Height = 500 };
        var panel = new Panel { Width = 100, Height = 100 };

        Extensions.Layout.SetX(panel, 40);
        Extensions.Layout.SetY(panel, 70);

        designer.Children.Add(panel);

        var window = new Window { Content = designer };
        window.Show();

        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Loaded);

        var dx = Extensions.Layout.GetDesignX(panel);
        var dy = Extensions.Layout.GetDesignY(panel);

        Assert.True(dx >= 0);
        Assert.True(dy >= 0);
    }

    /// <summary>
    /// Проверяет, что NaN в X и Y не приводит к созданию Transform.
    /// </summary>
    [AvaloniaFact]
    public async Task Handles_NaN_Values_Gracefully()
    {
        var panel = new Panel { Width = 100, Height = 100 };
        Extensions.Layout.SetX(panel, double.NaN);
        Extensions.Layout.SetY(panel, double.NaN);

        var stack = new StackPanel();
        stack.Children.Add(panel);

        var window = new Window { Content = stack };
        window.Show();

        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Loaded);

        Assert.Null(panel.RenderTransform);
    }
}