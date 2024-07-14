using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Avalonia.IDE.ToolKit.Services;

public class LogicalChildrenMonitorService : ILogicalChildrenMonitorService
{
    public event Action<Control> ChildAdded;
    public event Action<Control> ChildRemoved;

    public ObservableCollection<Control> LogicalChildren { get; } = new();

    public LogicalChildrenMonitorService(Panel panel)
    {
        _panel = panel;
    }

    public void StartMonitoring()
    {
        _monitorTimer = new Timer(MonitorTreeChanges!, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.5));
    }

    public void StopMonitoring()
    {
        _monitorTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _monitorTimer?.Dispose();
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
                // Dispose managed state (managed objects).
                _monitorTimer?.Dispose();
            }

            // Free any unmanaged resources (unmanaged objects) and override a finalizer below.
            // Set large fields to null.

            _disposed = true;
        }
    }

    private void MonitorTreeChanges(object state)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var currentChildren = new HashSet<Control>(_panel.Children.OfType<Control>());
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
    
    private readonly Panel _panel;
    private Timer _monitorTimer;
    private HashSet<Control> _previousChildren = new HashSet<Control>();
    private bool _disposed = false;
}