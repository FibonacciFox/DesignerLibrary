// DesignLayerModel.cs
using Avalonia;
using Avalonia.Controls;

namespace Avalonia.IDE.ToolKit.Controls.Designer;

/// <summary>
/// Представляет связующую модель между редактируемым элементом и визуальным слоем.
/// Управляет DesignX/Y и размерами, синхронизирует состояние между VisualEditingItem и AttachedControl.
/// </summary>
public class DesignLayerModel
{
    public Control AttachedControl { get; }
    public VisualEditingItem EditingItem { get; }

    public double DesignX { get; set; }
    public double DesignY { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public DesignLayerModel(Control control, VisualEditingItem editingItem)
    {
        AttachedControl = control;
        EditingItem = editingItem;
        ApplyFromControl();
    }

    /// <summary>
    /// Копирует значения из AttachedControl в модель.
    /// </summary>
    public void ApplyFromControl()
    {
        DesignX = Layout.GetDesignX(AttachedControl);
        DesignY = Layout.GetDesignY(AttachedControl);
        Width = AttachedControl.Width;
        Height = AttachedControl.Height;
    }

    /// <summary>
    /// Копирует значения из модели в AttachedControl и VisualEditingItem.
    /// </summary>
    public void ApplyToVisuals()
    {
        Layout.SetDesignX(AttachedControl, DesignX);
        Layout.SetDesignY(AttachedControl, DesignY);
        AttachedControl.Width = Width;
        AttachedControl.Height = Height;

        Layout.SetDesignX(EditingItem, DesignX - EditingItem.AnchorSize);
        Layout.SetDesignY(EditingItem, DesignY - EditingItem.AnchorSize);
        EditingItem.Width = Width + EditingItem.AnchorSize * 2;
        EditingItem.Height = Height + EditingItem.AnchorSize * 2;
    }

    /// <summary>
    /// Обновляет модель на основе EditingItem (например, после перемещения).
    /// </summary>
    public void ApplyFromEditingItem()
    {
        DesignX = Layout.GetDesignX(EditingItem) + EditingItem.AnchorSize;
        DesignY = Layout.GetDesignY(EditingItem) + EditingItem.AnchorSize;
        Width = EditingItem.Width - EditingItem.AnchorSize * 2;
        Height = EditingItem.Height - EditingItem.AnchorSize * 2;
    }

    public void UpdateFromControlLayout() => ApplyFromControl();
    public void UpdateFromEditingItem() => ApplyFromEditingItem();
    public void PushToControls() => ApplyToVisuals();
}
