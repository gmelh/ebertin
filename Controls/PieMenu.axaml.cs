using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using System;

namespace Ebertin.Controls
{
    public partial class PieMenu : UserControl
    {
        public event EventHandler<string>? ItemSelected;

        public PieMenu()
        {
            InitializeComponent();
            
            // Attach event handlers programmatically
            AttachHandlers(MenuItem1);
            AttachHandlers(MenuItem2);
            AttachHandlers(MenuItem3);
            AttachHandlers(MenuItem4);
            AttachHandlers(MenuItem5);
            AttachHandlers(MenuItem6);
        }

        private void AttachHandlers(Path path)
        {
            path.PointerEntered += MenuItem_PointerEnter;
            path.PointerExited += MenuItem_PointerLeave;
            path.PointerPressed += MenuItem_PointerPressed;
        }

        private void MenuItem_PointerEnter(object? sender, PointerEventArgs e)
        {
            if (sender is Path path)
            {
                path.Fill = new SolidColorBrush(Color.Parse("#4A5A64"));
            }
        }

        private void MenuItem_PointerLeave(object? sender, PointerEventArgs e)
        {
            if (sender is Path path)
            {
                path.Fill = new SolidColorBrush(Color.Parse("#3E4C54"));
            }
        }

        private void MenuItem_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Path path)
            {
                string? itemName = path.Name switch
                {
                    "MenuItem1" => "Copy",
                    "MenuItem2" => "Paste",
                    "MenuItem3" => "Delete",
                    "MenuItem4" => "Save",
                    "MenuItem5" => "Load",
                    "MenuItem6" => "New",
                    _ => null
                };

                if (itemName != null)
                {
                    ItemSelected?.Invoke(this, itemName);
                }
            }
        }
    }
}