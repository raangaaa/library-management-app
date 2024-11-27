using Avalonia.Controls;
using Perpustakaan.ViewModels;

namespace Perpustakaan.Views;

public partial class ReturnPageView : UserControl
{
    public ReturnPageView()
    {
        InitializeComponent();
        DataContext = new ReturnPageViewModel();
    }
}   