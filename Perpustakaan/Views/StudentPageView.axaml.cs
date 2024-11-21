using Avalonia.Controls;
using Perpustakaan.ViewModels;

namespace Perpustakaan.Views;

public partial class StudentPageView : UserControl
{
    public StudentPageView()
    {
        InitializeComponent();
        DataContext = new StudentPageViewModel();
    }
}