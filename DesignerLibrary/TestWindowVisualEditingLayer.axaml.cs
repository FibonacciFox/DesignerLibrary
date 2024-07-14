using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Threading;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace DesignerLibrary
{
    public partial class TestWindowVisualEditingLayer : Window
    {
        private ObservableCollection<Control> Children { get; } = new();
        private Timer _monitorTimer;
        
        
        public TestWindowVisualEditingLayer()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
            _monitorTimer = new Timer(MonitorTreeChanges!, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.5));

            Children.CollectionChanged += Children_CollectionChanged;
        }

        private void MonitorTreeChanges(object state)
        {
            try
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var currentDescendants = Panel1.GetLogicalDescendants().OfType<Control>().ToList();
                    UpdateChildrenCollection(currentDescendants);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при обновлении: " + ex.Message);
            }
        }

        private void UpdateChildrenCollection(List<Control> newChildren)
        {
            var newSet = new HashSet<Control>(newChildren);
            var toRemove = new HashSet<Control>(Children);
            toRemove.ExceptWith(newSet);  // Элементы для удаления

            var toAdd = new HashSet<Control>(newChildren);
            toAdd.ExceptWith(Children);  // Элементы для добавления

            foreach (Control child in toRemove)
            {
                Children.Remove(child);
            }

            foreach (Control child in toAdd)
            {
                Children.Add(child);
            }
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Control child in e.OldItems)
                {
                    LogRemoved(child);
                }
            }

            if (e.NewItems != null)
            {
                foreach (Control child in e.NewItems)
                {
                    LogAdded(child);
                    VisualLayer.AddItem(child);
                }
            }
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

        private void Control_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Control control)
            {
                Panel1.Children.Remove(control);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _monitorTimer?.Change(Timeout.Infinite, Timeout.Infinite); // Остановка таймера
            _monitorTimer?.Dispose(); // Освобождение ресурсов
        }
    }
}
