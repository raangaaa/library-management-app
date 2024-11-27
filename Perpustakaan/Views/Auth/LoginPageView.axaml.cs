using Avalonia.Controls;
using Perpustakaan.ViewModels.Auth;

namespace Perpustakaan.Views.Auth;

public partial class LoginPageView : UserControl
{
    public LoginPageView()
    {
        InitializeComponent();
        DataContext = new LoginPageViewModel();
    }
}   