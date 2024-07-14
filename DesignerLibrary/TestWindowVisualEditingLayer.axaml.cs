using System;
using Avalonia.Controls;
using Avalonia;
using Avalonia.IDE.ToolKit.Services;
using Avalonia.Interactivity;

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
            
            // Создаем сервис мониторинга
            var monitorService = new LogicalChildrenMonitorService(Panel1);

            // Подписываемся на события
            monitorService.ChildAdded += (control) =>
            {
                // Действия при добавлении нового контрола
                LogAdded(control);
            };

            monitorService.ChildRemoved += (control) =>
            {
                // Действия при удалении контрола
                LogRemoved(control);
            };

            // Запускаем мониторинг
            monitorService.StartMonitoring();

            // Пример остановки мониторинга по какому-либо событию или условию
            Closed += (sender, e) =>
            {
                monitorService.StopMonitoring();
                monitorService.Dispose();
            };
        }
        
        
        private void LogAdded(Control child)
        {
            Console.WriteLine($"Добавлен: {child.GetType().Name} в {child.Parent}");
        }

        private void LogRemoved(Control child)
        {
            var parentType = child.Parent?.GetType().Name ?? "неизвестный родитель";
            Console.WriteLine($"Удален: {child.GetType().Name} из {parentType}");
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var button = new Button { Content = "Новая кнопка" };
            Panel1.Children.Add(button);
        }

        private void AddTextBlock_Click(object sender, RoutedEventArgs e)
        {
            var textBlock = new TextBlock { Text = "Новый текст" };
            Panel1.Children.Add(textBlock);
        }

        private void AddCalendar_Click(object sender, RoutedEventArgs e)
        {
            var calendar = new Calendar();
            Panel1.Children.Add(calendar);
        }
        
    }
}
