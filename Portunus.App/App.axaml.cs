using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Portunus.App.Domain.Enum;
using Portunus.App.Services;
using Portunus.App.Services.Interfaces;
using Portunus.App.ViewModels;
using Portunus.App.ViewModels.Auth;
using Portunus.App.ViewModels.Dashboard;
using Portunus.App.Views;
using System;

namespace Portunus.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();

        LoadServices(serviceCollection);
        LoadViewModels(serviceCollection);

        var provider = serviceCollection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            };
            desktop.MainWindow = mainWindow;

            var navigation = provider.GetRequiredService<NavigationService>();
            await navigation.NavigateAsync(Screens.Auth);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void LoadServices(ServiceCollection serviceLocator)
    {
        serviceLocator.AddSingleton<NotificationService>();
        serviceLocator.AddSingleton<INotificationService>(sp => sp.GetRequiredService<NotificationService>());
        serviceLocator.AddSingleton<NavigationService>();
        serviceLocator.AddSingleton<VaultService>();

    }

    private void LoadViewModels(ServiceCollection serviceLocator)
    {
        serviceLocator.AddTransient<MainViewModel>();
        serviceLocator.AddTransient<AuthViewModel>();
        serviceLocator.AddTransient<EntryEditorViewModel>();
        serviceLocator.AddSingleton<Func<EntryEditorViewModel>>(sp => () => sp.GetRequiredService<EntryEditorViewModel>());
        serviceLocator.AddTransient<CategoryEditorViewModel>();
        serviceLocator.AddSingleton<Func<CategoryEditorViewModel>>(sp => () => sp.GetRequiredService<CategoryEditorViewModel>());
        serviceLocator.AddTransient<VaultEditorViewModel>();
        serviceLocator.AddSingleton<Func<VaultEditorViewModel>>(sp => () => sp.GetRequiredService<VaultEditorViewModel>());
        serviceLocator.AddTransient<DashboardViewModel>();
        
    }
}