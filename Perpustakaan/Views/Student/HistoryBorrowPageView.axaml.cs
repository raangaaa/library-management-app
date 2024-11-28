using Avalonia.Controls;
using Perpustakaan.ViewModels.Student;

namespace Perpustakaan.Views.Student;

public partial class HistoryBorrowPageView : UserControl
{
    public HistoryBorrowPageView()
    {
        InitializeComponent();
        DataContext = new HistoryBorrowPageViewModel();
    }
}   