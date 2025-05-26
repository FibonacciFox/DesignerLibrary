using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.IDE.ToolKit.Controls.Designer.Layers;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalonia.IDE.ToolKit.Controls.Designer.Items;

/**
 * Поддерживает выделение, псевдокласс :selected, IsSelectedProperty, и взаимодействие с InteractionLayer
 */
[PseudoClasses(":selected", ":pressed")]
public class SelectableInteractionLayerItem : InteractionLayerBaseItem, ISelectable
{
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<SelectableInteractionLayerItem>();

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    static SelectableInteractionLayerItem()
    {
        SelectableMixin.Attach<SelectableInteractionLayerItem>(IsSelectedProperty);
        PressedMixin.Attach<SelectableInteractionLayerItem>();
        FocusableProperty.OverrideDefaultValue<SelectableInteractionLayerItem>(true);
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        Console.WriteLine("OnPointerPressed");
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        if (ItemsControl.ItemsControlFromItemContainer(this) is InteractionLayer interactionLayer)
        {
            interactionLayer.TrySelectItem(this, e);
            Focus(); // необязательно, но полезно
            e.Handled = true;
        }
    }

}

