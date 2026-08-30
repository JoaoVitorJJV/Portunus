using Avalonia;
using Avalonia.Controls;
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
using Portunus.Platform;
using Portunus.Platform.Interfaces;
using Portunus.Platform.MacOS;
using Portunus.Platform.Windows;
using System;
using System.IO;
using System.Linq;
using System.Threading;

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
            IAutoStartService autoStartService = provider.GetRequiredService<IAutoStartService>();
            var cts = new CancellationTokenSource();
            desktop.ShutdownRequested += (sender, e) => cts.Cancel();

            var jobService = provider.GetRequiredService<BackgroundJobService>();

            _ = jobService.StartAsync(cts.Token);

            if (!autoStartService.IsAutoStartEnabled())
                autoStartService.EnableAutoStart();

            var args = Environment.GetCommandLineArgs();
            bool startHidden = args.Contains("--hidden");

            var mainWindow = new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            };
            desktop.MainWindow = mainWindow;

            if (startHidden)
            {
                // O Avalonia vai carregar, o ciclo de vida inicia, mas a janela não abre.
                // Para não matar o app quando a janela for fechada (porque a Tray a mantém viva):
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }

            var navigation = provider.GetRequiredService<NavigationService>();
            await navigation.NavigateAsync(Domain.Enum.Screens.Auth);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void LoadServices(ServiceCollection serviceLocator)
    {
        var baseDir = AppContext.BaseDirectory;
        var keystorePath = Path.Combine(baseDir, "keystore_data");

        if (OperatingSystem.IsWindows())
        {
            serviceLocator.AddSingleton<IAutoStartService, WindowsAutoStartService>();
            serviceLocator.AddSingleton<INativeNotificationService, WindowsNotificationNative>();
            // Novos serviços do Windows
            serviceLocator.AddSingleton<IAuthVerificationService, WindowsAuthVerification>();
            serviceLocator.AddSingleton<IKeyStore>(new WindowsKeyStore(keystorePath));
        }
        else if (OperatingSystem.IsMacOS())
        {
            serviceLocator.AddSingleton<IAutoStartService, MacAutoStartService>();
            serviceLocator.AddSingleton<INativeNotificationService, MacNotificationNative>();

            serviceLocator.AddSingleton<IAuthVerificationService, MacAuthVerification>();
            serviceLocator.AddSingleton<IKeyStore, MacKeyStore>();
        }

        serviceLocator.AddSingleton<NotificationService>();
        serviceLocator.AddSingleton<INotificationService>(sp => sp.GetRequiredService<NotificationService>());
        serviceLocator.AddSingleton<NavigationService>();
        serviceLocator.AddSingleton<VaultService>();
        serviceLocator.AddSingleton<BackgroundJobService>();
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

    // =========================================================================
    // EVENTOS DO TRAY ICON
    // =========================================================================

    private void TrayIcon_OnClicked(object? sender, EventArgs e) => ShowApp();
    private void MenuOpen_OnClick(object? sender, EventArgs e) => ShowApp();

    private void MenuLock_OnClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is MainWindow window)
        {
            var vm = window.DataContext as MainViewModel;

            // Caso o comando de bloqueio esteja no MainViewModel:
            // vm?.LockVaultCommand?.Execute(null);
        }
    }

    private void MenuExit_OnClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is MainWindow window)
        {
            window.ForceClose();
        }
    }

    private void ShowApp()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            desktop.MainWindow.Show();

            if (desktop.MainWindow.WindowState == WindowState.Minimized)
            {
                desktop.MainWindow.WindowState = WindowState.Normal;
            }

            desktop.MainWindow.Activate();
        }
    }
}