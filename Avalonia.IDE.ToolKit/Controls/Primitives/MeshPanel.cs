using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Metadata;

namespace Avalonia.IDE.ToolKit.Controls.Primitives;

/// <summary>
/// Панель с визуальной сеткой и поддержкой вложенных элементов.
/// Используется в визуальных редакторах и дизайнерах UI.
/// </summary>
public class MeshPanel : VisualMesh, IChildIndexProvider
{
    private EventHandler<ChildIndexChangedEventArgs>? _childIndexChanged;

    public MeshPanel()
    {
        Children.CollectionChanged += ChildrenChanged;
    }

    /// <summary>
    /// Коллекция дочерних элементов панели.
    /// </summary>
    [Content]
    public Avalonia.Controls.Controls Children { get; } = new();

    /// <summary>
    /// Обрабатывает изменение коллекции Children и синхронизирует логическое/визуальное дерево.
    /// </summary>
    protected virtual void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                LogicalChildren.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Control>());
                VisualChildren.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Visual>());
                break;

            case NotifyCollectionChangedAction.Move:
                LogicalChildren.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                VisualChildren.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
                LogicalChildren.RemoveAll(e.OldItems!.OfType<Control>());
                VisualChildren.RemoveAll(e.OldItems!.OfType<Visual>());
                break;

            case NotifyCollectionChangedAction.Replace:
                for (int i = 0; i < e.OldItems!.Count; i++)
                {
                    var index = i + e.OldStartingIndex;
                    var child = (Control)e.NewItems![i]!;
                    LogicalChildren[index] = child;
                    VisualChildren[index] = child;
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                throw new NotSupportedException("Reset not supported.");
        }

        _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.ChildIndexesReset);
        InvalidateMeasure();
    }

    #region IChildIndexProvider

    int IChildIndexProvider.GetChildIndex(ILogical child) =>
        child is Control c ? Children.IndexOf(c) : -1;

    bool IChildIndexProvider.TryGetTotalCount(out int count)
    {
        count = Children.Count;
        return true;
    }

    event EventHandler<ChildIndexChangedEventArgs>? IChildIndexProvider.ChildIndexChanged
    {
        add
        {
            if (_childIndexChanged == null)
                Children.PropertyChanged += ChildrenPropertyChanged;

            _childIndexChanged += value;
        }
        remove
        {
            _childIndexChanged -= value;

            if (_childIndexChanged == null)
                Children.PropertyChanged -= ChildrenPropertyChanged;
        }
    }

    private void ChildrenPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Children.Count) || e.PropertyName is null)
            _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.TotalCountChanged);
    }

    #endregion
}
