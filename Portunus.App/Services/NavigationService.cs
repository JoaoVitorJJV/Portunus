using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Portunus.App.ViewModels;
using Portunus.App.ViewModels.Auth;
using Portunus.App.ViewModels.Dashboard;
using Portunus.App.ViewModels.Interfaces;
using Portunus.App.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Portunus.App.Services
{
    public partial class NavigationService : ObservableObject
    {
        private readonly IServiceProvider _services;
        private readonly Dictionary<Screens, Type> _map = new()
        {
            [Screens.Auth] = typeof(AuthViewModel),
            [Screens.Dashboard] = typeof(DashboardViewModel),
        };

        [ObservableProperty] private object? _currentPage;

        public NavigationService(IServiceProvider services) => _services = services;

        public async Task NavigateAsync(Screens screen, object? parameters = null)
        {
            var page = _services.GetRequiredService(_map[screen]);
            if (page is IInitializable init)
                await init.InitializeAsync(parameters);  
            CurrentPage = page;
        }
    }
}
