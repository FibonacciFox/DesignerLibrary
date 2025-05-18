using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Avalonia.IDE.ToolKit.Controls.Designer;

public class CanvasSelectingItemsControl : SelectingItemsControl
{
    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var item in GetRealizedContainers())
        {
            if (item is Control control)
            {
                double x = Layout.GetX(control);
                double y = Layout.GetY(control);
                control.Arrange(new Rect(new Point(x, y), control.DesiredSize));
            }
        }

        return finalSize;
    }
}