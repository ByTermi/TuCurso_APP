using System.Collections.ObjectModel;
using System.Windows.Input;
using TuCurso.Models;
using TuCurso.Services;


namespace TuCurso.Views;

public partial class AnadirAmigosPage : ContentPage
{
    private readonly AmigosService _amigosService;
    private readonly AuthTokenService _authTokenService;
    private bool _isBusy;
    private string _lastSearchText = string.Empty;

    public ObservableCollection<UsuarioDTO> Usuarios { get; } = new();
    public ICommand EnviarSolicitudCommand { get; }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public AnadirAmigosPage(AmigosService amigosService, AuthTokenService authTokenService)
    {
        InitializeComponent();
        _amigosService = amigosService;
        _authTokenService = authTokenService;

        EnviarSolicitudCommand = new Command<UsuarioDTO>(async (usuario) => await EnviarSolicitudAsync(usuario));

        BindingContext = this;
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _lastSearchText = e.NewTextValue;
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            Usuarios.Clear();
        }
    }

    private async void OnSearchButtonPressed(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_lastSearchText)) return;
        await BuscarUsuariosAsync(_lastSearchText);
    }

    private async Task BuscarUsuariosAsync(string nombre)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var usuarioId = long.Parse(_authTokenService.GetUserId());
            var usuarios = await _amigosService.BuscarUsuariosParaAgregarAsync(usuarioId, nombre);

            Usuarios.Clear();
            foreach (var usuario in usuarios)
            {
                Usuarios.Add(usuario);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudieron buscar usuarios", "OK");
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EnviarSolicitudAsync(UsuarioDTO usuario)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var emisorId = long.Parse(_authTokenService.GetUserId());

            if (await _amigosService.EnviarSolicitudAsync(emisorId, usuario.Id))
            {
                await DisplayAlert("Éxito", $"Solicitud enviada a {usuario.Nombre}", "OK");
                Usuarios.Remove(usuario);
            }
            else
            {
                await DisplayAlert("Error", "No se pudo enviar la solicitud", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo enviar la solicitud", "OK");
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}