using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Portunus.App.ViewModels.Dashboard;

public partial class ConfirmActionViewModel(string title, string message, string confirmText = "Sim", string cancelText = "Não") : ViewModelBase
{
    public string Title { get; } = title;
    public string Message { get; } = message;
    public string ConfirmText { get; } = confirmText;
    public string CancelText { get; } = cancelText;

    public event Action? Confirmed;
    public event Action? Cancelled;

    [RelayCommand] private void Confirm() => Confirmed?.Invoke();
    [RelayCommand] private void Cancel() => Cancelled?.Invoke();
}