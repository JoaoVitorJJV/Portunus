using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Portunus.App.ViewModels.Interfaces;
using System;

namespace Portunus.App.ViewModels.Dashboard;

public partial class ShareEntryViewModel : ViewModelBase
{
    [ObservableProperty]
    private Bitmap? _qrImage;

    [ObservableProperty]
    private string _itemTitle = string.Empty;

    public event Action? Cancelled;

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke();
}