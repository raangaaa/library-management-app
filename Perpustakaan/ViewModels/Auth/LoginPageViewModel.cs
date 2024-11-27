using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Perpustakaan.Models;
using Perpustakaan.Services;
using Perpustakaan.Views;

namespace Perpustakaan.ViewModels.Auth;

public partial class LoginPageViewModel : ViewModelBase 
{
    [ObservableProperty]
    private string? _username;
    [ObservableProperty]
    private string? _password;

    public ObservableCollection<string> Errors { get; } = [];

    public IAsyncRelayCommand LoginCommand { get; }

    public LoginPageViewModel ()
    {
        LoginCommand = new AsyncRelayCommand(Login);
    }

    private async Task Login ()
    {
        Errors.Clear();

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            Errors.Add("Username and Password cannot be empty.");
            return;
        }

        try
        {
            var result = await AuthService.AuthenticateAsync(Username, Password);

            if (result != null)
            {
                AuthState.CurrentUser = result;
                if (MainWindow.Instance != null)
                {
                    MainWindow.Instance.Content = new MainView();
                }
            }
            else
            {
                Errors.Add("Username or Password doesn't matches!");
            }
        }
        catch (Exception ex)
        {
            Errors.Add($"Error: {ex.Message}");
        }

    }

}
