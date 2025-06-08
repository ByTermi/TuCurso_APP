using System.Collections.ObjectModel;
using System.Windows.Input;
using TuCurso.Data;
using TuCurso.Models;

namespace TuCurso.Views;

/// <summary>
/// Vista para la gestión de apuntes, permitiendo crear, ver y eliminar apuntes.
/// </summary>
public partial class ApuntesPage : ContentPage
{
    private readonly IApunteRepository _apunteRepository;
    private ObservableCollection<Apunte> _apuntes;
    private bool _isBusy;

    /// <summary>
    /// Colección observable de apuntes.
    /// </summary>
    public ObservableCollection<Apunte> Apuntes
    {
        get => _apuntes;
        set
        {
            _apuntes = value;
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
        }
    }

    /// <summary>
    /// Comando para cargar la lista de apuntes.
    /// </summary>
    public ICommand CargarApuntesCommand { get; }
    
    /// <summary>
    /// Comando para añadir un nuevo apunte.
    /// </summary>
    public ICommand AnadirApunteCommand { get; }
    
    /// <summary>
    /// Comando para eliminar un apunte existente.
    /// </summary>
    public ICommand EliminarApunteCommand { get; }
    
    /// <summary>
    /// Comando para abrir un apunte.
    /// </summary>
    public ICommand AbrirApunteCommand { get; }

    public ApuntesPage(IApunteRepository apunteRepository)
    {
        InitializeComponent();
        _apunteRepository = apunteRepository;
        _apuntes = new ObservableCollection<Apunte>();

        CargarApuntesCommand = new Command(async () => await CargarApuntesAsync());
        AnadirApunteCommand = new Command(async () => await AnadirApunteAsync());
        EliminarApunteCommand = new Command<Apunte>(async (apunte) => await EliminarApunteAsync(apunte));
        AbrirApunteCommand = new Command<Apunte>(
        async (apunte) => await AbrirApunteAsync(apunte),
        (apunte) => apunte?.ArchivoExiste ?? false  // Condición de habilitación
    );

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarApuntesAsync();
    }

    private async Task CargarApuntesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var apuntes = await _apunteRepository.GetAllApuntesAsync();
            Apuntes.Clear();
            foreach (var apunte in apuntes)
            {
                Apuntes.Add(apunte);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudieron cargar los apuntes", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AnadirApunteAsync()
    {
        try
        {
            var fileResult = await FilePicker.PickAsync();
            if (fileResult == null) return;

            string nombre = await DisplayPromptAsync("Nuevo Apunte", "Nombre del apunte:", maxLength: 50);
            if (string.IsNullOrEmpty(nombre)) return;

            string descripcion = await DisplayPromptAsync("Nuevo Apunte", "Descripción (opcional):", maxLength: 200, cancel: "Omitir");

            var apunte = new Apunte
            {
                Nombre = nombre,
                Descripcion = descripcion,
                RutaArchivo = fileResult.FullPath,
                FechaCreacion = DateTime.Now
            };

            await _apunteRepository.SaveApunteAsync(apunte);
            await CargarApuntesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo añadir el apunte", "OK");
        }
    }

    private async Task EliminarApunteAsync(Apunte apunte)
    {
        if (await DisplayAlert("Confirmar", $"¿Desea eliminar el apunte {apunte.Nombre}?", "Sí", "No"))
        {
            await _apunteRepository.DeleteApunteAsync(apunte);
            await CargarApuntesAsync();
        }
    }

    private async Task AbrirApunteAsync(Apunte apunte)
    {
        if (!apunte.ArchivoExiste)
        {
            await DisplayAlert("Error", "No se encuentra el archivo", "OK");
            return;
        }

        try
        {
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(apunte.RutaArchivo)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo abrir el archivo", "OK");
        }
    }
}
