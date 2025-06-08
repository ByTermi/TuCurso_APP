using TuCurso.Services;

namespace TuCurso
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Obtener el AuthTokenService del contenedor de dependencias
            var authTokenService = _serviceProvider.GetRequiredService<AuthTokenService>();
            var window = new Window(new AppShell(authTokenService));
            return window;
        }
    }
}