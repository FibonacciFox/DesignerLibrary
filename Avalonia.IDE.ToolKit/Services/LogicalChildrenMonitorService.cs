using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace Avalonia.IDE.ToolKit.Services
{
    /// <summary>
    /// Defines whether to monitor direct logical children or all descendants.
    /// </summary>
    public enum MonitorScope
    {
        /// <summary>
        /// Monitor only direct logical children.
        /// </summary>
        DirectChildren,

        /// <summary>
        /// Monitor all logical descendants.
        /// </summary>
        AllDescendants
    }

    /// <summary>
    /// Сервис мониторинга логических детей элемента управления в Avalonia.
    /// This service monitors logical children of a control in Avalonia.
    /// </summary>
    public class LogicalChildrenMonitorService : ILogicalChildrenMonitorService
    {
        /// <summary>
        /// Событие, вызываемое при добавлении нового элемента управления. Вызывается в UI-потоке.
        /// Event triggered when a new control is added. Raised on the UI thread.
        /// </summary>
        public event Action<Control> ChildAdded = delegate { };

        /// <summary>
        /// Событие, вызываемое при удалении элемента управления. Вызывается в UI-потоке.
        /// Event triggered when a control is removed. Raised on the UI thread.
        /// </summary>
        public event Action<Control> ChildRemoved = delegate { };

        /// <summary>
        /// Событие, вызываемое для логирования сообщений о действиях мониторинга. Вызывается в UI-потоке.
        /// Event triggered for logging messages about monitoring actions. Raised on the UI thread.
        /// </summary>
        public event Action<string> LogMessage = delegate { };

        /// <summary>
        /// Коллекция логических детей, которые отслеживаются.
        /// Collection of logical children being monitored.
        /// </summary>
        public ObservableCollection<Control> LogicalChildren { get; } = new();

        private readonly WeakReference<Control> _controlReference;
        private readonly Type[]? _excludedTypes;
        private readonly Func<Control, bool>? _additionalFilter;
        private readonly TimeSpan _pollingInterval;
        private readonly MonitorScope _monitorScope;
        private Timer? _monitorTimer;
        private HashSet<Control> _previousChildren = new();
        private bool _disposed;

        /// <summary>
        /// Конструктор сервиса мониторинга логических детей.
        /// Constructor for the logical children monitoring service.
        /// </summary>
        /// <param name="control">Элемент управления, чьи логические дети будут отслеживаться. The control whose logical children will be monitored.</param>
        /// <param name="excludedTypes">Необязательный массив типов контролов, которые не нужно отслеживать. Optional array of control types to exclude from monitoring.</param>
        /// <param name="additionalFilter">Необязательный фильтр для дополнительных условий исключения контролов. Optional filter for additional control exclusion conditions.</param>
        /// <param name="pollingInterval">Интервал опроса для проверки изменений (по умолчанию 0.3 секунды). Polling interval for checking changes (default is 0.3 seconds).</param>
        /// <param name="monitorScope">Область мониторинга: прямые дети или все потомки (по умолчанию все потомки). Monitoring scope: direct children or all descendants (default is all descendants).</param>
        public LogicalChildrenMonitorService(
            Control control,
            Type[]? excludedTypes = null,
            Func<Control, bool>? additionalFilter = null,
            TimeSpan? pollingInterval = null,
            MonitorScope monitorScope = MonitorScope.AllDescendants)
        {
            _controlReference = new WeakReference<Control>(control ?? throw new ArgumentNullException(nameof(control)));
            _excludedTypes = excludedTypes;
            _additionalFilter = additionalFilter;
            _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(0.3);
            _monitorScope = monitorScope;
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

            // Очищаем предыдущее состояние
            _previousChildren.Clear();
            LogicalChildren.Clear();

            // Откладываем начальную обработку до полной загрузки UI, чтобы логическое дерево было полностью инициализировано
            Dispatcher.UIThread.Post(() =>
            {
                if (_controlReference.TryGetTarget(out var control))
                {
                    var initialChildren = (_monitorScope == MonitorScope.AllDescendants
                        ? control.GetLogicalDescendants()
                        : control.GetLogicalChildren())
                        .OfType<Control>()
                        .Where(IsValidControl)
                        .ToList();
                    LogMessage($"Initial controls found: {initialChildren.Count} controls");
                    foreach (var child in initialChildren)
                    {
                        LogMessage?.Invoke($"  - {child.GetType().Name}, Name={child.Name ?? "Unnamed"}");
                    }
                    foreach (var child in initialChildren)
                    {
                        if (!LogicalChildren.Contains(child))
                        {
                            LogicalChildren.Add(child);
                            ChildAdded?.Invoke(child);
                            LogMessage?.Invoke($"Added initial child: {child.GetType().Name}, Name={child.Name ?? "Unnamed"}");
                        }
                    }
                    _previousChildren = new HashSet<Control>(initialChildren);
                }
            }, DispatcherPriority.Loaded);

            // Запускаем таймер для последующего мониторинга
            _monitorTimer = new Timer(MonitorTreeChanges, null, _pollingInterval, _pollingInterval);
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
                        (_monitorScope == MonitorScope.AllDescendants
                            ? control.GetLogicalDescendants()
                            : control.GetLogicalChildren())
                        .OfType<Control>()
                        .Where(c => IsValidControl(c))
                    );
                    LogMessage($"Current controls: {currentChildren.Count} controls");
                    foreach (var child in currentChildren)
                    {
                        LogMessage?.Invoke($"  - {child.GetType().Name}, Name={child.Name ?? "Unnamed"}");
                    }
                    CheckForChanges(currentChildren);
                });
            }
            else
            {
                StopMonitoring();
            }
        }

        /// <summary>
        /// Проверяет, является ли контрол валидным для мониторинга.
        /// Checks if a control is valid for monitoring.
        /// </summary>
        /// <param name="control">Контрол для проверки. The control to check.</param>
        /// <returns>True, если контрол должен отслеживаться, иначе false. True if the control should be monitored, otherwise false.</returns>
        private bool IsValidControl(Control control)
        {
            bool isValid = (_excludedTypes == null || !_excludedTypes.Any(t => t.IsInstanceOfType(control))) &&
                           (_additionalFilter == null || _additionalFilter(control));
            if (!isValid)
            {
                LogMessage?.Invoke($"Control filtered out: {control.GetType().Name}, Name={control.Name ?? "Unnamed"}");
            }
            return isValid;
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
                    if (!LogicalChildren.Contains(child))
                    {
                        LogicalChildren.Add(child);
                        ChildAdded?.Invoke(child);
                        LogMessage?.Invoke($"Added child: {child.GetType().Name}, Name={child.Name ?? "Unnamed"}");
                    }
                }

                foreach (var child in removedChildren)
                {
                    if (LogicalChildren.Contains(child))
                    {
                        LogicalChildren.Remove(child);
                        ChildRemoved?.Invoke(child);
                        LogMessage?.Invoke($"Removed child: {child.GetType().Name}, Name={child.Name ?? "Unnamed"}");
                    }
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