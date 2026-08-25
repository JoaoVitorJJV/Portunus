using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Portunus.App.Domain.Enum;
using Portunus.App.Services;
using Portunus.App.Util;
using Portunus.App.ViewModels.DTO;
using Portunus.App.ViewModels.DTO.Dashboard;
using Portunus.App.ViewModels.Interfaces;
using Portunus.Core.Models;
using Portunus.Core.Vault;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Portunus.App.ViewModels.Dashboard;

public partial class DashboardViewModel(
    VaultService vaultSession, 
    Func<EntryEditorViewModel> editorFactory,
    Func<CategoryEditorViewModel> categoryModalFactory,
    NotificationService notificationService,
    Func<VaultEditorViewModel> vaultModalFactory,
    NavigationService navigationService
    ) : ViewModelBase, IInitializable
{
    #region Properties
    public string AppVersionDisplay { get; } = $"Portunus v{AppUtils.ResolveVersion()}";
    public string SentinelaScore { get; } = "82";
    public string SentinelaLabel { get; } = "Sentinela · Boa";
    public string SentinelaDetail { get; } = "2 fracas · 1 reutilizada";
    public string VaultCountLabel { get; } = "24 itens · destravado";
    public bool HasItems => Items.Count > 0;
    public bool HasSelection => Detail is not null;

    #region Modais
    private readonly Func<EntryEditorViewModel> _editorFactory = editorFactory;
    private readonly Func<CategoryEditorViewModel> _categoryModalFactory = categoryModalFactory;
    private readonly Func<VaultEditorViewModel> _vaultModalFactory = vaultModalFactory;
    #endregion

    #region Services

    private readonly VaultService _vaultService = vaultSession;
    private readonly NotificationService _notifications = notificationService;
    private readonly NavigationService _navigationService = navigationService;
    #endregion

    #endregion

    #region Observables Properties
    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty] private object? _activeModal;
    public ObservableCollection<VaultsDashboard> UserVaults { get; } = [];
    public ObservableCollection<CategoriesDashboard> DashboardCategories { get; } = [];
    public bool HasCategories => DashboardCategories.Count > 0;

    [ObservableProperty]
    private DashboardItemDetail? _detail;

    private VaultsDashboard? VaultSelected;
    private CategoriesDashboard? CategorySelected;
    [ObservableProperty] private int _allItems;
    [ObservableProperty] private int _favoritedEntryCount;
    [ObservableProperty] private bool _isAllItemsSelected = true;
    [ObservableProperty] private bool _isFavoritesSelected;
    [ObservableProperty] private string _currentPageTitle = "Todos os itens";
    [ObservableProperty] private string _currentPageIcon = "fa-solid fa-table-cells-large";
    [ObservableProperty] private string _searchWatermark = "Buscar em Todos os itens…";

    #endregion

    public ObservableCollection<DashboardItem> Items { get; } = [];
   

    public async Task InitializeAsync(object? parameters)
    {
        
        SetCategories();
        SetVaults(true);
        await SetItems();

    }

    #region Page Commands

    [RelayCommand]
    private async Task SelectAllItems()
    {
        ToggleSelecteSection("Todos os itens", "fa-solid fa-table-cells-large", "Buscar em Todos os itens…");
        IsAllItemsSelected = true;
        IsFavoritesSelected = false;
        CategorySelected = null;
        VaultSelected = null;
        foreach (var v in UserVaults) v.IsSelected = false;

        await SetItems();
    }

    [RelayCommand]
    private async Task SelectFavoritesItems()
    {
        ToggleSelecteSection("Favoritos", "fa-solid fa-star", "Buscar em Favoritos…");
        IsAllItemsSelected = false;
        IsFavoritesSelected = true;
        CategorySelected = null;
        VaultSelected = null;
        foreach (var v in UserVaults) v.IsSelected = false;

        await SetItems();
    }

    [RelayCommand]
    private void NewVault()
    {
        var modal = _vaultModalFactory();
        modal.Saved += OnNewVaultSaved;
        modal.Cancelled += CloseModal;
        ActiveModal = modal;
    }
    private void OnNewVaultSaved(Vault vault)
    {
        try
        {
            _vaultService.SaveVault(vault);

            UserVaults.Clear();
            SetVaults(false);

            _notifications.Success("Boa!", "Novo cofre criado com sucesso.");
            CloseModal();
        }
        catch (Exception)
        {
            _notifications.Error("Ops!", "Não foi possível criar esse cofre.");
        }
    }

    [RelayCommand]
    private void DeleteVault(VaultsDashboard vaultDash)
    {
        var confirmModal = new ConfirmActionViewModel(
            "Excluir Cofre?",
            $"ATENÇÃO: Você está prestes a excluir o cofre '{vaultDash.VaultName}' e TODAS as senhas contidas nele. Essa ação é irreversível. Deseja continuar?",
            "Excluir Tudo",
            "Cancelar"
        );

        confirmModal.Confirmed += async () =>
        {
            try
            {
                _vaultService.DeleteVault(vaultDash.Id);

                UserVaults.Remove(vaultDash);
                await SetItems();

                if (Detail != null && Detail.VaultName == vaultDash.VaultName)
                {
                    Detail = null;
                    OnPropertyChanged(nameof(HasSelection));
                }

                _notifications.Success("Excluído", "Cofre e senhas removidos permanentemente.");

                CloseModal();

                if(UserVaults.Count == 0)
                {
                    _notifications.Success("Boa!", "Seu cofre foi deletado com sucesso.");
                    await _navigationService.NavigateAsync(Domain.Enum.Screens.Auth, new AuthViewParameters { ScreenInitial = AuthScreen.Welcome});
                    return;
                }
            }
            catch(Exception ex)
            {
                _notifications.Error("Ops!", "Não foi possível apagar esse cofre.");
                CloseModal();
            }
         
        };

        confirmModal.Cancelled += CloseModal;
        ActiveModal = confirmModal;
    }

    [RelayCommand]
    private void NewCategory()
    {
        var modal = _categoryModalFactory();

        modal.Saved += OnNewCategorySaved;
        modal.Cancelled += CloseModal;

        ActiveModal = modal;
    }

    private void OnNewCategorySaved(Category? category)
    {
        if(category == null)
        {
            _notifications.Error("Ops!", "Não foi possível criar essa categoria.");
        }

        try
        {
            Category? newCategory = _vaultService.CreateNewCategory(category!);

            if (newCategory == null) {
                
            }

            CategoriesDashboard categoryDash = new()
            {
                Id = newCategory!.Id,
                Name = newCategory.Name,
                Badge = newCategory.BadgeColor ?? "#64748b"
            };

            DashboardCategories.Add(categoryDash);
            OnPropertyChanged(nameof(HasCategories));
            _notifications.Success("Boa", "Categoria adicionada com sucesso!");
        }
        catch(Exception)
        {
            _notifications.Error("Ops!", "Não foi possível criar essa categoria.");
            CloseModal();
        }

        CloseModal();
    }

    [RelayCommand]
    private void Select(DashboardItem item)
    {
        foreach (var i in Items)
            i.IsSelected = ReferenceEquals(i, item);

        var entry = item.Entry;

        CategoriesDashboard? category = DashboardCategories.FirstOrDefault(d => d.Id == item.Entry.CategoryId);

        string? vaultName = UserVaults.FirstOrDefault(v => v.Id == item.Entry.VaultId)?.VaultName;
        int unusedCodes = entry.RecoveryCodes.Count(c => !c.DateUsed.HasValue);

        var newDetail = new DashboardItemDetail
        {
            Title = entry.Name,
            ActualPassword = entry.Password,
            Initial = entry.Name[0].ToString().ToUpper(),
            FavColor = entry.FavColor,

            Username = string.IsNullOrWhiteSpace(entry.Username) ? "N/A" : entry.Username,
            Url = string.IsNullOrWhiteSpace(entry.Url) ? "N/A" : entry.Url,
            Notes = string.IsNullOrWhiteSpace(entry.Notes) ? "N/A" : entry.Notes,

            Created = entry.DateCreated.ToString("dd/MM/yyyy"),
            Modified = entry.DateUpdated?.ToString("dd/MM/yyyy") ?? "Nunca",
            Category = category?.Name ?? "Sem categoria",
            CategoryColor = category?.Badge ?? "#64748b",
            VaultName = vaultName ?? "N/A",

            StrengthLabel = "Boa",
            OtpCode = "N/A",

            HasRecoveryCodes = entry.RecoveryCodes.Count > 0,
            RecoveryTotal = entry.RecoveryCodes.Count > 0
                 ? $"{entry.RecoveryCodes.Count} códigos restantes"
                 : "Nenhum código",
            RecoveryFooter = entry.RecoveryCodes.Count > 0
             ? $"Restam {unusedCodes} de {entry.RecoveryCodes.Count} códigos não utilizados."
             : "Nenhum código."
        };

        for (int idx = 0; idx < entry.RecoveryCodes.Count; idx++)
        {
            var code = entry.RecoveryCodes[idx];
            newDetail.RecoveryCodes.Add(new RecoveryCode
            {
                Code = code.Code,
                Number = (idx + 1).ToString("D2"),
                IsUsed = code.DateUsed.HasValue
            });
        }
        Detail = newDetail;

        OnPropertyChanged(nameof(HasSelection));
    }

    [RelayCommand]
    private async Task CopyCode(RecoveryCode rc) // Agora recebe o objeto inteiro
    {
        if (rc == null) return;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var clipboard = desktop.MainWindow?.Clipboard;
            if (clipboard != null)
            {
                _notifications.Success("Boa", "Código copiado pra sua área de transferência.");
                await clipboard.SetTextAsync(rc.Code);
            }
        }

        if (!rc.IsUsed)
        {
            var confirmModal = new ConfirmActionViewModel(
                "Código Utilizado?",
                "Deseja marcar esse código de recuperação como utilizado?"
            );

            confirmModal.Confirmed += () =>
            {
                rc.IsUsed = true;

                var selectedItem = Items.FirstOrDefault(i => i.IsSelected);
                if (selectedItem != null)
                {
                    var actualCode = selectedItem.Entry.RecoveryCodes.FirstOrDefault(c => c.Code == rc.Code);
                    if (actualCode != null) actualCode.DateUsed = DateTime.UtcNow;
                    _vaultService.CreateUpdatePassword(selectedItem.Entry);
                    int remaining = selectedItem.Entry.RecoveryCodes.Count(c => !c.DateUsed.HasValue);
                    int total = selectedItem.Entry.RecoveryCodes.Count;
                    Detail?.RecoveryFooter = $"Restam {remaining} de {total} códigos não utilizados.";
                }

                _notifications.Success("Atualizado", "Código marcado como utilizado.");
                CloseModal();
            };

            confirmModal.Cancelled += CloseModal;
            ActiveModal = confirmModal; // Exibe o modal na tela
        }
        else
        {
            _notifications.Success("Copiado!", "Código copiado.");
        }
    }
    [RelayCommand] private void Copy(string what) { }

    [RelayCommand]
    private void RevealPassword()
    {
        if (Detail is null) return;

        if (Detail.PasswordDisplay == "•••••••••••")
        {
            Detail.PasswordDisplay = Detail.ActualPassword;
            Detail.RevealIcon = "fa-solid fa-eye-slash";
        }
        else 
        {
            Detail.PasswordDisplay = "•••••••••••";
            Detail.RevealIcon = "fa-solid fa-eye";
        }
    }

    [RelayCommand]
    private void ToggleRecovery()
    {
        if (Detail is null || !Detail.HasRecoveryCodes) return;

        bool isRevealed = Detail.RecoveryToggleLabel == "Ocultar";

        if (isRevealed)
        {
            Detail.RecoveryToggleLabel = "Mostrar";
            foreach (var rc in Detail.RecoveryCodes)
                rc.Display = "••••-••••";
        }
        else
        {
            Detail.RecoveryToggleLabel = "Ocultar";
            foreach (var rc in Detail.RecoveryCodes)
                rc.Display = rc.Code;
        }
    }

    [RelayCommand]
    private async Task Lock()
    {
        _vaultService.DisposeVault();
        _notifications.Success("Cofre trancado", "Seu cofre foi bloqueado com segurança.");

        await _navigationService.NavigateAsync(Domain.Enum.Screens.Auth, new AuthViewParameters { ScreenInitial = AuthScreen.Master });
    }

    [RelayCommand]
    private void Edit()
    {
        if (Detail == null)
            return;

        var selectedItem = Items.FirstOrDefault(i => i.IsSelected);
        if (selectedItem == null)
            return;

        PasswordEntry entryToEdit = selectedItem.Entry;

        var editor = _editorFactory();

        editor.LoadCategories(DashboardCategories);
        editor.LoadVault(UserVaults);
        editor.LoadEntry(entryToEdit);

        editor.Saved += OnEntryUpdated;
        editor.Cancelled += CloseModal;

        ActiveModal = editor;
    }
    [RelayCommand] private void OpenUrl() { }

    [RelayCommand]
    private void NewItem()
    {
        var editor = _editorFactory();
        editor.LoadCategories(DashboardCategories);

        editor.LoadVault(UserVaults);

        editor.Saved += OnEntrySaved;
        editor.Cancelled += CloseModal;
        ActiveModal = editor;         
    }

    [RelayCommand]
    private void DeleteCategory(CategoriesDashboard category)
    {
        var confirmModal = new ConfirmActionViewModel(
            "Excluir Categoria?",
            $"Tem certeza que deseja excluir a categoria '{category.Name}'? Essa ação não pode ser desfeita.",
            "Excluir",
            "Cancelar"
        );

        confirmModal.Confirmed += () =>
        {
            _vaultService.DeleteCategory(category.Id);

            DashboardCategories.Remove(category);
            OnPropertyChanged(nameof(HasCategories));
            _notifications.Success("Boa!", "A categoria foi removida.");
            CloseModal();
        };

        confirmModal.Cancelled += CloseModal;
        ActiveModal = confirmModal;
    }

    [RelayCommand]
    private void DeleteItem(DashboardItem item)
    {
        var confirmModal = new ConfirmActionViewModel(
            "Excluir Senha?",
            $"Tem certeza que deseja excluir a senha '{item.Title}'? Essa ação não pode ser desfeita.",
            "Excluir",
            "Cancelar"
        );

        confirmModal.Confirmed += () =>
        {
            _vaultService.DeleteEntry(item.Entry.Id);

            Items.Remove(item);
            UpdateCounters();
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(AllItems));
            OnPropertyChanged(nameof(FavoritedEntryCount));

            if (Detail != null && Detail.Title == item.Title)
            {
                Detail = null;
                OnPropertyChanged(nameof(HasSelection));
            }

            _notifications.Success("Boa!", "A senha foi removida do cofre.");
            CloseModal();
        };

        confirmModal.Cancelled += CloseModal;
        ActiveModal = confirmModal;
    }

    [RelayCommand]
    private void ToggleFavorite(DashboardItem item)
    {
        item.IsFavorite = !item.IsFavorite;

        item.Entry.IsFavorite = item.IsFavorite;

        _vaultService.CreateUpdatePassword(item.Entry);

        FavoritedEntryCount = _vaultService.GetPasswordEntry()?.Count(p => p.IsFavorite) ?? 0;

        if (item.IsFavorite)
            _notifications.Success("Favoritos", $"'{item.Title}' foi adicionada aos favoritos.");
    }

    partial void OnSearchQueryChanged(string? value)
    {
        _ = SetItems();
    }

    [RelayCommand]
    private async Task ToggleVault(VaultsDashboard vault)
    {
        IsAllItemsSelected = false;
        IsFavoritesSelected = false;
        CategorySelected = null;
        foreach (var v in UserVaults)
            v.IsSelected = (v.Id == vault.Id);

        VaultSelected = vault;
        ToggleSelecteSection(vault.VaultName, vault.Icon, $"Buscar em {vault.VaultName}…");
        OnPropertyChanged(nameof(VaultSelected));

        await SetItems();
    }

    [RelayCommand]
    private async Task ImportVault()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            if (topLevel == null) return;

            // 2. Abre a janela para o usuário escolher o arquivo
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Importar Cofre Portunus",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Arquivo de Cofre") { Patterns = ["*.portunus", "*.vault"] },
                    FilePickerFileTypes.All
                ]
            });

            if (files.Count >= 1)
            {
                string importPath = files[0].Path.LocalPath;

                var confirmModal = new ConfirmActionViewModel(
                    "Substituir Cofres?",
                    "Atenção: Ao importar este arquivo, TODOS os seus cofres e senhas atuais serão apagados e substituídos. O aplicativo será bloqueado. Deseja continuar?",
                    "Sim, Substituir",
                    "Cancelar"
                );

                confirmModal.Confirmed += async () =>
                {
                    CloseModal();
                    try
                    {
                        _vaultService.DisposeVault();

                        VaultStorage.ImportVault(importPath, VaultLocation.DefaultPath);

                        _notifications.Success("Importado", "Cofres substituídos com sucesso. Digite sua nova senha-mestra.");
                        await _navigationService.NavigateAsync(Domain.Enum.Screens.Auth, AuthScreen.Master);
                    }
                    catch (Exception)
                    {
                        _notifications.Error("Erro!", "Não foi possível substituir o arquivo do cofre.");
                        CloseModal();
                    }
                };

                confirmModal.Cancelled += CloseModal;
                ActiveModal = confirmModal;
            }
        }
    }

    public void UpdateCounters()
    {
        var passwords = _vaultService.GetPasswordEntry() ?? [];

        foreach (var vault in UserVaults)
        {
            vault.Count = passwords.Count(p => p.VaultId == vault.Id);
        }

        foreach (var category in DashboardCategories)
        {
            category.Count = passwords.Count(p => p.CategoryId == category.Id);
        }
    }
    private async void OnEntrySaved(PasswordEntry entry)
    {

        try
        {
            PasswordEntry? passCreated = _vaultService.CreateUpdatePassword(entry);
            if (passCreated == null)
                _notifications.Error("Ops!", "Não foi possível criar a sua senha.");

            await SetItems();
            UpdateCounters();

            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(AllItems));
            _notifications.Success("Senha criada com sucesso!", entry.Name);
            CloseModal();
        }
        catch(Exception) {
            _notifications.Error("Ops!", "Não foi possível criar a sua senha.");
            CloseModal();
        }

      
    }

    private async void OnEntryUpdated(PasswordEntry entry)
    {
        try
        {
            PasswordEntry? passCreated = _vaultService.CreateUpdatePassword(entry);
            if (passCreated == null)
                _notifications.Error("Ops!", "Não foi possível editar a sua senha.");

            await SetItems();
            UpdateCounters();
            OnPropertyChanged(nameof(HasItems));

            var updatedItem = Items.FirstOrDefault(i => i.Id == entry.Id);
            if (updatedItem != null)
            {
                Select(updatedItem); 
            }

            _notifications.Success("Senha editada com sucesso!", entry.Name);
            CloseModal();
        }
        catch (Exception)
        {
            _notifications.Error("Ops!", "Não foi possível editar a sua senha.");
            CloseModal();
        }


    }

    private void CloseModal()
    {
        if (ActiveModal is null) return;

        if (ActiveModal is EntryEditorViewModel entryEditor)
        {
            entryEditor.Saved -= OnEntrySaved;
            entryEditor.Cancelled -= CloseModal;
        }

        else if (ActiveModal is CategoryEditorViewModel categoryEditor)
        {
            categoryEditor.Saved -= OnNewCategorySaved;
            categoryEditor.Cancelled -= CloseModal;
        }

        else if (ActiveModal is ShareEntryViewModel shareModal)
        {
            shareModal.Cancelled -= CloseModal;
        }

        ActiveModal = null;
    }
    #endregion

    #region Private Methods
    public async Task SetItems()
    {
        IReadOnlyList<PasswordEntry> passwords = [];
        await Task.Run(() => passwords = _vaultService.GetPasswordEntry() ?? []);
        Items.Clear();

        AllItems = passwords.Count;
        FavoritedEntryCount = passwords.Count(p => p.IsFavorite);

        if (VaultSelected is not null)
        {
            passwords = [.. passwords.Where(p => p.VaultId == VaultSelected.Id)];
        }

        if (IsFavoritesSelected)
        {
            passwords = [.. passwords.Where(p => p.IsFavorite == true)];
        }

        if(CategorySelected is not null)
        {
            passwords = [.. passwords.Where(p => p.CategoryId == CategorySelected.Id)];
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            // Ignora maiúsculas e minúsculas
            string query = SearchQuery.ToLowerInvariant();

            passwords = [.. passwords.Where(p =>
            p.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase) ||
            p.Username.Contains(query, StringComparison.InvariantCultureIgnoreCase) ||
            p.Url.Contains(query, StringComparison.InvariantCultureIgnoreCase)
        )];
        }

        var groupedPasswords = passwords
            .GroupBy(p => p.CategoryId)
            .Select(grupo => new
            {
                CategoryId = grupo.Key,
                CategoryName = DashboardCategories.FirstOrDefault(c => c.Id == grupo.Key)?.Name ?? "Sem categoria",
                Entries = grupo.OrderBy(e => e.Name).ToList()
            })
            .OrderBy(g => g.CategoryName == "Sem categoria" ? 1 : 0)
            .ThenBy(g => g.CategoryName);

        foreach (var group in groupedPasswords)
        {
            bool isFirstOfCategory = true;

            foreach (var item in group.Entries)
            {
                var itemDash = new DashboardItem
                {
                    Id = item.Id,
                    Title = item.Name,
                    Subtitle = string.IsNullOrWhiteSpace(item.Username) ? "N/A" : item.Username,
                    Initial = string.IsNullOrWhiteSpace(item.Name) ? "?" : item.Name[0].ToString().ToUpper(),
                    FavColor = item.FavColor,
                    GroupHeader = group.CategoryName.ToUpper(),
                    ShowGroupHeader = isFirstOfCategory,
                    Entry = item,
                    IsFavorite = item.IsFavorite,
                    // Mantém a senha selecionada visualmente caso ela passe no filtro da pesquisa
                    IsSelected = Detail != null && Detail.Title == item.Name
                };

                Items.Add(itemDash);
                isFirstOfCategory = false;
            }
        }

        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(AllItems));
        OnPropertyChanged(nameof(FavoritedEntryCount));
    }

    [RelayCommand]
    public async Task SelectCategory(CategoriesDashboard category)
    {
        IsAllItemsSelected = false;
        IsFavoritesSelected = false;
        VaultSelected = null;
        foreach (var v in UserVaults) v.IsSelected = false;

        CategorySelected = category;

        ToggleSelecteSection(category.Name, "fa-solid fa-folder", $"Buscar em {category.Name}…");

        await SetItems();
    }

    [RelayCommand]
    private void Share()
    {
        if (Detail == null) return;

        var textToShare = $"Site: {Detail.Url}\nUsuário: {Detail.Username}\nSenha: {Detail.ActualPassword}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(textToShare, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeBytes = qrCode.GetGraphic(10);

        using var ms = new MemoryStream(qrCodeBytes);
        var bitmap = new Bitmap(ms);
        var shareModal = new ShareEntryViewModel
        {
            ItemTitle = Detail.Title,
            QrImage = bitmap
        };

        shareModal.Cancelled += CloseModal;
        ActiveModal = shareModal;
    }

    public void SetVaults(bool isInitialization)
    {
        IReadOnlyList<Vault> vaults = [];
        vaults = _vaultService.GetVaults() ?? [];
        var passwords = _vaultService.GetPasswordEntry();

        if (VaultSelected == null && isInitialization && vaults.Count > 0)
        {
            var dashItem = new VaultsDashboard
            {
                Id = vaults[0].Id,
                VaultName = vaults[0].Name,
                Icon = vaults[0].Icon ?? "fa-solid fa-house",
                Count = passwords?.Count(p => p.VaultId == vaults[0].Id) ?? 0,
                IsSelected = VaultSelected?.Id == vaults[0].Id
            };
            VaultSelected = dashItem;
        }

        foreach (var item in vaults)
        {
            var itemDash = new VaultsDashboard
            {
                Id = item.Id,
                VaultName = item.Name,
                Icon = item.Icon ?? "fa-solid fa-house",
                Count = passwords?.Count(p => p.VaultId == item.Id) ?? 0,
                IsSelected = VaultSelected?.Id == item.Id
            };

            UserVaults.Add(itemDash);
        }
    }

    public void SetCategories()
    {
        IReadOnlyList<Category> categories = [];
        categories = _vaultService.GetCategories() ?? [];

        var passwords = _vaultService.GetPasswordEntry() ?? [];

        foreach (var item in categories)
        {
            var itemDash = new CategoriesDashboard
            {
                Id = item.Id,
                Name = item.Name,
                Badge = item.BadgeColor,
                Count = passwords.Count(p => p.CategoryId == item.Id)
            };

            DashboardCategories.Add(itemDash);
        }
    }

    private void ToggleSelecteSection(string pagleTitle, string icon, string waterMark)
    {
        CurrentPageTitle = pagleTitle;
        CurrentPageIcon = icon ?? "fa-solid fa-house";
        SearchWatermark = waterMark;
    }
    #endregion
}

