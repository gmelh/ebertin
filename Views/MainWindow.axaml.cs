using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Ebertin.ViewModels;
using System;
using System.Threading.Tasks; // Add this for Task

namespace Ebertin.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize flyout as closed
            //FlyoutBorder.Width = 0;
            
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
        
        // In your MainWindow.xaml.cs

        private void LocationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                var textBox = (TextBox)sender;
                viewModel.LocationSearchText = textBox.Text;
            }
        }

        private void LocationSuggestion_Tapped(object sender, TappedEventArgs e)
        {
            if (sender is TextBlock textBlock && DataContext is MainWindowViewModel viewModel)
            {
                string selectedLocation = textBlock.Text;
                viewModel.LocationSearchText = selectedLocation;
                viewModel.IsSuggestionsVisible = false;
            }
        }
        
        private void LocationSuggestion_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && DataContext is MainWindowViewModel viewModel)
            {
                string selectedLocation = e.AddedItems[0].ToString();
                
                // Set the properties directly
                viewModel.IsSuggestionsVisible = false;
                viewModel.LocationSearchText = selectedLocation;
                
                // Clear the selection
                if (sender is ListBox listBox)
                {
                    listBox.SelectedItem = null;
                }
                
                // Force focus back to the textbox
                var textBox = this.FindControl<TextBox>("LocationTextBox");
                textBox?.Focus();
            }
        }
        
        private void LocationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                // Hide suggestions when the text box loses focus
                viewModel.IsSuggestionsVisible = false;
            }
        }
    }
}