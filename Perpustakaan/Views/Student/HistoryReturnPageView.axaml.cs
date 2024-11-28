using Avalonia.Controls;
using Perpustakaan.ViewModels.Student;

namespace Perpustakaan.Views.Student;

public partial class HistoryReturnPageView : UserControl
{
    public HistoryReturnPageView()
    {
        InitializeComponent();
        DataContext = new HistoryReturnPageViewModel();
    }
}   