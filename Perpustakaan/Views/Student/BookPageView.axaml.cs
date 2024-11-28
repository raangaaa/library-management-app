using Avalonia.Controls;

namespace Perpustakaan.Views.Student;

public partial class BookPageView : UserControl
{
    public BookPageView()
    {
        InitializeComponent();
        DataContext = new ViewModels.Student.BookPageViewModel();
    }
}   