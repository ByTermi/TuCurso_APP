using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TuCurso.Services;
using TuCurso.Models;

namespace TuCurso.Views;

public partial class LoginPage : ContentPage, INotifyPropertyChanged
{
    private string _email = string.Empty;
    private string _password = string.Empty;
    private bool _isBusy;
    private bool _isErrorVisible;
    private string _errorMessage = string.Empty;

    private readonly AuthService _authService;
    private readonly AuthTokenService _authTokenService;

    public string Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                _email = value;
                OnPropertyChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsErrorVisible
    {
        get => _isErrorVisible;
        set
        {
            if (_isErrorVisible != value)
            {
                _isErrorVisible = value;
                OnPropertyChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
                IsErrorVisible = !string.IsNullOrEmpty(value);
            }
        }
    }

    public ICommand LoginCommand { get; }
    public ICommand NavigateToRegisterCommand { get; }

    public LoginPage(AuthService authService, AuthTokenService authTokenService)
    {
        InitializeComponent();
        _authService = authService;
        _authTokenService = authTokenService;

        LoginCommand = new Command(async () => await LoginAsync());
        NavigateToRegisterCommand = new Command(async () => await NavigateToRegisterAsync());

        BindingContext = this;
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Patrón para validar email
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
    }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor, complete todos los campos";
            return;
        }

        if (!IsValidEmail(Email))
        {
            ErrorMessage = "Por favor, ingrese un correo electrónico válido";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var loginResult = await _authService.LoginAsync(Email, Password);

            if (loginResult.Success)
            {
                _authTokenService.Token = loginResult.Token;
                _authTokenService.Expiration = loginResult.GetExpirationAsDateTime();
                _authTokenService.UserId = loginResult.Id;

                // Establecer el estado de autenticación
                _authTokenService.SetAuthentication(true);

                // Limpiar campos
                Email = string.Empty;
                Password = string.Empty;

                // La navegación se manejará automáticamente por el AppShell
                // gracias al evento AuthenticationChanged
            }
            else
            {
                ErrorMessage = loginResult.ErrorMessage ?? "Error al iniciar sesión";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error durante el login: {ex}");
            ErrorMessage = "Error al conectar con el servidor";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task NavigateToRegisterAsync()
    {
        Email = string.Empty;
        Password = string.Empty;
        ErrorMessage = string.Empty;
        await Shell.Current.GoToAsync("register");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Limpiar campos cuando la página aparece
        Email = string.Empty;
        Password = string.Empty;
        ErrorMessage = string.Empty;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}