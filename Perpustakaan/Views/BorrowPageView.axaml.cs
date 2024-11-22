using Avalonia.Controls;
using Perpustakaan.ViewModels;

namespace Perpustakaan.Views;

public partial class BorrowPageView : UserControl
{
    public BorrowPageView()
    {
        InitializeComponent();
        DataContext = new BorrowPageViewModel();
    }
}