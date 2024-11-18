using Avalonia.Controls;
using Perpustakaan.ViewModels;

namespace Perpustakaan.Views;

public partial class BookPageView : UserControl
{
    public BookPageView()
    {
        InitializeComponent();
        DataContext = new BookPageViewModel();
    }
}