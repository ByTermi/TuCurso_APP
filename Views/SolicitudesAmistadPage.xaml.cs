using System.Collections.ObjectModel;
using System.Windows.Input;
using TuCurso.Models;
using TuCurso.Services;

namespace TuCurso.Views;

/// <summary>
/// Vista para gestionar las solicitudes de amistad recibidas.
/// </summary>
public partial class SolicitudesAmistadPage : ContentPage
{
    private readonly AmigosService _amigosService;
    private readonly AuthTokenService _authTokenService;
    private bool _isBusy;

    /// <summary>
    /// Colección de solicitudes de amistad pendientes.
    /// </summary>
    public ObservableCollection<SolicitudAmistad> Solicitudes { get; } = new();

    /// <summary>
    /// Comando para refrescar la lista de solicitudes.
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Comando para aceptar una solicitud de amistad.
    /// </summary>
    public ICommand AceptarSolicitudCommand { get; }

    /// <summary>
    /// Comando para rechazar una solicitud de amistad.
    /// </summary>
    public ICommand RechazarSolicitudCommand { get; }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public SolicitudesAmistadPage(AmigosService amigosService, AuthTokenService authTokenService)
    {
        InitializeComponent();
        _amigosService = amigosService;
        _authTokenService = authTokenService;

        RefreshCommand = new Command(async () => await CargarSolicitudesAsync());
        AceptarSolicitudCommand = new Command<SolicitudAmistad>(async (solicitud) => await AceptarSolicitudAsync(solicitud));
        RechazarSolicitudCommand = new Command<SolicitudAmistad>(async (solicitud) => await RechazarSolicitudAsync(solicitud));

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarSolicitudesAsync();
    }

    private async Task CargarSolicitudesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var usuarioId = long.Parse(_authTokenService.GetUserId());
            var solicitudes = await _amigosService.ObtenerSolicitudesRecibidasAsync(usuarioId);

            Solicitudes.Clear();
            foreach (var solicitud in solicitudes)
            {
                Solicitudes.Add(solicitud);
            }

            ActualizarEtiquetaSolicitudes();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudieron cargar las solicitudes", "OK");
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ActualizarEtiquetaSolicitudes()
    {
        cantidadSolicitudesLabel.Text = Solicitudes.Count switch
        {
            0 => "No tienes solicitudes pendientes",
            1 => "Tienes 1 solicitud pendiente",
            _ => $"Tienes {Solicitudes.Count} solicitudes pendientes"
        };
    }

    private async Task AceptarSolicitudAsync(SolicitudAmistad solicitud)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            if (await _amigosService.AceptarSolicitudAsync(solicitud.Id))
            {
                Solicitudes.Remove(solicitud);
                ActualizarEtiquetaSolicitudes();
                await DisplayAlert("Éxito", $"Has aceptado la solicitud de {solicitud.Emisor.Nombre}", "OK");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo aceptar la solicitud", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo aceptar la solicitud", "OK");
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RechazarSolicitudAsync(SolicitudAmistad solicitud)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            if (await _amigosService.RechazarSolicitudAsync(solicitud.Id))
            {
                Solicitudes.Remove(solicitud);
                ActualizarEtiquetaSolicitudes();
                await DisplayAlert("Éxito", $"Has rechazado la solicitud de {solicitud.Emisor.Nombre}", "OK");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo rechazar la solicitud", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo rechazar la solicitud", "OK");
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
