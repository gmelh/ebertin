using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;

namespace Ebertin.Controls
{
    public partial class PieMenuPopup : UserControl
    {
        public event EventHandler<string>? ItemSelected;

        public PieMenuPopup()
        {
            InitializeComponent();
            
            PieMenuControl.ItemSelected += (s, e) =>
            {
                ItemSelected?.Invoke(this, e);
                Hide();
            };
            
            if (PieMenuPopupControl != null)
            {
                PieMenuPopupControl.Closed += (s, e) =>
                {
                    IsVisible = false;
                };
            }
        }

        public void Show(Point position)
        {
            if (PieMenuPopupControl != null)
            {
                // Set the placement target to be the parent control
                PieMenuPopupControl.PlacementTarget = this.Parent as Control;
                
                // Set placement mode to manual to control position
                PieMenuPopupControl.PlacementMode = PlacementMode.Pointer;
                
                // Center the pie menu on the click position by adjusting offsets
                var menuSize = new Size(300, 300);
                PieMenuPopupControl.HorizontalOffset = -menuSize.Width / 2;
                PieMenuPopupControl.VerticalOffset = -menuSize.Height / 2;
                
                PieMenuPopupControl.IsOpen = true;
                IsVisible = true;
            }
        }

        public void Hide()
        {
            if (PieMenuPopupControl != null)
            {
                PieMenuPopupControl.IsOpen = false;
                IsVisible = false;
            }
        }
    }
}