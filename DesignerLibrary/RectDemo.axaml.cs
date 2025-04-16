using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace DesignerLibrary;

public class RectDemo : TemplatedControl
{
    public Rect Rect1 { get; set; } = new Rect(50, 50, 150, 100);
    public Rect Rect2 { get; set; } = new Rect(100, 80, 100, 120);

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Синий прямоугольник - Rect1
        context.DrawRectangle(null, new Pen(Brushes.Blue, 2), Rect1);

        // Зелёный пунктир - Rect2
        context.DrawRectangle(null, new Pen(Brushes.Green, 2, dashStyle: DashStyle.Dash), Rect2);
        
    }
}