using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace Avalonia.IDE.ToolKit.Services;

public class LogicalChildrenMonitorService : ILogicalChildrenMonitorService
{
    public event Action<Control> ChildAdded;
    public event Action<Control> ChildRemoved;

    public ObservableCollection<Control> LogicalChildren { get; } = new();

    public LogicalChildrenMonitorService(Control control)
    {
        _control = control;
    }

    public void StartMonitoring()
    {
        _monitorTimer = new Timer(MonitorTreeChanges!, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.5));
    }

    public void StopMonitoring()
    {
        _monitorTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _monitorTimer?.Dispose();
        _monitorTimer = null;
    }

    private void MonitorTreeChanges(object state)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var currentChildren = new HashSet<Control>(_control.GetLogicalDescendants().OfType<Control>());
            CheckForChanges(currentChildren);
            _previousChildren = currentChildren;
        });
    }

    private void CheckForChanges(HashSet<Control> currentChildren)
    {
        var newChildren = currentChildren.Except(_previousChildren).ToList();
        var removedChildren = _previousChildren.Except(currentChildren).ToList();

        foreach (var child in newChildren)
        {
            LogicalChildren.Add(child);
            ChildAdded?.Invoke(child);
        }

        foreach (var child in removedChildren)
        {
            LogicalChildren.Remove(child);
            ChildRemoved?.Invoke(child);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects)
                _monitorTimer?.Dispose();
            }

            _disposed = true;
        }
    }
    
    private readonly Control _control;
    private Timer _monitorTimer;
    private HashSet<Control> _previousChildren = new();
    private bool _disposed;
}