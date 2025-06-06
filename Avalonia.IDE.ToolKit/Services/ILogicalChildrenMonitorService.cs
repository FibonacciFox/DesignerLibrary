using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace Avalonia.IDE.ToolKit.Services
{
    /// <summary>
    /// Defines a service that monitors changes to the logical children of a control in an Avalonia application.
    /// </summary>
    public interface ILogicalChildrenMonitorService : IDisposable
    {
        /// <summary>
        /// Starts monitoring changes to the logical children.
        /// </summary>
        void StartMonitoring();

        /// <summary>
        /// Stops monitoring changes to the logical children.
        /// </summary>
        void StopMonitoring();

        /// <summary>
        /// Occurs when a new child control is added to the monitored collection.
        /// </summary>
        event Action<Control> ChildAdded;

        /// <summary>
        /// Occurs when a child control is removed from the monitored collection.
        /// </summary>
        event Action<Control> ChildRemoved;

        /// <summary>
        /// Occurs when a log message is generated about monitoring actions.
        /// </summary>
        event Action<string> LogMessage;

        /// <summary>
        /// Gets the collection of logical children being monitored.
        /// </summary>
        ObservableCollection<Control> LogicalChildren { get; }
    }
}