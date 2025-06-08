namespace TuCurso.Services;

/// <summary>
/// Manejador HTTP que añade automáticamente el token de autenticación a las peticiones salientes.
/// Implementa DelegatingHandler para interceptar y modificar las peticiones HTTP.
/// </summary>
public class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly AuthTokenService _authTokenService;

    /// <summary>
    /// Inicializa una nueva instancia del manejador HTTP autenticado.
    /// </summary>
    /// <param name="authTokenService">Servicio de autenticación que proporciona el token JWT.</param>
    public AuthenticatedHttpClientHandler(AuthTokenService authTokenService)
    {
        _authTokenService = authTokenService;
    }

    /// <summary>
    /// Intercepta y procesa las peticiones HTTP añadiendo el token de autenticación.
    /// </summary>
    /// <param name="request">Petición HTTP a procesar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Respuesta HTTP del servidor.</returns>
    /// <exception cref="HttpRequestException">
    /// Se lanza cuando:
    /// - Hay problemas de conexión con el servidor
    /// - La petición excede el tiempo de espera
    /// - Ocurre cualquier otro error inesperado
    /// </exception>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            if (_authTokenService.Token != null)
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authTokenService.Token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error de conexión: {ex.Message}");
            throw new HttpRequestException("No se ha podido establecer conexión con el servidor. Por favor, compruebe su conexión a Internet.", ex);
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("La solicitud ha sido cancelada por timeout");
            throw new HttpRequestException("La solicitud ha tardado demasiado. Por favor, inténtelo de nuevo más tarde.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error inesperado: {ex.Message}");
            throw new HttpRequestException("Ha ocurrido un error inesperado. Por favor, inténtelo de nuevo más tarde.", ex);
        }
    }

}
