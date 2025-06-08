using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System.ComponentModel;
using TuCurso.Services;
using TuCurso.Models;
using System.Windows.Input;

namespace TuCurso.Views;

/// <summary>
/// Vista del temporizador Pomodoro y registro de sesiones.
/// Implementa la técnica Pomodoro con diferentes configuraciones de tiempo.
/// </summary>
public partial class PomodoroView : ContentPage, INotifyPropertyChanged
{
    private readonly IDispatcherTimer _timer;
    private DateTime _tiempoInicio;
    private DateTime _tiempoFin;
    private bool _isRunning;
    private TimeSpan _duracionSeleccionada = TimeSpan.FromMinutes(25);

    private readonly PomodoroService _pomodoroService;
    private readonly AuthTokenService _authTokenService;

    private List<PomodoroDTO> _pomodoros = new();
    /// <summary>
    /// Lista de sesiones Pomodoro completadas.
    /// </summary>
    public List<PomodoroDTO> Pomodoros
    {
        get => _pomodoros;
        set
        {
            _pomodoros = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Comando para eliminar una sesión Pomodoro.
    /// </summary>
    public ICommand EliminarPomodoroCommand { get; }


    public PomodoroView(PomodoroService pomodoroService, AuthTokenService authTokenService)
    {
        InitializeComponent();
        _pomodoroService = pomodoroService;
        _authTokenService = authTokenService;
        _timer = Application.Current?.Dispatcher.CreateTimer();
        if (_timer != null)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        // Seleccionar el primer preset por defecto
        tiempoPicker.SelectedIndex = 0;
        ActualizarCirculoProgreso(1.0f);

        EliminarPomodoroCommand = new Command<PomodoroDTO>(async (pomodoro) => await EliminarPomodoroAsync(pomodoro));

        BindingContext = this;
    }

    private void OnTiempoSeleccionado(object sender, EventArgs e)
    {
        if (tiempoPicker.SelectedIndex >= 0)
        {
            tiempoPersonalizadoGrid.IsVisible = tiempoPicker.SelectedIndex == 3; // Personalizado

            switch (tiempoPicker.SelectedIndex)
            {
                case 0: // 25 minutos (Pomodoro)
                    _duracionSeleccionada = TimeSpan.FromMinutes(25);
                    break;
                case 1: // 5 minutos (Descanso corto)
                    _duracionSeleccionada = TimeSpan.FromMinutes(5);
                    break;
                case 2: // 15 minutos (Descanso largo)
                    _duracionSeleccionada = TimeSpan.FromMinutes(15);
                    break;
                case 3: // Personalizado
                    ActualizarTiempoPersonalizado();
                    break;
            }

            ActualizarDisplayTiempo();
        }
    }

    private void OnTiempoPersonalizadoCambiado(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            // Si está vacío, establecer a "0"
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                entry.Text = "0";
                return;
            }

            // Validar que solo sean números
            if (!string.IsNullOrEmpty(e.NewTextValue) && !int.TryParse(e.NewTextValue, out _))
            {
                entry.Text = e.OldTextValue ?? "0";
                return;
            }

            // Validar rangos
            if (int.TryParse(e.NewTextValue, out int valor))
            {
                if (entry == horasEntry && valor > 23)
                {
                    entry.Text = "23";
                }
                else if ((entry == minutosEntry || entry == segundosEntry) && valor > 60)
                {
                    entry.Text = "60";
                }
            }

            ActualizarTiempoPersonalizado();
        }
    }


    private void ActualizarTiempoPersonalizado()
    {
        if (int.TryParse(horasEntry.Text, out int horas) &&
            int.TryParse(minutosEntry.Text, out int minutos) &&
            int.TryParse(segundosEntry.Text, out int segundos))
        {
            TimeSpan tiempo = new TimeSpan(horas, minutos, segundos);
            ActualizarTiempoSeleccionado(tiempo);
        }
    }