public partial class DashboardItem : ViewModelBase
{
    public required PasswordEntry Entry { get; init; }
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string Initial { get; init; }
    public required string FavColor { get; init; }
    public string? GroupHeader { get; init; }
    public bool ShowGroupHeader { get; init; }

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavoriteIcon))]
    private bool _isFavorite;

    public string FavoriteIcon => IsFavorite ? "fa-solid fa-star" : "fa-regular fa-star";
}

public partial class DashboardItemDetail : ViewModelBase
{
    public required string ActualPassword { get; init; }
    public bool HasRecoveryCodes { get; set; } 

    public required string Title { get; init; }
    public required string Initial { get; init; }
    public required string FavColor { get; init; }
    public required string Category { get; init; }
    public required string CategoryColor { get; init; }
    public string VaultName { get; init; } = "N/A";
    public required string Username { get; init; }
    public required string StrengthLabel { get; init; }
    public required string OtpCode { get; init; }
    public required string Url { get; init; }
    public required string Notes { get; init; }
    public required string Created { get; init; }
    public required string Modified { get; init; }
    public required string RecoveryTotal { get; init; }
    [ObservableProperty]
    private string _recoveryFooter = string.Empty;
    public ObservableCollection<RecoveryCode> RecoveryCodes { get; } = new();

    [ObservableProperty]
    private string _passwordDisplay = "•••••••••••";

    [ObservableProperty]
    private string _revealIcon = "fa-solid fa-eye";

    [ObservableProperty]
    private string _recoveryToggleLabel = "Mostrar";
}

public partial class RecoveryCode : ViewModelBase
{
    public required string Code { get; init; }
    public required string Number { get; init; }

    [ObservableProperty]
    private bool _isUsed;

    [ObservableProperty]
    private string _display = "••••‑••••";
}