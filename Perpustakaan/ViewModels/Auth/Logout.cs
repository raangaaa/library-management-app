using Perpustakaan.Services;
using Perpustakaan.Views;
using Perpustakaan.Views.Auth;

namespace Perpustakaan.ViewModels.Auth;

class Logout
{
    public Logout ()
    {
        AuthState.CurrentUser = null;

        if (MainWindow.Instance != null)
        {
            MainWindow.Instance.Content = new LoginPageView();
        }
    }
}