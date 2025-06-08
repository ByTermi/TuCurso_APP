using System.Net.Http.Json;
using TuCurso.Models;

namespace TuCurso.Services;

/// <summary>
/// Servicio que gestiona las operaciones relacionadas con la técnica Pomodoro.
/// </summary>
public class PomodoroService
{
    private readonly HttpClient _httpClient;
    private readonly AuthTokenService _authTokenService;

    /// <summary>
    /// Inicializa una nueva instancia del servicio Pomodoro.
    /// </summary>
    /// <param name="httpClient">Cliente HTTP para las peticiones al servidor.</param>
    /// <param name="authTokenService">Servicio de autenticación.</param>
    public PomodoroService(HttpClient httpClient, AuthTokenService authTokenService)
    {
        _httpClient = httpClient;
        _authTokenService = authTokenService;
    }

    /// <summary>
    /// Crea un nuevo registro de sesión Pomodoro.
    /// </summary>
    /// <param name="pomodoro">Datos de la sesión Pomodoro.</param>
    /// <param name="usuarioId">ID del usuario que creó la sesión.</param>
    /// <returns>True si la sesión fue creada exitosamente.</returns>
    public async Task<bool> CrearPomodoroAsync(PomodoroDTO pomodoro, long usuarioId)
    {
        try
        {
            // Debug info
            System.Diagnostics.Debug.WriteLine($"Fecha Inicio: {pomodoro.FechaHoraInicial:yyyy-MM-ddTHH:mm:ss}");
            System.Diagnostics.Debug.WriteLine($"Fecha Fin: {pomodoro.FechaHoraDestino:yyyy-MM-ddTHH:mm:ss}");
            System.Diagnostics.Debug.WriteLine($"Usuario ID: {usuarioId}");

            // Construir la URL con todos los parámetros en el query string
            var url = $"pomodoros/crear" +
                     $"?fechaHoraInicial={Uri.EscapeDataString(pomodoro.FechaHoraInicial.ToString("yyyy-MM-ddTHH:mm:ss"))}" +
                     $"&fechaHoraDestino={Uri.EscapeDataString(pomodoro.FechaHoraDestino.ToString("yyyy-MM-ddTHH:mm:ss"))}" +
                     $"&usuarioId={usuarioId}";

            System.Diagnostics.Debug.WriteLine($"URL de la solicitud: {url}");

            // Ahora usamos PostAsync sin contenido ya que los datos van en la URL
            var response = await _httpClient.PostAsync(url, null);

            // Debug de la respuesta
            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Código de respuesta: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Contenido de respuesta: {responseContent}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al crear pomodoro: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Obtiene todas las sesiones Pomodoro de un usuario.
    /// </summary>
    /// <param name="usuarioId">ID del usuario.</param>
    /// <returns>Lista de sesiones Pomodoro del usuario.</returns>
    public async Task<List<PomodoroDTO>> ObtenerPomodorosPorUsuarioAsync(long usuarioId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/pomodoros/usuario/{usuarioId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<PomodoroDTO>>()
                    ?? new List<PomodoroDTO>();
            }
            return new List<PomodoroDTO>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al obtener pomodoros: {ex.Message}");
            return new List<PomodoroDTO>();
        }
    }

    /// <summary>
    /// Elimina una sesión Pomodoro específica.
    /// </summary>
    /// <param name="id">ID de la sesión Pomodoro a eliminar.</param>
    /// <returns>True si la sesión fue eliminada exitosamente.</returns>
    public async Task<bool> EliminarPomodoroAsync(long id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/pomodoros/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al eliminar pomodoro: {ex.Message}");
            return false;
        }
    }
}
