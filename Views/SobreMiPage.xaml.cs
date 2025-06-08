using System.Windows.Input;

namespace TuCurso.Views;

public partial class SobreMiPage : ContentPage
{
    public SobreMiPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public ICommand ComandoAbrirLinkedIn => new Command(async () =>
    {
        try
        {
            await Launcher.OpenAsync("https://www.linkedin.com/in/jaime-novillo-benito");
        }
        catch (Exception excepcion)
        {
            await DisplayAlert("Error", "No se pudo abrir el enlace de LinkedIn", "Aceptar");
        }
    });

    public ICommand ComandoAbrirGitHub => new Command(async () =>
    {
        try
        {
            await Launcher.OpenAsync("https://github.com/ByTermi");
        }
        catch (Exception excepcion)
        {
            await DisplayAlert("Error", "No se pudo abrir el enlace de GitHub", "Aceptar");
        }
    });
}