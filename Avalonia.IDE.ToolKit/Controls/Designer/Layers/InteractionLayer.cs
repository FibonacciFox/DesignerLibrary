using System.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.IDE.ToolKit.Controls.Designer.Items;
using Avalonia.Input;

namespace Avalonia.IDE.ToolKit.Controls.Designer.Layers;

public class InteractionLayer : SelectingItemsControl
{
    
    /// <summary>
    /// The default value for the <see cref="ItemsControl.ItemsPanel"/> property.
    /// </summary>
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new Canvas());
    
    /// <summary>
    /// Defines the <see cref="SelectedItems"/> property.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1010",
        Justification = "This property is owned by SelectingItemsControl, but protected there. InteractionLayer changes its visibility.")]
    public new static readonly DirectProperty<SelectingItemsControl, IList?> SelectedItemsProperty =
        SelectingItemsControl.SelectedItemsProperty;

    /// <summary>
    /// Defines the <see cref="Selection"/> property.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1010",
        Justification = "This property is owned by SelectingItemsControl, but protected there. InteractionLayer changes its visibility.")]
    public new static readonly DirectProperty<SelectingItemsControl, ISelectionModel> SelectionProperty =
        SelectingItemsControl.SelectionProperty;

    /// <summary>
    /// Defines the <see cref="SelectionMode"/> property.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1010",
        Justification = "This property is owned by SelectingItemsControl, but protected there. InteractionLayer changes its visibility.")]
    public new static readonly StyledProperty<SelectionMode> SelectionModeProperty =
        SelectingItemsControl.SelectionModeProperty;

    static InteractionLayer()
    {
        ItemsPanelProperty.OverrideDefaultValue<InteractionLayer>(DefaultPanel);
        SelectionModeProperty.OverrideDefaultValue<InteractionLayer>(SelectionMode.Multiple);
        
        KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue(
            typeof(InteractionLayer),
            KeyboardNavigationMode.Once);
    }
    
    /// <inheritdoc/>
    public new IList? SelectedItems
    {
        get => base.SelectedItems;
        set => base.SelectedItems = value;
    }

    /// <inheritdoc/>
    public new ISelectionModel Selection
    {
        get => base.Selection;
        set => base.Selection = value;
    }

    /// <summary>
    /// Gets or sets the selection mode.
    /// </summary>
    /// <remarks>
    /// Note that the selection mode only applies to selections made via user interaction.
    /// Multiple selections can be made programmatically regardless of the value of this property.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1012",
        Justification = "This property is owned by SelectingItemsControl, but protected there. InteractionLayer changes its visibility.")]
    public new SelectionMode SelectionMode
    {
        get => base.SelectionMode;
        set => base.SelectionMode = value;
    }

    /// <summary>
    /// Selects all items in the <see cref="ListBox"/>.
    /// </summary>
    public void SelectAll() => Selection.SelectAll();

    /// <summary>
    /// Deselects all items in the <see cref="ListBox"/>.
    /// </summary>
    public void UnselectAll() => Selection.Clear();

    
    
    internal void TrySelectItem(Control item, PointerEventArgs e)
    {
        Console.WriteLine($"TrySelectItem: {item}, Selection.Count: {Selection.Count}");

        var toggle = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var range = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var right = e.GetCurrentPoint(item).Properties.IsRightButtonPressed;

        var changed = UpdateSelectionFromEventSource(item, true, false, range, right);

       var  item1 = item as TransformBox;

        Console.WriteLine($"Updated: {changed} Button:{item1.Target.Name} IsSelected: {(item1 as ISelectable)?.IsSelected}");
    }

    
}