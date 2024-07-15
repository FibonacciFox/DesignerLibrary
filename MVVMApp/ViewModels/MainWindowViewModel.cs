using Avalonia.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.IDE.ToolKit.Services; // Убедитесь, что пространство имён правильное

namespace MVVMApp.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly LogicalChildrenMonitorService _monitorService;
    private readonly Control _controlToMonitor; // Хранение ссылки на контроль

    public string Greeting => "Welcome to Avalonia!";
    public ObservableCollection<Control> LogicalChildren { get; private set; }
    public ICommand AddButtonCommand { get; }

    public MainWindowViewModel(Control controlToMonitor)
    {
        _controlToMonitor = controlToMonitor; // Сохранение переданного контроля
        LogicalChildren = new ObservableCollection<Control>();
        _monitorService = new LogicalChildrenMonitorService(_controlToMonitor);
        _monitorService.ChildAdded += child => LogicalChildren.Add(child);
        _monitorService.ChildRemoved += child => LogicalChildren.Remove(child);
        _monitorService.StartMonitoring();
        AddButtonCommand = new RelayCommand(AddButton);
    }

    private void AddButton()
    {
        // Добавление кнопки на контрол для мониторинга
        var newButton = new Button { Content = "New Button" };
        ((Panel)_controlToMonitor).Children.Add(newButton);
    }

    public void Dispose()
    {
        _monitorService.StopMonitoring();
        _monitorService.ChildAdded -= OnChildAdded;
        _monitorService.ChildRemoved -= OnChildRemoved;
        _monitorService.Dispose();
    }

    private void OnChildAdded(Control child)
    {
        // Действия при добавлении дочернего элемента
        Console.WriteLine($"Child added: {child}");
    }

    private void OnChildRemoved(Control child)
    {
        // Действия при удалении дочернего элемента
        Console.WriteLine($"Child removed: {child}");
    }
}