    private void ActualizarTiempoSeleccionado(TimeSpan tiempo)
    {
        if (tiempo.TotalSeconds <= 0)
        {
            // Si el tiempo es 0 o negativo, establecer valores mínimos
            horasEntry.Text = "0";
            minutosEntry.Text = "1"; // Mínimo 1 minuto
            segundosEntry.Text = "0";
            _duracionSeleccionada = TimeSpan.FromMinutes(1);
        }
        else
        {
            _duracionSeleccionada = tiempo;
        }

        ActualizarDisplayTiempo();
        ActualizarCirculoProgreso(1.0f);
    }


    private void ActualizarDisplayTiempo()
    {
        if (_duracionSeleccionada.TotalHours >= 1)
        {
            tiempoRestanteLabel.Text = $"{(int)_duracionSeleccionada.TotalHours}:{_duracionSeleccionada.Minutes:00}:{_duracionSeleccionada.Seconds:00}";
        }
        else
        {
            tiempoRestanteLabel.Text = $"{_duracionSeleccionada.Minutes:00}:{_duracionSeleccionada.Seconds:00}";
        }
    }

    private async void OnIniciarClicked(object sender, EventArgs e)
    {
        if (!_isRunning && _duracionSeleccionada.TotalSeconds > 0)
        {
            _tiempoInicio = DateTime.Now;
            _tiempoFin = _tiempoInicio.Add(_duracionSeleccionada);
            _isRunning = true;

            iniciarButton.IsEnabled = false;
            detenerButton.IsEnabled = true;
            tiempoPicker.IsEnabled = false;
            tiempoPersonalizadoGrid.IsEnabled = false;
            estadoLabel.Text = "En progreso";

            _timer?.Start();
        }
    }

    private void OnDetenerClicked(object sender, EventArgs e)
    {
        if (_isRunning)
        {
            _timer?.Stop();
            _isRunning = false;

            iniciarButton.IsEnabled = true;
            detenerButton.IsEnabled = false;
            tiempoPicker.IsEnabled = true;
            tiempoPersonalizadoGrid.IsEnabled = true;
            estadoLabel.Text = "Detenido";

            ActualizarDisplayTiempo();
            ActualizarCirculoProgreso(1.0f);
        }
    }

