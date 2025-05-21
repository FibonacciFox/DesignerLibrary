using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.IDE.ToolKit.Controls.Designer.Layers;
using Avalonia.Input;

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
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        if (ItemsControl.ItemsControlFromItemContainer(this) is InteractionLayer layer)
        {
            layer.TrySelectItem(this, e);
            Focus();
            e.Handled = true;
        }
    }
}
