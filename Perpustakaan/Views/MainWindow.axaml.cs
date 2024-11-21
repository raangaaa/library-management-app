using Avalonia.Controls;
using Perpustakaan.ViewModels;

namespace Perpustakaan.Views;

public partial class MainWindow : Window
{
    public NotificationService NotificationService { get; } 
    public MainWindow(MainViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();

        NotificationService = new NotificationService(this);
    }

    public MainWindow() : this(new MainViewModel()) { }
}