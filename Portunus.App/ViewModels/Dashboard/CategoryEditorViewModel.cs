using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Portunus.App.Services;
using Portunus.Core.Models;
using System;
using System.Xml.Linq;

namespace Portunus.App.ViewModels.Dashboard;

public partial class CategoryEditorViewModel(NotificationService notifications) : ViewModelBase
{
    public event Action<Category>? Saved;
    public event Action? Cancelled;
    public NotificationService _notifications = notifications;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Initial))] 
    private string _name = string.Empty;

    [ObservableProperty]
    private string _selectedBadgeColor = "#64748b"; 

    public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name[0].ToString().ToUpper();

    // Comandos
    [RelayCommand]
    private void SelectBadgeColor(string colorHex)
    {
        SelectedBadgeColor = colorHex;
    }

    [RelayCommand]
    private void Create()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            _notifications.Error("Ops", "Você precisa preencher o nome da categoria.");
            return;
        };

        var category = new Category
        {
            Name = Name,
            BadgeColor = SelectedBadgeColor
        };

        Saved?.Invoke(category);
    }

    [RelayCommand]
    private void Cancel()
    {
        Cancelled?.Invoke();
    }
}