    /// <summary>
    /// Manejador del temporizador que actualiza la UI y gestiona el fin de la sesión.
    /// </summary>
    private async void Timer_Tick(object sender, EventArgs e)
    {
        var tiempoRestante = _tiempoFin - DateTime.Now;
        if (tiempoRestante.TotalSeconds <= 0)
        {
            _timer?.Stop();
            _isRunning = false;
            iniciarButton.IsEnabled = true;
            detenerButton.IsEnabled = false;
            tiempoPicker.IsEnabled = true;
            tiempoPersonalizadoGrid.IsEnabled = true;
            estadoLabel.Text = "¡Tiempo completado!";
            tiempoRestanteLabel.Text = "0:00";

            ActualizarCirculoProgreso(1.0f);

            // Guardar el pomodoro completado
            var pomodoro = new PomodoroDTO
            {
                FechaHoraInicial = _tiempoInicio,
                FechaHoraDestino = _tiempoFin,
                UsuarioId = long.Parse(_authTokenService.GetUserId())
            };

            try
            {
                if (await _pomodoroService.CrearPomodoroAsync(pomodoro, pomodoro.UsuarioId))
                {
                    await DisplayAlert("Pomodoro", "¡Tiempo completado y registrado!", "OK");
                    await CargarPomodorosAsync();
                }
                else
                {
                    await DisplayAlert("Pomodoro", "¡Tiempo completado pero no se pudo registrar!", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar pomodoro: {ex.Message}");
                await DisplayAlert("Error", "No se pudo guardar el pomodoro", "OK");
            }
        }
        else
        {
            // El resto del código permanece igual...
            if (tiempoRestante.TotalHours >= 1)
            {
                tiempoRestanteLabel.Text = $"{(int)tiempoRestante.TotalHours}:{tiempoRestante.Minutes:00}:{tiempoRestante.Seconds:00}";
            }
            else
            {
                tiempoRestanteLabel.Text = $"{tiempoRestante.Minutes:00}:{tiempoRestante.Seconds:00}";
            }

            float progreso = (float)(tiempoRestante.TotalSeconds / _duracionSeleccionada.TotalSeconds);
            ActualizarCirculoProgreso(progreso);
        }
    }

    /// <summary>
    /// Actualiza la visualización del progreso en el círculo.
    /// </summary>
    /// <param name="porcentaje">Valor entre 0 y 1 que representa el progreso.</param>
    private void ActualizarCirculoProgreso(float porcentaje)
    {
        const float radio = 145;
        const float centerX = 150;
        const float centerY = 150;

        // Asegurar que el porcentaje está entre 0 y 1
        porcentaje = Math.Max(0, Math.Min(1, porcentaje));

        // Si es 1 (círculo completo), dibujamos un círculo completo
        if (Math.Abs(porcentaje - 1) < 0.001)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure
            {
                StartPoint = new Point(centerX, centerY - radio)
            };

            var arcSegment1 = new ArcSegment
            {
                Point = new Point(centerX, centerY + radio),
                Size = new Size(radio, radio),
                IsLargeArc = true,
                SweepDirection = SweepDirection.Clockwise,
                RotationAngle = 0
            };

            var arcSegment2 = new ArcSegment
            {
                Point = new Point(centerX, centerY - radio),
                Size = new Size(radio, radio),
                IsLargeArc = true,
                SweepDirection = SweepDirection.Clockwise,
                RotationAngle = 0
            };

            pathFigure.Segments.Add(arcSegment1);
            pathFigure.Segments.Add(arcSegment2);
            pathGeometry.Figures.Add(pathFigure);

            progressPath.Data = pathGeometry;
            return;
        }

        // Para porcentajes menores a 1
        float angulo = porcentaje * 360;
        float anguloRadianes = (float)(Math.PI * (angulo - 90) / 180.0);

        float endX = centerX + (float)(radio * Math.Cos(anguloRadianes));
        float endY = centerY + (float)(radio * Math.Sin(anguloRadianes));

        var normalPathGeometry = new PathGeometry();
        var normalPathFigure = new PathFigure
        {
            StartPoint = new Point(centerX, centerY - radio),
            IsClosed = false
        };

        var arcSegment = new ArcSegment
        {
            Point = new Point(endX, endY),
            Size = new Size(radio, radio),
            IsLargeArc = angulo > 180,
            SweepDirection = SweepDirection.Clockwise,
            RotationAngle = 0
        };

        normalPathFigure.Segments.Add(arcSegment);
        normalPathGeometry.Figures.Add(normalPathFigure);

        progressPath.Data = normalPathGeometry;
    }

    private async Task EliminarPomodoroAsync(PomodoroDTO pomodoro)
    {
        if (pomodoro?.Id == null) return;

        bool confirmar = await DisplayAlert(
            "Confirmar eliminación",
            "¿Estás seguro de que deseas eliminar este pomodoro?",
            "Sí, eliminar",
            "Cancelar");

        if (!confirmar) return;

        if (await _pomodoroService.EliminarPomodoroAsync(pomodoro.Id.Value))
        {
            await CargarPomodorosAsync();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo eliminar el pomodoro", "OK");
        }
    }

    private async Task CargarPomodorosAsync()
    {
        try
        {
            var userId = long.Parse(_authTokenService.GetUserId());
            Pomodoros = await _pomodoroService.ObtenerPomodorosPorUsuarioAsync(userId);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudieron cargar los pomodoros", "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarPomodorosAsync();
    }
}
