using Avalonia.Markup.Xaml;

namespace Avalonia.IDE.ToolKit.Tests;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}