using Microcharts;
using SkiaSharp;
using System.ComponentModel;
using System.Windows.Input;
using TuCurso.Models;
using TuCurso.Services;
using Microcharts;
using SkiaSharp;
using Entry = Microcharts.ChartEntry;

namespace TuCurso;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private readonly CursoService _cursoService;
    private readonly AuthTokenService _authTokenService;
    private bool _isBusy;
    private List<CursoDTO> _cursos;
    private string _userId;

    public ICommand VerDetallesCommand { get; private set; }
    public ICommand AgregarCursoCommand { get; private set; }
    public ICommand EliminarCursoCommand { get; private set; }


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

    public List<CursoDTO> Cursos
    {
        get => _cursos;
        set
        {
            if (_cursos != value)
            {
                _cursos = value;
                OnPropertyChanged();
            }
        }
    }

    public string UserId
    {
        get => _userId;
        set
        {
            if (_userId != value)
            {
                _userId = value;
                OnPropertyChanged();
            }
        }
    }

    private Chart _progresoChart;

    public Chart ProgresoChart
    {
        get => _progresoChart;
        set
        {
            _progresoChart = value;
            OnPropertyChanged();
        }
    }

    private void ActualizarGrafico()
    {
        if (Cursos == null || !Cursos.Any())
        {
            ProgresoChart = null;
            return;
        }

        int totalCursos = Cursos.Count;
        int cursosFinalizados = Cursos.Count(c => c.Finalizado);
        int cursosPendientes = totalCursos - cursosFinalizados;

        var entries = new[]
        {
            new Entry(cursosFinalizados)
            {
                Label = "Finalizados",
                ValueLabel = $"{(float)cursosFinalizados/totalCursos:P0}",
                Color = SKColor.Parse("#2ecc71")
            },
            new Entry(cursosPendientes)
            {
                Label = "Pendientes",
                ValueLabel = $"{(float)cursosPendientes/totalCursos:P0}",
                Color = SKColor.Parse("#e74c3c")
            }
        };

        ProgresoChart = new DonutChart
        {
            Entries = entries,
            BackgroundColor = SKColors.Transparent,
            LabelTextSize = 30,
            HoleRadius = 0.6f
        };
    }

    public ICommand RefreshCommand { get; }

    public MainPage(CursoService cursoService, AuthTokenService authTokenService)
    {
        InitializeComponent();
        _cursoService = cursoService;
        _authTokenService = authTokenService;
        _cursos = new List<CursoDTO>();
        _userId = _authTokenService.GetUserId();

        RefreshCommand = new Command(async () => await RefreshCursosAsync());

        VerDetallesCommand = new Command<long>(async (id) => await NavigateToDetailAsync(id));

        AgregarCursoCommand = new Command(async () =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Intentando navegar a CrearCursoPage...");
                await Shell.Current.GoToAsync("CrearCursoPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al navegar: {ex.Message}");
                await DisplayAlert("Error", "No se pudo abrir la página de crear curso", "OK");
            }
        });

        EliminarCursoCommand = new Command<long>(async (id) => await EliminarCursoAsync(id));


        ActualizarGrafico();

        BindingContext = this;

        // Cargar cursos al iniciar
        MainThread.BeginInvokeOnMainThread(async () => await LoadCursosAsync());

        

        

    }

    private async Task NavigateToDetailAsync(long id)
    {
        var navigationParameter = new Dictionary<string, object>
    {
        { "id", id }
    };
        await Shell.Current.GoToAsync("cursos", navigationParameter);
    }

    private async Task LoadCursosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            if (string.IsNullOrEmpty(UserId))
            {
                await DisplayAlert("Error", "No se ha identificado el usuario", "OK");
                return;
            }

            Cursos = await _cursoService.ObtenerCursosPorUsuarioAsync(UserId);
            ActualizarGrafico(); 
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudieron cargar los cursos", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshCursosAsync()
    {
        await LoadCursosAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Actualizar el ID de usuario por si ha cambiado
        UserId = _authTokenService.GetUserId();

        // Recargar cursos al volver a la página
        await LoadCursosAsync();
    }

    private async Task EliminarCursoAsync(long id)
    {
        bool confirmar = await DisplayAlert(
            "Confirmar eliminación",
            "¿Estás seguro de que deseas eliminar este curso?",
            "Sí, eliminar",
            "Cancelar");

        if (!confirmar) return;

        try
        {
            if (await _cursoService.EliminarCursoAsync(id))
            {
                await LoadCursosAsync(); // Recargar la lista
                await DisplayAlert("Éxito", "Curso eliminado correctamente", "OK");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo eliminar el curso", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al eliminar curso: {ex.Message}");
            await DisplayAlert("Error", "No se pudo eliminar el curso", "OK");
        }
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Limpiar la lista de cursos al salir de la página
        Cursos.Clear();
    }
}
