using System.ComponentModel;
using System.Windows.Input;
using TuCurso.Models;
using TuCurso.Services;

namespace TuCurso.Views;

public partial class CrearCursoPage : ContentPage, INotifyPropertyChanged
{
    private readonly CursoService _cursoService;
    private readonly AuthTokenService _authTokenService;
    private bool _isBusy;
    private string _nombre = string.Empty;
    private string _enlace = string.Empty;
    private double _precio;
    private bool _finalizado;
    private string _anotaciones = string.Empty;
    private bool _isErrorVisible;
    private string _errorMessage = string.Empty;

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    public bool IsNotBusy => !IsBusy;

    public string Nombre
    {
        get => _nombre;
        set
        {
            _nombre = value;
            OnPropertyChanged();
        }
    }

    public string Enlace
    {
        get => _enlace;
        set
        {
            _enlace = value;
            OnPropertyChanged();
        }
    }

    public double Precio
    {
        get => _precio;
        set
        {
            _precio = value;
            OnPropertyChanged();
        }
    }

    public bool Finalizado
    {
        get => _finalizado;
        set
        {
            _finalizado = value;
            OnPropertyChanged();
        }
    }

    public string Anotaciones
    {
        get => _anotaciones;
        set
        {
            _anotaciones = value;
            OnPropertyChanged();
        }
    }

    public bool IsErrorVisible
    {
        get => _isErrorVisible;
        set
        {
            _isErrorVisible = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
            IsErrorVisible = !string.IsNullOrEmpty(value);
        }
    }

    public ICommand GuardarCommand { get; }
    public ICommand CancelarCommand { get; }

    public CrearCursoPage(CursoService cursoService, AuthTokenService authTokenService)
    {
        InitializeComponent();
        _cursoService = cursoService;
        _authTokenService = authTokenService;

        GuardarCommand = new Command(async () => await GuardarCursoAsync());
        CancelarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

        BindingContext = this;
    }

    private async Task GuardarCursoAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            ErrorMessage = "El nombre del curso es obligatorio";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var curso = new CursoDTO
            {
                Nombre = Nombre,
                Enlace = Enlace,
                Precio = Precio,
                Finalizado = Finalizado,
                Anotaciones = Anotaciones,
                UsuarioId = long.Parse(_authTokenService.GetUserId())
            };

            var result = await _cursoService.CrearCursoAsync(curso);
            if (result)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = "No se pudo crear el curso";
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
}
