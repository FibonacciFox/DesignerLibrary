using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Input;

namespace Avalonia.IDE.ToolKit.Controls.Designer;

public class CanvasSelectingItemsControl : SelectingItemsControl
{
    internal void TrySelectItem(Control item, PointerEventArgs e)
    {
        var toggle = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var range = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var right = e.GetCurrentPoint(item).Properties.IsRightButtonPressed;

        UpdateSelectionFromEventSource(item, true, false, range, right);
        
    }

    /// <summary>
    /// Defines the <see cref="P:Avalonia.IDE.ToolKit.Controls.Designer.CanvasSelectingItemsControl.Selection" /> property.
    /// </summary>
    public new static readonly DirectProperty<SelectingItemsControl, ISelectionModel> SelectionProperty =
        SelectingItemsControl.SelectionProperty;
    
    /// <summary>
    /// Defines the <see cref="P:Avalonia.Controls.ListBox.SelectionMode" /> property.
    /// </summary>
    public new static readonly StyledProperty<SelectionMode> SelectionModeProperty = SelectingItemsControl.SelectionModeProperty;


    public new ISelectionModel Selection
    {
        get => base.Selection;
        set => base.Selection = value;
    }
    
    /// <summary>Gets or sets the selection mode.</summary>
    /// <remarks>
    /// Note that the selection mode only applies to selections made via user interaction.
    /// Multiple selections can be made programmatically regardless of the value of this property.
    /// </remarks>
    public new SelectionMode SelectionMode
    {
        get => base.SelectionMode;
        set => base.SelectionMode = value;
    }

    /// <summary>
    /// Selects all items in the <see cref="T:Avalonia.Controls.ListBox" />.
    /// </summary>
    public void SelectAll() => this.Selection.SelectAll();

    /// <summary>
    /// Deselects all items in the <see cref="T:Avalonia.Controls.ListBox" />.
    /// </summary>
    public void UnselectAll() => this.Selection.Clear();
    
}