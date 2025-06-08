using System.Collections.ObjectModel;
using System.Windows.Input;
using TuCurso.Models;
using TuCurso.Services;

namespace TuCurso.Views;

public partial class MisAmigosPage : ContentPage
{
    private readonly AmigosService _amigosService;
    private readonly AuthTokenService _authTokenService;
    private bool _isBusy;

    public ObservableCollection<UsuarioDTO> Amigos { get; } = new();
    public ICommand RefreshCommand { get; }
    public ICommand EliminarAmigoCommand { get; }
    public ICommand AmigoTappedCommand { get; }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public MisAmigosPage(AmigosService amigosService, AuthTokenService authTokenService)
    {
        InitializeComponent();
        _amigosService = amigosService;
        _authTokenService = authTokenService;

        RefreshCommand = new Command(async () => await CargarAmigosAsync());
        EliminarAmigoCommand = new Command<UsuarioDTO>(async (amigo) => await EliminarAmigoAsync(amigo));
        AmigoTappedCommand = new Command<UsuarioDTO>(async (amigo) => await OnAmigoTapped(amigo));

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarAmigosAsync();
    }

    private async Task CargarAmigosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var usuarioId = long.Parse(_authTokenService.GetUserId());
            var amigos = await _amigosService.ObtenerAmigosAsync(usuarioId);

            Amigos.Clear();
            foreach (var amigo in amigos)
            {
                Amigos.Add(amigo);
            }

            cantidadAmigosLabel.Text = $"Tienes {Amigos.Count} amigo{(Amigos.Count == 1 ? "" : "s")}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudieron cargar los amigos", "OK");
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EliminarAmigoAsync(UsuarioDTO amigo)
    {
        if (IsBusy) return;

        bool confirm = await DisplayAlert("Confirmar",
            $"¿Estás seguro de que quieres eliminar a {amigo.Nombre} de tus amigos?",
            "Sí", "No");

        if (!confirm) return;

        try
        {
            IsBusy = true;
            var usuarioId = long.Parse(_authTokenService.GetUserId());

            if (await _amigosService.RemoverAmigoAsync(usuarioId, amigo.Id))
            {
                Amigos.Remove(amigo);
                cantidadAmigosLabel.Text = $"Tienes {Amigos.Count} amigo{(Amigos.Count == 1 ? "" : "s")}";
            }
            else
            {
                await DisplayAlert("Error", "No se pudo eliminar el amigo", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo eliminar el amigo", "OK");
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnAmigoTapped(UsuarioDTO amigo)
    {
        if (IsBusy) return;

        try
        {
            var cursoService = Handler?.MauiContext?.Services.GetService<CursoService>();
            var pomodoroService = Handler?.MauiContext?.Services.GetService<PomodoroService>();

            if (cursoService == null || pomodoroService == null)
            {
                await DisplayAlert("Error", "No se pudieron obtener los servicios necesarios", "OK");
                return;
            }

            var detailPage = new AmigoDetailPage(amigo, cursoService, pomodoroService);
            await Navigation.PushAsync(detailPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo abrir el detalle del amigo", "OK");
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
    }


}
