using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TuCurso.Models;

namespace TuCurso.Services;

/// <summary>
/// Servicio que gestiona las operaciones de autenticación con el servidor.
/// Maneja el login y registro de usuarios en la aplicación.
/// </summary>
public class AuthService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Inicializa una nueva instancia del servicio de autenticación.
    /// </summary>
    /// <param name="httpClient">Cliente HTTP configurado para las peticiones al servidor.</param>
    /// <exception cref="ArgumentNullException">Se lanza si httpClient es null.</exception>
    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Realiza el proceso de inicio de sesión de un usuario.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="password">Contraseña del usuario.</param>
    /// <returns>
    /// Un LoginResult que contiene:
    /// - Success: indica si el login fue exitoso
    /// - Token: token JWT si el login fue exitoso
    /// - Id: identificador del usuario
    /// - Expiration: fecha de expiración del token
    /// - ErrorMessage: mensaje de error si el login falló
    /// </returns>
    /// <remarks>
    /// El método realiza logging detallado para diagnóstico e incluye manejo de diferentes
    /// escenarios de error como credenciales inválidas o problemas de conexión.
    /// </remarks>
    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            var loginData = new
            {
                email = email,
                pass = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginData),
                Encoding.UTF8,
                "application/json");

            // Imprimir el JSON que se envía
            System.Diagnostics.Debug.WriteLine("JSON enviado:");
            System.Diagnostics.Debug.WriteLine(await content.ReadAsStringAsync());

            var response = await _httpClient.PostAsync("/usuarios/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                if (responseContent != null &&
                    responseContent.TryGetValue("token", out string? token) &&
                    responseContent.TryGetValue("expiration", out string? expirationStr) &&
                    responseContent.TryGetValue("id", out string? id)) // Añadido id
                {
                    // Imprimir el contenido completo de la respuesta
                    System.Diagnostics.Debug.WriteLine("Respuesta completa:");
                    System.Diagnostics.Debug.WriteLine(JsonSerializer.Serialize(responseContent, new JsonSerializerOptions { WriteIndented = true }));

                    return new LoginResult
                    {
                        Success = true,
                        Id = id, // Asignando el id
                        Token = token,
                        Expiration = expirationStr
                    };
                }

                // Si llegamos aquí, imprimir las claves que sí llegaron
                System.Diagnostics.Debug.WriteLine("Claves recibidas en la respuesta:");
                if (responseContent != null)
                {
                    foreach (var key in responseContent.Keys)
                    {
                        System.Diagnostics.Debug.WriteLine($"Clave: {key}, Valor: {responseContent[key]}");
                    }
                }

                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = "Formato de respuesta inválido"
                };
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = "Credenciales inválidas"
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error response: {errorContent}");
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = $"Error: {response.StatusCode} - {errorContent}"
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
            return new LoginResult
            {
                Success = false,
                ErrorMessage = $"Error de conexión: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="nombre">Nombre del usuario.</param>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="password">Contraseña del usuario.</param>
    /// <param name="descripcion">Descripción o biografía del usuario (opcional).</param>
    /// <param name="icono">URL o identificador del icono del usuario (opcional).</param>
    /// <returns>
    /// Un RegisterResult que contiene:
    /// - Success: indica si el registro fue exitoso
    /// - ErrorMessage: mensaje de error si el registro falló
    /// </returns>
    /// <remarks>
    /// El método maneja errores de conexión y respuestas no exitosas del servidor,
    /// proporcionando mensajes de error descriptivos.
    /// </remarks>
    public async Task<RegisterResult> RegisterAsync(string nombre, string email, string password,
        string descripcion, string icono)
    {
        try
        {
            var registerData = new
            {
                nombre = nombre,
                email = email,
                pass = password,
                descripcion = descripcion,
                icono = icono
            };

            var content = new StringContent(
                JsonSerializer.Serialize(registerData),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/usuarios/crear", content);

            if (response.IsSuccessStatusCode)
            {
                return new RegisterResult { Success = true };
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new RegisterResult
            {
                Success = false,
                ErrorMessage = $"Error: {response.StatusCode} - {errorContent}"
            };
        }
        catch (Exception ex)
        {
            return new RegisterResult
            {
                Success = false,
                ErrorMessage = $"Error de conexión: {ex.Message}"
            };
        }
    }
}
