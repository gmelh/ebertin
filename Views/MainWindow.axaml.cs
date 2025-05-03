using Avalonia.Controls;
using System;
using Avalonia.Input;

namespace Ebertin.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            
            // Debug: Verify DataContext is set
            this.AttachedToVisualTree += (s, e) =>
            {
                Console.WriteLine($"DataContext type: {DataContext?.GetType().Name ?? "null"}");
            };
        }

        private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            
            // Check if it's a right click
            if (point.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonReleased)
            {
                var position = e.GetPosition(this);
                PieMenuPopup.Show(position);
                e.Handled = true;
            }
        }

        private void MainWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Close the pie menu on any left click outside of it
            var point = e.GetCurrentPoint(this);
            if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
            {
                if (PieMenuPopup.IsVisible)
                {
                    PieMenuPopup.Hide();
                }
            }
        }

        private void PieMenu_ItemSelected(object? sender, string itemName)
        {
            Console.WriteLine($"Pie menu item selected: {itemName}");
            
            // Handle the selected item
            switch (itemName)
            {
                case "Copy":
                    // Implement copy functionality
                    break;
                case "Paste":
                    // Implement paste functionality
                    break;
                case "Delete":
                    // Implement delete functionality
                    break;
                case "Save":
                    // Implement save functionality
                    break;
                case "Load":
                    // Implement load functionality
                    break;
                case "New":
                    // Implement new functionality
                    break;
            }
        }
    }
}