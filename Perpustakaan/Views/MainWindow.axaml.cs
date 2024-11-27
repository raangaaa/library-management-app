using Avalonia.Controls;
using Perpustakaan.ViewModels;
using Perpustakaan.ViewModels.Auth;
using Perpustakaan.Views.Auth;

namespace Perpustakaan.Views;

public partial class MainWindow : Window
{
    public NotificationService NotificationService { get; } 
    public static MainWindow? Instance { get; private set; }
    public MainWindow(LoginPageViewModel loginPageViewModel)
    {
        InitializeComponent();

        var loginView = new LoginPageView();

        this.Content = loginView;

        Instance = this;

        NotificationService = new NotificationService(this);
    }

    public MainWindow() : this(new LoginPageViewModel()) { }
}