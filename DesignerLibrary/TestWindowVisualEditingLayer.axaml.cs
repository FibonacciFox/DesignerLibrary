using Avalonia;
using Avalonia.Controls;

namespace DesignerLibrary
{
    public partial class TestWindowVisualEditingLayer : Window
    {
        public TestWindowVisualEditingLayer()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
            
#endif
            AddNewItem();
        }

        // Пример метода для добавления нового элемента
        private void AddNewItem()
        {
            var newButton = new Button
            {
                Content = "New Button",
                Width = 100,
                Height = 50
            };

            Panel1.Content = newButton;
        }
    }
}
