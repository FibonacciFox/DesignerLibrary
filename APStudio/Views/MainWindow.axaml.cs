using System.Reactive;
using Avalonia.Controls;
using ReactiveUI;

namespace APStudio.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        MinimizeCommand = ReactiveCommand.Create(() => WindowState = WindowState.Minimized);
        MaximizeCommand = ReactiveCommand.Create(() =>
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        });
        CloseCommand = ReactiveCommand.Create(Close);
    }
    
    public ReactiveCommand<Unit, WindowState> MinimizeCommand { get; }
    public ReactiveCommand<Unit, Unit> MaximizeCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
}