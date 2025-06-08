using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using TuCurso.Services;
using TuCurso.Views;
using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using TuCurso.Data;

namespace TuCurso;

/// <summary>
/// Clase estática que configura y construye la aplicación MAUI.
/// Esta es la clase principal de configuración donde se registran todos los servicios,
/// dependencias y configuraciones necesarias para que la aplicación funcione correctamente.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Propiedad que determina la URL base de la API según la plataforma.
    /// - En Android: usa 10.0.2.2 (IP especial del emulador para localhost)
    /// - En otras plataformas: usa localhost directamente
    /// </summary>
    private static Uri BaseApiUri => DeviceInfo.Platform == DevicePlatform.Android
        ? new Uri("http://10.0.2.2:8080")
        : new Uri("http://localhost:8080");

    /// <summary>
    /// Método principal que crea y configura la aplicación MAUI.
    /// Aquí se registran todos los servicios, dependencias y configuraciones.
    /// </summary>
    /// <returns>Una instancia configurada de MauiApp</returns>
    public static MauiApp CreateMauiApp()
    {
        // Crear el constructor de la aplicación MAUI
        var builder = MauiApp.CreateBuilder();

        // ============================================
        // CONFIGURACIÓN BÁSICA DE MAUI
        // ============================================
        builder
            .UseMauiApp<App>()                    // Establece la clase App como aplicación principal
            .UseSkiaSharp()                       // Habilita SkiaSharp para gráficos avanzados
            .UseMauiCommunityToolkit()            // Añade herramientas adicionales de la comunidad
            .ConfigureFonts(fonts =>              // Configura las fuentes de texto
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ============================================
        // REGISTRO DE SERVICIOS DE NEGOCIO
        // ============================================

        // SERVICIOS DE AUTENTICACIÓN
        // Singleton: Una sola instancia durante toda la vida de la aplicación
        builder.Services.AddSingleton<AuthTokenService>();              // Gestiona tokens de autenticación
        builder.Services.AddSingleton<HttpsClientHandlerService>();     // Maneja conexiones HTTPS seguras
        builder.Services.AddTransient<AuthenticatedHttpClientHandler>(); // Intercepta peticiones HTTP para añadir autenticación
        builder.Services.AddSingleton<AuthService>();                   // Servicio principal de autenticación

        // CLIENTE HTTP GENÉRICO
        builder.Services.AddSingleton<HttpClient>();                    // Cliente HTTP básico

        // SERVICIOS DE LA APLICACIÓN
        builder.Services.AddSingleton<CursoService>();                  // Gestiona operaciones con cursos
        builder.Services.AddSingleton<PomodoroService>();               // Gestiona temporizador Pomodoro
        builder.Services.AddSingleton<AmigosService>();                 // Gestiona sistema de amigos

        // ============================================
        // CONFIGURACIÓN DE BASE DE DATOS SQLITE
        // ============================================

        // Crear la ruta donde se guardará la base de datos SQLite
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "apuntes.db3");
        // Registrar el repositorio de apuntes con la ruta de la BD
        builder.Services.AddSingleton<IApunteRepository>(s => new ApunteRepository(dbPath));

        // ============================================
        // CONFIGURACIÓN DE CLIENTES HTTP ESPECÍFICOS
        // ============================================

        // CLIENTE HTTP PARA AUTHSERVICE
        // AddHttpClient crea un HttpClient configurado específicamente para este servicio
        builder.Services.AddHttpClient<AuthService>((serviceProvider, client) =>
        {
            client.BaseAddress = BaseApiUri;  // Establece la URL base
        })
        .ConfigurePrimaryHttpMessageHandler(services =>
        {
            // Configura el manejador HTTPS específico para la plataforma
            var handler = services.GetRequiredService<HttpsClientHandlerService>()
                                .GetPlatformMessageHandler();
            return handler;
        })
        .AddHttpMessageHandler<AuthenticatedHttpClientHandler>();  // Añade autenticación automática

        // CLIENTE HTTP PARA CURSOSERVICE
        builder.Services.AddHttpClient<CursoService>((serviceProvider, client) =>
        {
            client.BaseAddress = BaseApiUri;
        })
        .AddHttpMessageHandler<AuthenticatedHttpClientHandler>();  // Solo necesita autenticación

        // CLIENTE HTTP PARA POMODOROSERVICE
        builder.Services.AddHttpClient<PomodoroService>((serviceProvider, client) =>
        {
            client.BaseAddress = BaseApiUri;
        })
        .ConfigurePrimaryHttpMessageHandler(services =>
        {
            var handler = services.GetRequiredService<HttpsClientHandlerService>()
                                .GetPlatformMessageHandler();
            return handler;
        })
        .AddHttpMessageHandler<AuthenticatedHttpClientHandler>();

        // CLIENTE HTTP PARA AMIGOSSERVICE
        builder.Services.AddHttpClient<AmigosService>((serviceProvider, client) =>
        {
            client.BaseAddress = BaseApiUri;
        })
        .ConfigurePrimaryHttpMessageHandler(services =>
        {
            var handler = services.GetRequiredService<HttpsClientHandlerService>()
                                .GetPlatformMessageHandler();
            return handler;
        })
        .AddHttpMessageHandler<AuthenticatedHttpClientHandler>();

        // ============================================
        // REGISTRO DE PÁGINAS/VISTAS
        // ============================================

        // Transient: Se crea una nueva instancia cada vez que se solicita
        // Esto es ideal para páginas porque cada navegación puede necesitar una instancia fresca
        builder.Services.AddTransient<LoginPage>();                    // Página de inicio de sesión
        builder.Services.AddTransient<RegisterPage>();                 // Página de registro
        builder.Services.AddTransient<MainPage>();                     // Página principal
        builder.Services.AddTransient<CursoDetailPage>();              // Detalle de curso
        builder.Services.AddTransient<CrearCursoPage>();               // Crear nuevo curso
        builder.Services.AddTransient<EditarCursoPage>();              // Editar curso existente
        builder.Services.AddTransient<MisAmigosPage>();                // Lista de amigos
        builder.Services.AddTransient<AnadirAmigosPage>();             // Añadir nuevos amigos
        builder.Services.AddTransient<SolicitudesAmistadPage>();       // Solicitudes de amistad pendientes
        builder.Services.AddTransient<ApuntesPage>();                  // Página de apuntes
        builder.Services.AddTransient<AmigoDetailPage>();              // Detalle de un amigo

        // ============================================
        // CONFIGURACIÓN DE DESARROLLO
        // ============================================

#if DEBUG
        // Solo en modo DEBUG: añade logging detallado para depuración
        builder.Logging.AddDebug();
#endif

        // Construir y retornar la aplicación configurada
        return builder.Build();
    }
}
