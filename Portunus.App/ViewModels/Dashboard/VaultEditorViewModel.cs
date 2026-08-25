using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Portunus.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Portunus.App.ViewModels.Dashboard;

public partial class VaultEditorViewModel : ViewModelBase
{
    public event Action<Vault>? Saved;
    public event Action? Cancelled;

    [ObservableProperty]
    private string _name = string.Empty;

    [RelayCommand]
    private void Create()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        var vault = new Vault
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Icon = "fa-solid fa-house"
        };

        Saved?.Invoke(vault);
    }

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke();
}