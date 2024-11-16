using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Perpustakaan.Models;

namespace Perpustakaan.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    private readonly List<ListPageTemplate> _templates =
    [
        new ListPageTemplate(typeof(HomePageViewModel), "HomeRegular", "Home"),
        new ListPageTemplate(typeof(BookPageViewModel), "BookRegular", "Data Book"),
        new ListPageTemplate(typeof(StudentPageViewModel), "PersonRegular", "Data Student"),
    ];

    [ObservableProperty]
    private bool _isPaneOpen;

    [ObservableProperty]
    private ViewModelBase _currentPage = new BookPageViewModel();

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
        Items = [.. _templates];
    }

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

}
