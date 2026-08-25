using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Portunus.App.Services.Interfaces;
using System;

namespace Portunus.App.Services
{
    public sealed partial class NotificationService : ObservableObject, INotificationService
    {
        private readonly DispatcherTimer _timer;

        [ObservableProperty] private bool _isVisible;
        [ObservableProperty] private string? _message;
        [ObservableProperty] private bool _isError;
        [ObservableProperty] private string? _title;

        public NotificationService()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += (_, _) => { _timer.Stop(); IsVisible = false; };
        }


        public void Success(string title, string message) => Show(title, message, isError: false, seconds: 4);
        public void Error(string title, string message) => Show(title, message, isError: true, seconds: 6);

        private void Show(string title, string message, bool isError, int seconds)
        {
            Title = title;
            Message = message;
            IsError = isError;
            IsVisible = true;

            _timer.Stop();
            _timer.Interval = TimeSpan.FromSeconds(seconds);
            _timer.Start();
        }

    }
}
