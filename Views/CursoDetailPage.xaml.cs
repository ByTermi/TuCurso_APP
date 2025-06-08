using System.ComponentModel;
using System.Windows.Input;
using TuCurso.Models;
using TuCurso.Services;
using CommunityToolkit.Maui.Converters;


namespace TuCurso.Views;

[QueryProperty(nameof(CursoId), "id")]
public partial class CursoDetailPage : ContentPage, INotifyPropertyChanged
{
    private readonly CursoService _cursoService;
    private CursoDTO? _curso;
    private bool _isBusy;
    private long _cursoId;

    public CursoDTO? Curso
    {
        get => _curso;
        set
        {
            _curso = value;
            OnPropertyChanged();
        }
    }

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

    public long CursoId
    {
        get => _cursoId;
        set
        {
            _cursoId = value;
            LoadCursoAsync(value);
        }
    }

    private List<PuntoDeControlDTO> _puntosDeControl = new();
    private bool _isEditing;

    public List<PuntoDeControlDTO> PuntosDeControl
    {
        get => _puntosDeControl;
        set
        {
            _puntosDeControl = value;
            OnPropertyChanged();
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            _isEditing = value;
            OnPropertyChanged();
        }
    }

    public ICommand EditarCommand { get; }
    public ICommand VolverCommand { get; }
    public ICommand OpenUrlCommand { get; }
    public ICommand MarcarPuntoCommand { get; }
    public ICommand AgregarPuntoCommand { get; private set; }

    public ICommand EditarFechaCommand { get; private set; }

    public ICommand EliminarPuntoCommand { get; private set; }



    public CursoDetailPage(CursoService cursoService)
    {
        InitializeComponent();
        _cursoService = cursoService;

        EditarCommand = new Command(async () => await EditarCursoAsync());
        VolverCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        OpenUrlCommand = new Command(async () => await AbrirEnlaceCursoAsync());
        AgregarPuntoCommand = new Command(async () => await AgregarPuntoAsync());
        EditarFechaCommand = new Command<PuntoDeControlDTO>(async (punto) => await EditarFechaAsync(punto));
        EliminarPuntoCommand = new Command<PuntoDeControlDTO>(async (punto) => await EliminarPuntoAsync(punto));



        BindingContext = this;
    }

