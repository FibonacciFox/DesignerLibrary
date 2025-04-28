using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace Avalonia.IDE.ToolKit.Controls.Designer;

/// <summary>
/// Контрол, который принимает детей через XAML и рендерит их в панель.
/// </summary>
public class UiDesigner : TemplatedControl
{
    private Canvas? _panel;
    private VisualEditingLayer? _editingLayer;

    public VisualEditingLayer EditingLayer => _editingLayer!;

    /// <summary>
    /// Коллекция детей.
    /// </summary>
    [Content]
    public Avalonia.Controls.Controls Children { get; } = new();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _panel = e.NameScope.Find<Canvas>("PART_Workspace");
        _editingLayer = e.NameScope.Find<VisualEditingLayer>("PART_EditingLayer");

        if (_panel != null)
        {
            _panel.Children.Clear();
            foreach (var child in Children)
            {
                _panel.Children.Add(child);
            }

            Children.CollectionChanged += (s, ev) =>
            {
                if (_panel == null)
                    return;

                // Простая синхронизация
                switch (ev.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        foreach (var item in ev.NewItems!)
                            _panel.Children.Add((Control)item!);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        foreach (var item in ev.OldItems!)
                            _panel.Children.Remove((Control)item!);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        _panel.Children.Clear();
                        break;
                }
            };
        }
    }
}