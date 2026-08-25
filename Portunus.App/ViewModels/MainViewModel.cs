using CommunityToolkit.Mvvm.ComponentModel;
using Portunus.App.Services;

namespace Portunus.App.ViewModels;

public partial class MainViewModel(NavigationService navigationService, NotificationService notifications) : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Avalonia!";

    public NavigationService Navigation { get;  } = navigationService;
    public NotificationService Notifications { get; } = notifications;
}
