using TuCurso.Models;
using TuCurso.Services;

namespace TuCurso.Views;

/// <summary>
/// Página que muestra el detalle de un amigo, incluyendo sus cursos y sesiones Pomodoro.
/// Esta vista forma parte del módulo social de la aplicación.
/// </summary>
public partial class AmigoDetailPage : ContentPage
{
    private readonly CursoService _cursoService;
    private readonly PomodoroService _pomodoroService;

    /// <summary>
    /// Obtiene o establece el usuario cuyo detalle se está mostrando.
    /// El setter es privado para mantener la integridad de los datos.
    /// </summary>
    public UsuarioDTO Usuario { get; private set; }

    /// <summary>
    /// Obtiene o establece la lista de cursos del usuario.
    /// El setter es privado para asegurar que solo se modifique internamente.
    /// </summary>
    public List<CursoDTO> Cursos { get; private set; } = new();

    /// <summary>
    /// Obtiene o establece la lista de sesiones Pomodoro del usuario.
    /// El setter es privado para asegurar que solo se modifique internamente.
    /// </summary>
    public List<PomodoroDTO> Pomodoros { get; private set; } = new();

    /// <summary>
    /// Inicializa una nueva instancia de AmigoDetailPage.
    /// </summary>
    /// <param name="usuario">Usuario cuyos detalles se mostrarán.</param>
    /// <param name="cursoService">Servicio para acceder a los datos de cursos.</param>
    /// <param name="pomodoroService">Servicio para acceder a los datos de Pomodoro.</param>
    /// <exception cref="ArgumentNullException">Se lanza si alguno de los servicios es null.</exception>
    public AmigoDetailPage(UsuarioDTO usuario, CursoService cursoService, PomodoroService pomodoroService)
    {
        InitializeComponent();
        Usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
        _cursoService = cursoService ?? throw new ArgumentNullException(nameof(cursoService));
        _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
        
        BindingContext = this;
        CargarDatosAmigo();
    }

    /// <summary>
    /// Carga los datos del amigo desde los servicios correspondientes.
    /// Incluye cursos y sesiones Pomodoro asociadas al usuario.
    /// </summary>
    /// <remarks>
    /// Este método es asíncrono pero se llama de forma void porque es un manejador de eventos.
    /// En caso de error, muestra un mensaje al usuario.
    /// </remarks>
    private async void CargarDatosAmigo()
    {
        try
        {
            // Cargar cursos y pomodoros del amigo de forma paralela para mejorar el rendimiento
            var cursosTask = _cursoService.ObtenerCursosPorUsuarioAsync(Usuario.Id.ToString());
            var pomodorosTask = _pomodoroService.ObtenerPomodorosPorUsuarioAsync(Usuario.Id);

            await Task.WhenAll(cursosTask, pomodorosTask);

            Cursos = cursosTask.Result.ToList();
            Pomodoros = pomodorosTask.Result.ToList();

            // Forzar actualización del binding para reflejar los cambios en la UI
            OnPropertyChanged(nameof(Cursos));
            OnPropertyChanged(nameof(Pomodoros));
        }
        catch (Exception ex)
        {
            // Log del error para diagnóstico
            System.Diagnostics.Debug.WriteLine($"Error al cargar datos del amigo: {ex}");
            await DisplayAlert("Error", "No se pudo cargar la información del amigo", "OK");
        }
    }
}
