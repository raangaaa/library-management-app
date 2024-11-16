using Avalonia.Controls;
using Perpustakaan.ViewModels;

namespace Perpustakaan.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }

    public MainWindow() : this(new MainViewModel()) { }
}