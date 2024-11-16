using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

using Perpustakaan.ViewModels;
using Perpustakaan.Views;

namespace Perpustakaan;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var locator = new ViewLocator();
        DataTemplates.Add(locator);

        var services = new ServiceCollection();
        ConfigureViewModels(services);
        ConfigureViews(services);
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
        services.AddTransient<HomePageViewModel>();
        services.AddTransient<HomePageView>();
        services.AddTransient<BookPageViewModel>();
        services.AddTransient<BookPageView>();
        services.AddTransient<StudentPageViewModel>();
        services.AddTransient<StudentPageView>();


        var provider = services.BuildServiceProvider();

        Ioc.Default.ConfigureServices(provider);

        var vm = Ioc.Default.GetRequiredService<MainViewModel>();


        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) 
        {
            desktop.MainWindow = new MainWindow(vm);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView { DataContext = vm };
        }

        base.OnFrameworkInitializationCompleted();
    }

    static partial void ConfigureViewModels(IServiceCollection services);
    static partial void ConfigureViews(IServiceCollection services);

}