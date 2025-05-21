using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.IDE.ToolKit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;

namespace DesignerLibrary;

public partial class TransformBoxDemo : Window
{
    public TransformBoxDemo()
    {
        InitializeComponent();
        
        CanvasSelectingItemsControl1.SelectionMode = SelectionMode.Multiple;
        PointerPressed += (sender, args) => CanvasSelectingItemsControl1.UnselectAll();

        DoubleTapped += (sender, args) =>
        {
            CanvasSelectingItemsControl1.SelectAll();
            
            args.Handled = true;
        };
    }
    
    public async Task TakeScreenshotAsync(Control control, Image targetImage)
    {
        // Получаем визуальный элемент для Composition API
        var compVisual = ElementComposition.GetElementVisual(control);

        // Снимаем скриншот (масштаб 1.0)
        var surface = await compVisual.Compositor.CreateCompositionVisualSnapshot(compVisual, 1.0);
        

        // Применяем к Image
        targetImage.Source = surface;
    }
    
    private void Attach(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        TransformBox1.Target = button as Button;
        
        Layout.SetX(TransformBox1, Layout.GetX(TransformBox1.Target) - TransformBox1.AnchorSize);
        Layout.SetY(TransformBox1, Layout.GetY(TransformBox1.Target) - TransformBox1.AnchorSize);
    }
    
}