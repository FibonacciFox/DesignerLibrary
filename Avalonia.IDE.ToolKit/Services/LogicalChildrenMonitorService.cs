using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace Avalonia.IDE.ToolKit.Services
{
    /// <summary>
    /// Сервис мониторинга логических детей элемента управления в Avalonia.
    /// This service monitors logical children of a control in Avalonia.
    /// </summary>
    public class LogicalChildrenMonitorService : ILogicalChildrenMonitorService
    {
        /// <summary>
        /// Событие, вызываемое при добавлении нового элемента управления.
        /// Event triggered when a new control is added.
        /// </summary>
        public event Action<Control> ChildAdded = delegate { };

        /// <summary>
        /// Событие, вызываемое при удалении элемента управления.
        /// Event triggered when a control is removed.
        /// </summary>
        public event Action<Control> ChildRemoved = delegate { };

        /// <summary>
        /// Коллекция логических детей, которые отслеживаются.
        /// Collection of logical children being monitored.
        /// </summary>
        public ObservableCollection<Control> LogicalChildren { get; } = new();

        private readonly WeakReference<Control> _controlReference;
        private readonly Type[]? _excludedTypes;
        private Timer? _monitorTimer;
        private HashSet<Control> _previousChildren = new();
        private bool _disposed;

        /// <summary>
        /// Конструктор сервиса мониторинга логических детей.
        /// Constructor for the logical children monitoring service.
        /// </summary>
        /// <param name="control">Элемент управления, чьи логические дети будут отслеживаться. The control whose logical children will be monitored.</param>
        /// <param name="excludedTypes">Необязательный массив типов контролов, которые не нужно отслеживать. Optional array of control types to exclude from monitoring.</param>
        public LogicalChildrenMonitorService(Control control, Type[]? excludedTypes = null)
        {
            _controlReference = new WeakReference<Control>(control ?? throw new ArgumentNullException(nameof(control)));
            _excludedTypes = excludedTypes;
        }

        /// <summary>
        /// Начинает мониторинг изменений логических детей.
        /// Starts monitoring changes to the logical children.
        /// </summary>
        public void StartMonitoring()
        {
            if (_monitorTimer != null)
            {
                throw new InvalidOperationException("Monitoring is already started.");
            }

            _monitorTimer = new Timer(MonitorTreeChanges, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.3));
        }

        /// <summary>
        /// Останавливает мониторинг изменений логических детей.
        /// Stops monitoring changes to the logical children.
        /// </summary>
        public void StopMonitoring()
        {
            _monitorTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _monitorTimer?.Dispose();
            _monitorTimer = null;
        }

        /// <summary>
        /// Метод, вызываемый таймером для проверки изменений в логическом дереве.
        /// Method called by the timer to check for changes in the logical tree.
        /// </summary>
        /// <param name="state">Состояние таймера. Timer state.</param>
        private void MonitorTreeChanges(object? state)
        {
            if (_controlReference.TryGetTarget(out var control))
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var currentChildren = new HashSet<Control>(
                        control.GetLogicalDescendants()
                               .OfType<Control>()
                               .Where(c => _excludedTypes == null || !_excludedTypes.Any(t => t.IsInstanceOfType(c)))
                    );
                    CheckForChanges(currentChildren);
                });
            }
            else
            {
                StopMonitoring();
            }
        }

        /// <summary>
        /// Проверяет изменения в логических детях и вызывает соответствующие события.
        /// Checks for changes in logical children and triggers the appropriate events.
        /// </summary>
        /// <param name="currentChildren">Текущие логические дети. Current logical children.</param>
        private void CheckForChanges(HashSet<Control> currentChildren)
        {
            var newChildren = currentChildren.Except(_previousChildren).ToList();
            var removedChildren = _previousChildren.Except(currentChildren).ToList();

            if (newChildren.Count > 0 || removedChildren.Count > 0)
            {
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

                _previousChildren = currentChildren;
            }
        }

        /// <summary>
        /// Освобождает ресурсы, используемые сервисом.
        /// Releases the resources used by the service.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Защищенный метод для освобождения ресурсов.
        /// Protected method for releasing resources.
        /// </summary>
        /// <param name="disposing">Флаг, указывающий, должно ли происходить освобождение управляемых ресурсов. Flag indicating whether managed resources should be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _monitorTimer?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
