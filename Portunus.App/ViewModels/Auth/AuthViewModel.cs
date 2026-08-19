using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Portunus.App.ViewModels.Auth;

public enum AuthScreen { Welcome, Master, Pin }

public partial class AuthViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWelcome), nameof(IsMaster), nameof(IsPin))]
    private AuthScreen _screen = AuthScreen.Welcome;

    public bool IsWelcome => Screen == AuthScreen.Welcome;
    public bool IsMaster  => Screen == AuthScreen.Master;
    public bool IsPin     => Screen == AuthScreen.Pin;

    [ObservableProperty] private string _vaultName = "Pessoal";
    [ObservableProperty] private bool   _isRevealed;

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

    [RelayCommand] private void ToggleReveal() => IsRevealed = !IsRevealed;
    [RelayCommand] private void Create() { /* TODO: VaultSession.CreateNew(...) */ }
    [RelayCommand] private void Unlock() { /* TODO: VaultSession.TryUnlock(...) */ }

    [RelayCommand] private void ShowWelcome() => Screen = AuthScreen.Welcome;
    [RelayCommand] private void ShowMaster()  => Screen = AuthScreen.Master;
    [RelayCommand] private void ShowPin()     => Screen = AuthScreen.Pin;

    [RelayCommand] private void PinPress(string digit) { /* TODO */ }
    [RelayCommand] private void PinDelete() { /* TODO */ }
    [RelayCommand] private void Biometric() { /* TODO */ }
}