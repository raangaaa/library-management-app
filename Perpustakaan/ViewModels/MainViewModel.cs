using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Perpustakaan.Models;
using Perpustakaan.Services;
using Perpustakaan.Views;
using Perpustakaan.Views.Auth;

namespace Perpustakaan.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly List<ListPageTemplate> _templates =
    [
        new ListPageTemplate(typeof(HomePageViewModel), "HomeRegular", "Dashboard"),
        new ListPageTemplate(typeof(BookPageViewModel), "BookRegular", "Data Book"),
        new ListPageTemplate(typeof(StudentPageViewModel), "PersonRegular", "Data Student"),
        new ListPageTemplate(typeof(BorrowPageViewModel), "Handshake", "Borrow Book"),
        new ListPageTemplate(typeof(ReturnPageViewModel), "BookRegular", "Return Book"),

    ];

    private readonly List<ListPageTemplate> _templatesStudent =
    [
        new ListPageTemplate(typeof(Student.BookPageViewModel), "BookRegular", "Explore Book"),
    ];


    [ObservableProperty]
    private bool _isPaneOpen = true;

    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private ListPageTemplate? _selectedListPage;

    partial void OnSelectedListPageChanged(ListPageTemplate? value)
    {
        if (value is null) return;

        var vm = Design.IsDesignMode
            ? Activator.CreateInstance(value.ModelType)
            : Ioc.Default.GetService(value.ModelType);

        if (vm is not ViewModelBase vmb) return;

        CurrentPage = vmb;

    }

    public ObservableCollection<ListPageTemplate> Items { get; }

    public MainViewModel()
    {
        if (AuthState.CurrentUser == null)
        {
            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.Content = new LoginPageView();
            }
        }

        if (AuthState.CurrentUser != null && AuthState.CurrentUser.Role.ToString() == Roles.Admin.ToString())
        {
            CurrentPage = new HomePageViewModel();
            Items = [.. _templates];
        }
        else if (AuthState.CurrentUser != null && AuthState.CurrentUser.Role.ToString() == Roles.Student.ToString())
        {
            CurrentPage = new Student.BookPageViewModel();
            Items = [.. _templatesStudent];
        }
        else {
            Items = [];
            CurrentPage = new HomePageViewModel();
        }
    }

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

}
