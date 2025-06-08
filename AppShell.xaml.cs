using System.ComponentModel;
using System.Windows.Input;
using TuCurso.Services;
using TuCurso.Views;

namespace TuCurso
{
    public partial class AppShell : Shell, INotifyPropertyChanged
    {
        private readonly AuthTokenService _authTokenService;
        private bool _isAuthenticated;

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set
            {
                if (_isAuthenticated != value)
                {
                    _isAuthenticated = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LogoutCommand { get; }

        public AppShell(AuthTokenService authTokenService)
        {
            InitializeComponent();
            _authTokenService = authTokenService;

            // Registrar rutas para navegación
            RegisterRoutes();

            // Comando para cerrar sesión
            LogoutCommand = new Command(ExecuteLogout);

            // Suscribirse a cambios de autenticación
            _authTokenService.AuthenticationChanged += OnAuthenticationChanged;

            // Verificar estado inicial de autenticación
            IsAuthenticated = _authTokenService.IsAuthenticated;

            BindingContext = this;
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("main", typeof(MainPage));
            Routing.RegisterRoute("cursos", typeof(CursoDetailPage));
            Routing.RegisterRoute("crearcurso", typeof(CrearCursoPage));
            Routing.RegisterRoute("editarcurso", typeof(EditarCursoPage));
            Routing.RegisterRoute("amigos", typeof(MisAmigosPage));
            Routing.RegisterRoute("anadiramigos", typeof(AnadirAmigosPage));
            Routing.RegisterRoute("solicitudes", typeof(SolicitudesAmistadPage));
            Routing.RegisterRoute("apuntes", typeof(ApuntesPage));
            Routing.RegisterRoute("pomodoro", typeof(PomodoroView));
            Routing.RegisterRoute("informacionCreador", typeof(SobreMiPage));

        }

        private void ExecuteLogout()
        {
            _authTokenService.Clear();
        }

        private void OnAuthenticationChanged(object sender, bool isAuthenticated)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsAuthenticated = isAuthenticated;
                if (isAuthenticated)
                {
                    Shell.Current.GoToAsync("///main");
                }
                else
                {
                    Shell.Current.GoToAsync("///login");
                }
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (_authTokenService != null)
            {
                _authTokenService.AuthenticationChanged -= OnAuthenticationChanged;
            }
        }
    }
}