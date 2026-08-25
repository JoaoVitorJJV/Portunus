using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Portunus.App.Domain.Enum;
using Portunus.App.Services;
using Portunus.App.Services.Interfaces;
using Portunus.App.ViewModels.Dashboard;
using Portunus.App.ViewModels.DTO;
using Portunus.App.ViewModels.Interfaces;
using Portunus.Core.DTO;
using Portunus.Core.Vault;
using System;
using System.Threading.Tasks;

namespace Portunus.App.ViewModels.Auth;

public partial class AuthViewModel(
    VaultService vaultService, 
    NavigationService navService,
    INotificationService notifications
    ) : ViewModelBase, IInitializable
{

    #region Services
    private readonly VaultService _vaultService = vaultService;
    private readonly NavigationService _navService = navService;
    private readonly INotificationService _notifications = notifications;
    #endregion

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWelcome), nameof(IsMaster), nameof(IsPin))]

    private AuthScreen _screen = AuthScreen.Welcome;

    public bool IsWelcome => Screen == AuthScreen.Welcome;
    public bool IsMaster  => Screen == AuthScreen.Master;
    public bool IsPin     => Screen == AuthScreen.Pin;

    [ObservableProperty] private string _vaultName = "Pessoal";
    [ObservableProperty] private bool   _isRevealed;
    [ObservableProperty] private object? _activeModal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreate), nameof(HasPassword), nameof(StrengthLabel))]
    private string _password = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreate))]
    private string _passwordConfirm = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreate))]
    private bool _acknowledged;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanUnlock))]
    private string _masterPassword = "";

    [ObservableProperty] private bool _hasError;

    public bool   HasPassword   => Password.Length > 0;
    public bool   CanCreate     => Password.Length >= 8 && Password == PasswordConfirm && Acknowledged;
    public bool   CanUnlock     => MasterPassword.Length > 0;
    public string StrengthLabel => Password.Length >= 14 ? "Excelente"
                                 : Password.Length >= 10 ? "Boa"
                                 : Password.Length >= 8  ? "Razoável" : "Fraca";

    public Task InitializeAsync(object? parameters)
    {

        if (parameters is AuthViewParameters authViewParameters) { 

            if (authViewParameters.ScreenInitial.HasValue)
            {
                Screen = authViewParameters.ScreenInitial.Value;
            }

            return Task.CompletedTask;
        }

        bool isVaultCreated = _vaultService.IsVaultsCreated();
        Screen = isVaultCreated ? AuthScreen.Master : AuthScreen.Welcome;

        return Task.CompletedTask;
    }

    [RelayCommand] private void ToggleReveal() => IsRevealed = !IsRevealed;

    [RelayCommand]
    private void CloseModal() => ActiveModal = null;
    [RelayCommand] private async Task Create() {
        CreateVaultDTO createVaultDto = new()
        {
            MasterPassword = Password,
            VaultName = VaultName
        };

        try
        {
            _vaultService.CreateVault(createVaultDto);
            _notifications.Success("Boa!", "O cofre foi criado com sucesso.");
            await _navService.NavigateAsync(Domain.Enum.Screens.Dashboard);
        }
        catch(Exception ex)
        {
            #if DEBUG
                _notifications.Error("Ops!", ex.Message);
            #else
                _notifications.Success("Ops!", "Não foi possível criar o cofre neste momento.");
            #endif
        }

    }
    [RelayCommand]
    private async Task Unlock()
    {
        const string errorMsg = "Não foi possível destrancar o seu cofre. A senha pode estar incorreta ou o arquivo corrompido.";

        try
        {
            bool isUnlocked = await Task.Run(() => _vaultService.UnlockVault(MasterPassword));

            if (isUnlocked)
                await _navService.NavigateAsync(Domain.Enum.Screens.Dashboard);
            else
                _notifications.Error("Ops!", errorMsg);
        }
        catch (Exception)
        {
            _notifications.Error("Ops!", errorMsg);
        }
    }

    [RelayCommand]
    private void WipeVault()
    {
        var confirmModal = new ConfirmActionViewModel(
            "Apagar TODO o Cofre?",
            "ATENÇÃO: Você está prestes a excluir permanentemente este cofre e todas as suas senhas. Esta ação não pode ser desfeita, nem mesmo por nós. Tem certeza absoluta?",
            "Sim, Apagar Tudo",
            "Cancelar"
        );

        confirmModal.Confirmed += () =>
        {
            try
            {
                 _vaultService.DestroyVault();

                // 3. Limpa os campos da tela
                MasterPassword = "";
                Password = "";
                PasswordConfirm = "";
                VaultName = "Pessoal";
                Acknowledged = false;

                // 4. Volta a tela para o modo "Boas-vindas"
                Screen = AuthScreen.Welcome;

                _notifications.Success("Cofre Apagado", "O cofre foi removido com sucesso deste dispositivo.");
            }
            catch (Exception)
            {
                _notifications.Error("Erro", "Não foi possível apagar o arquivo do cofre. Ele pode estar em uso.");
            }
            finally
            {
                CloseModal();
            }
        };

        confirmModal.Cancelled += CloseModal;
        ActiveModal = confirmModal;
    }

    [RelayCommand]
    private async Task ImportVault()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            if (topLevel == null) return;

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

                try
                {
                    VaultStorage.ImportVault(importPath, VaultLocation.DefaultPath);

                    _notifications.Success("Importado", "Cofres substituídos com sucesso. Digite sua nova senha-mestra.");
                    Screen = AuthScreen.Master;
                }
                catch (Exception)
                {
                    _notifications.Error("Erro!", "Não foi possível importar o arquivo do cofre.");
                }
            }
        }
    }

    [RelayCommand] private void ShowWelcome() => Screen = AuthScreen.Welcome;
    [RelayCommand] private void ShowMaster()  => Screen = AuthScreen.Master;
    [RelayCommand] private void ShowPin()     => Screen = AuthScreen.Pin;

    [RelayCommand] private void PinPress(string digit) { /* TODO */ }
    [RelayCommand] private void PinDelete() { /* TODO */ }
    [RelayCommand] private void Biometric() { /* TODO */ }
}