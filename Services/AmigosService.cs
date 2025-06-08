using System.Net.Http.Json;
using TuCurso.Models;

namespace TuCurso.Services
{
    /// <summary>
    /// Servicio que gestiona las operaciones relacionadas con amigos y solicitudes de amistad.
    /// </summary>
    public class AmigosService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthTokenService _authTokenService;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de amigos.
        /// </summary>
        /// <param name="httpClient">Cliente HTTP para las peticiones al servidor.</param>
        /// <param name="authTokenService">Servicio de autenticación.</param>
        public AmigosService(HttpClient httpClient, AuthTokenService authTokenService)
        {
            _httpClient = httpClient;
            _authTokenService = authTokenService;
        }

        /// <summary>
        /// Obtiene la lista de amigos de un usuario.
        /// </summary>
        /// <param name="usuarioId">ID del usuario.</param>
        /// <returns>Lista de amigos del usuario.</returns>
        public async Task<List<UsuarioDTO>> ObtenerAmigosAsync(long usuarioId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"usuarios/{usuarioId}/amigos");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<UsuarioDTO>>() ?? new List<UsuarioDTO>();
                }
                return new List<UsuarioDTO>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener amigos: {ex.Message}");
                return new List<UsuarioDTO>();
            }
        }

        /// <summary>
        /// Elimina una amistad existente.
        /// </summary>
        /// <param name="usuarioId">ID del usuario que realiza la acción.</param>
        /// <param name="amigoId">ID del amigo a eliminar.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public async Task<bool> RemoverAmigoAsync(long usuarioId, long amigoId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"usuarios/{usuarioId}/amigos/{amigoId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al eliminar amigo: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Busca usuarios para agregar como amigos.
        /// </summary>
        /// <param name="usuarioId">ID del usuario que realiza la búsqueda.</param>
        /// <param name="nombre">Nombre o término de búsqueda.</param>
        /// <returns>Lista de usuarios encontrados.</returns>
        public async Task<List<UsuarioDTO>> BuscarUsuariosParaAgregarAsync(long usuarioId, string nombre)
        {
            try
            {
                var response = await _httpClient.GetAsync($"usuarios/{usuarioId}/buscar-amigos?nombre={Uri.EscapeDataString(nombre)}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<UsuarioDTO>>() ?? new List<UsuarioDTO>();
                }
                return new List<UsuarioDTO>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al buscar usuarios: {ex.Message}");
                return new List<UsuarioDTO>();
            }
        }

        /// <summary>
        /// Envía una solicitud de amistad.
        /// </summary>
        /// <param name="emisorId">ID del usuario que envía la solicitud.</param>
        /// <param name="receptorId">ID del usuario que recibirá la solicitud.</param>
        /// <returns>True si la solicitud fue enviada exitosamente.</returns>
        public async Task<bool> EnviarSolicitudAsync(long emisorId, long receptorId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"solicitudes-amistad/enviar?emisorId={emisorId}&receptorId={receptorId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al enviar solicitud: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene la lista de solicitudes de amistad recibidas por un usuario.
        /// </summary>
        /// <param name="usuarioId">ID del usuario.</param>
        /// <returns>Lista de solicitudes de amistad recibidas.</returns>
        public async Task<List<SolicitudAmistad>> ObtenerSolicitudesRecibidasAsync(long usuarioId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"solicitudes-amistad/recibidas/{usuarioId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"JSON de solicitudes recibidas: {jsonString}");

                    return await response.Content.ReadFromJsonAsync<List<SolicitudAmistad>>() ?? new List<SolicitudAmistad>();
                }
                System.Diagnostics.Debug.WriteLine($"Error en la respuesta: {response.StatusCode}");
                return new List<SolicitudAmistad>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener solicitudes: {ex.Message}");
                return new List<SolicitudAmistad>();
            }
        }

        /// <summary>
        /// Acepta una solicitud de amistad.
        /// </summary>
        /// <param name="solicitudId">ID de la solicitud de amistad.</param>
        /// <returns>True si la solicitud fue aceptada exitosamente.</returns>
        public async Task<bool> AceptarSolicitudAsync(long solicitudId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"solicitudes-amistad/{solicitudId}/aceptar", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al aceptar solicitud: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Rechaza una solicitud de amistad.
        /// </summary>
        /// <param name="solicitudId">ID de la solicitud de amistad.</param>
        /// <returns>True si la solicitud fue rechazada exitosamente.</returns>
        public async Task<bool> RechazarSolicitudAsync(long solicitudId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"solicitudes-amistad/{solicitudId}/rechazar", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al rechazar solicitud: {ex.Message}");
                return false;
            }
        }
    }
}
