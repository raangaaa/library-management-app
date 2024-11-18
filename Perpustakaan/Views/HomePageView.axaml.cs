using Avalonia.Controls;

namespace Perpustakaan.Views;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
    }

        private void CreateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            FormPopup.IsOpen = true;
        }

        // Event handler untuk tombol Cancel
        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            FormPopup.IsOpen = false;
        }

        // Event handler untuk tombol Submit

}   