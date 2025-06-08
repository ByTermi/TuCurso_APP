using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TuCurso.Services;

namespace TuCurso.Views;

public partial class RegisterPage : ContentPage, INotifyPropertyChanged
{
    private string _nombre;
    private string _email;
    private string _password;
    private string _descripcion;
    private string _icono;
    private bool _isBusy;
    private bool _isErrorVisible;
    private string _errorMessage;
    private bool _isSuccessVisible;
    private string _successMessage;

    private readonly AuthService _authService;

    public string Nombre
    {
        get => _nombre;
        set
        {
            if (_nombre != value)
            {
                _nombre = value;
                OnPropertyChanged();
            }
        }
    }

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

    public string Descripcion
    {
        get => _descripcion;
        set
        {
            if (_descripcion != value)
            {
                _descripcion = value;
                OnPropertyChanged();
            }
        }
    }

    public string Icono
    {
        get => _icono;
        set
        {
            if (_icono != value)
            {
                _icono = value;
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

    public bool IsSuccessVisible
    {
        get => _isSuccessVisible;
        set
        {
            if (_isSuccessVisible != value)
            {
                _isSuccessVisible = value;
                OnPropertyChanged();
            }
        }
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set
        {
            if (_successMessage != value)
            {
                _successMessage = value;
                OnPropertyChanged();
                IsSuccessVisible = !string.IsNullOrEmpty(value);
            }
        }
    }

    public ICommand RegisterCommand { get; }
    public ICommand NavigateToLoginCommand { get; }

    public RegisterPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        RegisterCommand = new Command(async () => await RegisterAsync());
        NavigateToLoginCommand = new Command(async () => await NavigateToLoginAsync());

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

    private async Task RegisterAsync()
    {
        // Validación básica
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            ErrorMessage = "Por favor, ingresa tu nombre";
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Por favor, ingresa tu correo electrónico";
            return;
        }

        if (!IsValidEmail(Email))
        {
            ErrorMessage = "Por favor, ingresa un correo electrónico válido";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor, ingresa una contraseña";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var registerResult = await _authService.RegisterAsync(Nombre, Email, Password, Descripcion, Icono);

            if (registerResult.Success)
            {
                SuccessMessage = "¡Registro exitoso! Ya puedes iniciar sesión.";

                // Limpiar campos
                Nombre = string.Empty;
                Email = string.Empty;
                Password = string.Empty;
                Descripcion = string.Empty;
                Icono = string.Empty;

                // Opcional: navegar automáticamente a la página de inicio de sesión después de un breve retraso
                await Task.Delay(2000);
                await NavigateToLoginAsync();
            }
            else
            {
                ErrorMessage = registerResult.ErrorMessage ?? "Error al registrar usuario";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task NavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync("..");  // Volver a la página anterior (Login)
    }

    public new event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}