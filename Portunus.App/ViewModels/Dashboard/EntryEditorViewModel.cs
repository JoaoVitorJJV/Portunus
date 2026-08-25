using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Portunus.App.Services;
using Portunus.App.ViewModels.DTO.Dashboard;
using Portunus.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Portunus.App.ViewModels.Dashboard
{
    public partial class EntryEditorViewModel(NotificationService notifications) : ViewModelBase
    {

        #region Properties
        private readonly NotificationService _notifications = notifications;
        private Guid? _editingId;
        #endregion

        #region Observable Properties
        [ObservableProperty]
        private string _modalTitle = "Nova senha";

        [ObservableProperty]
        private string _primaryButtonText = "Criar senha";
        [ObservableProperty]
        private DateTime? _expirationDate;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPassword))]
        [NotifyPropertyChangedFor(nameof(StrengthLabel))]
        [NotifyPropertyChangedFor(nameof(StrengthBrush))]
        [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _url = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RevealIcon))]
        private bool _isPasswordRevealed;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Initial))]
        [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _selectedBadgeColor = "#64748b";

        [ObservableProperty]
        private ObservableCollection<CategoriesDashboard> _availableCategories = [];

        [ObservableProperty]
        private ObservableCollection<VaultsDashboard> _avaliableVaults = [];

        [ObservableProperty]
        private CategoriesDashboard? _selectedCategory;

        [ObservableProperty]
        private VaultsDashboard? _selectedVault;

        [ObservableProperty]
        private ObservableCollection<RecoveryCodeEntry> _recoveryCodes = new();

        [ObservableProperty]
        private string _description = string.Empty;
        #endregion

        public bool HasPassword => !string.IsNullOrEmpty(Password);

        public string RevealIcon => IsPasswordRevealed
            ? "fa-solid fa-eye-slash"
            : "fa-solid fa-eye";

        public string StrengthLabel => Strength switch
        {
            0 => "Muito fraca",
            1 => "Fraca",
            2 => "Razoável",
            3 => "Boa",
            _ => "Excelente",
        };

        public IBrush StrengthBrush => Strength switch
        {
            >= 4 => Brush.Parse("#4cc48d"), // ok
            3 => Brush.Parse("#d0a765"), // brass
            2 => Brush.Parse("#d9a24c"), // warn
            _ => Brush.Parse("#e0707f"), // bad
        };

        private int Strength => ScoreOf(Password);

        public event Action<PasswordEntry>? Saved;
        public event Action? Cancelled;
        public string Initial =>
            string.IsNullOrWhiteSpace(Title) ? "?" : Title.Trim()[..1].ToUpperInvariant();


        #region Page Commands
        [RelayCommand]
        private void AddRecoveryCode()
        {
            RecoveryCodes.Add(new RecoveryCodeEntry { Code = string.Empty });
        }

        [RelayCommand]
        private void RemoveRecoveryCode(RecoveryCodeEntry codeToRemove)
        {
            if (codeToRemove != null)
            {
                RecoveryCodes.Remove(codeToRemove);
            }
        }
        [RelayCommand]
        private void SelectBadgeColor(string hex)
        {
            SelectedBadgeColor = hex;
        }

        [RelayCommand]
        private void ToggleReveal() => IsPasswordRevealed = !IsPasswordRevealed;

        [RelayCommand]
        private void Generate()
        {
            const string pool = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%&*?";
            var rng = new Random();
            Password = new string(Enumerable.Range(0, 16)
                .Select(_ => pool[rng.Next(pool.Length)]).ToArray());
            IsPasswordRevealed = true; 
        }

        private bool CanCreate =>
            !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrEmpty(Password);

        [RelayCommand(CanExecute = nameof(CanCreate))]
        private void Create()
        {
            var (IsValid, ErrorMessage) = ValidateEntry();

            if (!IsValid)
            {
                _notifications.Error("Ops!", ErrorMessage);
                return;
            }


            PasswordEntry entry = new()
            {
                Id = _editingId ?? Guid.NewGuid(), 
                Name = Title,
                Description = Description,
                VaultId = SelectedVault!.Id,
                CategoryId = SelectedCategory?.Id,
                RecoveryCodes = [.. RecoveryCodes],
                Notes = Notes,
                Password = Password,
                Username = Username,
                DateToChangePass = ExpirationDate,
                FavColor = SelectedBadgeColor,
                Url = Url
            };

            Saved?.Invoke(entry);
        }

        [RelayCommand]
        private void Cancel() => Cancelled?.Invoke();

        private static int ScoreOf(string p)
        {
            if (string.IsNullOrEmpty(p)) return 0;
            int s = 0;
            if (p.Length >= 8) s++;
            if (p.Length >= 14) s++;
            if (p.Any(char.IsUpper) && p.Any(char.IsLower)) s++;
            if (p.Any(char.IsDigit) && p.Any(c => !char.IsLetterOrDigit(c))) s++;
            return Math.Min(s, 4);
        }
        #endregion

        #region Private Methods
        public void LoadEntry(PasswordEntry entryToEdit)
        {
            _editingId = entryToEdit.Id;
            Title = entryToEdit.Name;
            Description = entryToEdit.Description ?? string.Empty;
            Notes = entryToEdit.Notes ?? string.Empty;
            Password = entryToEdit.Password;
            Username = entryToEdit.Username ?? string.Empty;
            Url = entryToEdit.Url ?? string.Empty;
            SelectedBadgeColor = entryToEdit.FavColor;

            if (entryToEdit.DateToChangePass.HasValue)
                ExpirationDate = entryToEdit.DateToChangePass.Value;

            RecoveryCodes.Clear();
            foreach (var rc in entryToEdit.RecoveryCodes)
            {
                RecoveryCodes.Add(new RecoveryCodeEntry
                {
                    Code = rc.Code,
                    DateCreated = rc.DateCreated,
                    DateExpiration = rc.DateExpiration,
                    DateUsed = rc.DateUsed
                });
            }

            SelectedCategory = AvailableCategories.FirstOrDefault(c => c.Id == entryToEdit.CategoryId);
            SelectedVault = AvaliableVaults.FirstOrDefault(v => v.Id == entryToEdit.VaultId);

            ModalTitle = "Editar senha";
            PrimaryButtonText = "Salvar alterações";
        }
        public void LoadCategories(IEnumerable<CategoriesDashboard> categories)
        {
            AvailableCategories.Clear();
            foreach (var category in categories)
            {
                AvailableCategories.Add(category);
            }
        }

        public void LoadVault(IEnumerable<VaultsDashboard> vaults)
        {
            AvaliableVaults.Clear();
            foreach (var vault in vaults)
            {
                AvaliableVaults.Add(vault);
            }
        }

        private (bool IsValid, string ErrorMessage) ValidateEntry()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return (false, "O título da senha é obrigatório.");

            if (SelectedVault is null || SelectedVault.Id == Guid.Empty)
                return (false, "Você precisa selecionar um cofre de destino.");

            if (string.IsNullOrEmpty(Password))
                return (false, "A senha não pode ficar em branco.");

            if (string.IsNullOrWhiteSpace(SelectedBadgeColor))
                return (false, "Uma cor de ícone deve ser selecionada.");

            return (true, string.Empty);
        }
        #endregion
    }
}