    private async Task LoadCursoAsync(long id)
    {
        try
        {
            IsBusy = true;
            Curso = await _cursoService.ObtenerCursoPorIdAsync(id);
            PuntosDeControl = await _cursoService.ObtenerPuntosDeControlPorCursoAsync(id);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo cargar el curso", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    


    private async void OnPuntoCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        try
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is PuntoDeControlDTO punto)
            {
                System.Diagnostics.Debug.WriteLine($"Intentando marcar punto {punto.Id} como {e.Value}");

                if (!punto.Id.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine("Error: ID del punto es null");
                    return;
                }

                // Llamar al endpoint PATCH /puntos-de-control/{id}/completado
                if (await _cursoService.MarcarPuntoDeControlAsync(punto.Id, e.Value))
                {
                    System.Diagnostics.Debug.WriteLine($"Punto {punto.Id} marcado como {(e.Value ? "completado" : "pendiente")}");
                    // Actualizar el estado local
                    punto.Completado = e.Value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Error al actualizar el estado del punto");
                    checkBox.IsChecked = !e.Value;
                    await DisplayAlert("Error", "No se pudo actualizar el estado del punto de control", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en OnPuntoCheckedChanged: {ex.Message}");
            if (sender is CheckBox checkBox)
            {
                // Revertir el cambio en caso de error
                checkBox.IsChecked = !e.Value;
            }
            await DisplayAlert("Error", "No se pudo actualizar el estado del punto de control", "OK");
        }
    }






    private async Task LoadPuntosDeControlAsync()
    {
        if (Curso == null) return;

        try
        {
            IsBusy = true;
            PuntosDeControl = await _cursoService.ObtenerPuntosDeControlPorCursoAsync(Curso.Id);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cargando puntos de control: {ex.Message}");
            await DisplayAlert("Error", "No se pudieron cargar los puntos de control", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }


    private async Task AgregarPuntoAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Iniciando AgregarPuntoAsync");

            if (Curso?.Id == null)
            {
                System.Diagnostics.Debug.WriteLine("Error: Curso ID es null");
                await DisplayAlert("Error", "No se pudo identificar el curso", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Curso ID: {Curso.Id}");

            // Pedir descripción
            string? descripcion = await DisplayPromptAsync(
                "Nuevo Punto de Control",
                "Introduce la descripción:",
                accept: "Siguiente",
                cancel: "Cancelar",
                placeholder: "Descripción");

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                System.Diagnostics.Debug.WriteLine("Usuario canceló o descripción vacía");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Descripción introducida: {descripcion}");

            // Pedir fecha de finalización
            DateTime fechaFinalizacion = DateTime.Now;
            bool fechaSeleccionada = false;

            if (await DisplayAlert("Fecha de finalización",
                "¿Deseas establecer una fecha de finalización?",
                "Sí", "No"))
            {
                try
                {
                    var fecha = await DisplayPromptAsync(
                        "Fecha de finalización",
                        "Introduce la fecha (dd/MM/yyyy):",
                        accept: "Guardar",
                        cancel: "Cancelar",
                        placeholder: "dd/MM/yyyy");

                    if (!string.IsNullOrWhiteSpace(fecha) &&
                        DateTime.TryParseExact(fecha, "dd/MM/yyyy",
                            null, System.Globalization.DateTimeStyles.None,
                            out DateTime fechaParsed))
                    {
                        fechaFinalizacion = fechaParsed;
                        fechaSeleccionada = true;
                    }
                    else if (!string.IsNullOrWhiteSpace(fecha))
                    {
                        await DisplayAlert("Error", "Formato de fecha inválido", "OK");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al parsear fecha: {ex.Message}");
                    await DisplayAlert("Error", "Formato de fecha inválido", "OK");
                    return;
                }
            }

            // Crear el nuevo punto de control
            var nuevoPunto = new PuntoDeControlDTO
            {
                Descripcion = descripcion,
                FechaFinalizacionDeseada = fechaSeleccionada ? fechaFinalizacion : null,
                Completado = false,
                CursoId = Curso.Id
            };

            // Mostrar el JSON que se va a enviar
            var jsonString = System.Text.Json.JsonSerializer.Serialize(nuevoPunto,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
            System.Diagnostics.Debug.WriteLine("JSON a enviar:");
            System.Diagnostics.Debug.WriteLine(jsonString);

            // Guardar en la base de datos
            if (await _cursoService.CrearPuntoDeControlAsync(nuevoPunto, Curso.Id))
            {
                System.Diagnostics.Debug.WriteLine("Punto de control creado exitosamente");
                await LoadPuntosDeControlAsync();
                await DisplayAlert("Éxito", "Punto de control añadido correctamente", "OK");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Error al crear punto de control en el servicio");
                await DisplayAlert("Error", "No se pudo crear el punto de control", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en AgregarPuntoAsync: {ex}");
            await DisplayAlert("Error", "Error al crear el punto de control", "OK");
        }
    }

    private async Task EliminarPuntoAsync(PuntoDeControlDTO punto)
    {
        try
        {
            if (punto == null || !punto.Id.HasValue)
            {
                System.Diagnostics.Debug.WriteLine("Error: punto es null o no tiene ID");
                return;
            }

            bool confirmar = await DisplayAlert(
                "Confirmar eliminación",
                "¿Estás seguro de que deseas eliminar este punto de control?",
                "Sí, eliminar",
                "Cancelar");

            if (!confirmar) return;

            System.Diagnostics.Debug.WriteLine($"Intentando eliminar punto de control {punto.Id}");

            if (await _cursoService.EliminarPuntoDeControlAsync(punto.Id.Value))
            {
                System.Diagnostics.Debug.WriteLine("Punto de control eliminado exitosamente");
                await LoadPuntosDeControlAsync(); // Recargar la lista
                await DisplayAlert("Éxito", "Punto de control eliminado correctamente", "OK");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Error al eliminar el punto de control");
                await DisplayAlert("Error", "No se pudo eliminar el punto de control", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en EliminarPuntoAsync: {ex.Message}");
            await DisplayAlert("Error", "No se pudo eliminar el punto de control", "OK");
        }
    }





    private async Task EditarFechaAsync(PuntoDeControlDTO punto)
    {
        try
        {
            string? fecha = await DisplayPromptAsync(
                "Editar fecha",
                "Introduce la nueva fecha (dd/MM/yyyy):",
                accept: "Guardar",
                cancel: "Cancelar",
                placeholder: punto.FechaFinalizacionDeseada?.ToString("dd/MM/yyyy") ?? "dd/MM/yyyy");

            if (string.IsNullOrWhiteSpace(fecha))
                return;

            if (DateTime.TryParseExact(fecha, "dd/MM/yyyy",
                null, System.Globalization.DateTimeStyles.None,
                out DateTime nuevaFecha))
            {
                punto.FechaFinalizacionDeseada = nuevaFecha;
                if (await _cursoService.ActualizarPuntoDeControlAsync(punto.Id ?? 0, punto))
                {
                    await LoadPuntosDeControlAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo actualizar la fecha", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Formato de fecha inválido", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo actualizar la fecha", "OK");
        }
    }





    private async Task EditarCursoAsync()
    {
        IsEditing = true;
        if (Curso == null) return;

        // Navegar a la página de edición pasando el ID del curso
        var navigationParameter = new Dictionary<string, object>
        {
            { "id", Curso.Id }
        };
        await Shell.Current.GoToAsync("editarcurso", navigationParameter);
    }

    private async Task AbrirEnlaceCursoAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(Curso?.Enlace))
            {
                await DisplayAlert("Error", "No hay ningún enlace disponible", "OK");
                return;
            }

            if (await Launcher.CanOpenAsync(Curso.Enlace))
            {
                await Launcher.OpenAsync(Curso.Enlace);
            }
            else
            {
                await DisplayAlert("Error", "No se puede abrir el enlace", "OK");
            }
        }
        catch (UriFormatException)
        {
            await DisplayAlert("Error", "El enlace no tiene un formato válido", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir el enlace: {ex.Message}", "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCursoAsync(CursoId);
        await LoadPuntosDeControlAsync();
    }


    private async void OnAgregarPuntoClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Botón clicked");
        await AgregarPuntoAsync();
    }


}
