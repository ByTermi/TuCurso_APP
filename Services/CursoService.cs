using System.Net.Http.Json;
using TuCurso.Models;

namespace TuCurso.Services;

/// <summary>
/// Servicio que gestiona las operaciones relacionadas con los cursos en la aplicación.
/// Maneja la comunicación con el servidor para operaciones CRUD y gestión de puntos de control.
/// </summary>
public class CursoService
{
    private readonly HttpClient _httpClient;
    private readonly AuthTokenService _authTokenService;

    /// <summary>
    /// Inicializa una nueva instancia del servicio de cursos.
    /// </summary>
    /// <param name="httpClient">Cliente HTTP para realizar las peticiones al servidor.</param>
    /// <param name="authTokenService">Servicio de autenticación para obtener el token de usuario.</param>
    public CursoService(HttpClient httpClient, AuthTokenService authTokenService)
    {
        _httpClient = httpClient;
        _authTokenService = authTokenService;
    }

    /// <summary>
    /// Obtiene todos los cursos de un usuario específico.
    /// </summary>
    /// <param name="usuarioId">Identificador del usuario.</param>
    /// <returns>Lista de cursos del usuario. Si ocurre un error, devuelve una lista vacía.</returns>
    public async Task<List<CursoDTO>> ObtenerCursosPorUsuarioAsync(string usuarioId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/cursos/usuario/{usuarioId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<CursoDTO>>() ?? new List<CursoDTO>();
            }
            return new List<CursoDTO>();
        }
        catch (Exception)
        {
            return new List<CursoDTO>();
        }
    }

    /// <summary>
    /// Obtiene un curso específico por su identificador.
    /// </summary>
    /// <param name="id">Identificador del curso.</param>
    /// <returns>El curso encontrado o null si no existe o hay un error.</returns>
    public async Task<CursoDTO?> ObtenerCursoPorIdAsync(long id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/cursos/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CursoDTO>();
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Crea un nuevo curso en el sistema.
    /// </summary>
    /// <param name="cursoDTO">Datos del curso a crear.</param>
    /// <returns>True si la creación fue exitosa, False en caso contrario.</returns>
    /// <remarks>
    /// El método realiza una conversión del DTO a la entidad Curso antes de enviarlo al servidor.
    /// Incluye logs para diagnóstico de la operación.
    /// </remarks>
    public async Task<bool> CrearCursoAsync(CursoDTO cursoDTO)
    {
        try
        {
            // Convertir DTO a Curso
            var curso = Curso.FromDTO(cursoDTO);

            // Serializar para debug
            var jsonString = System.Text.Json.JsonSerializer.Serialize(curso, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            System.Diagnostics.Debug.WriteLine("JSON a enviar:");
            System.Diagnostics.Debug.WriteLine(jsonString);

            // Realizar la petición
            var response = await _httpClient.PostAsJsonAsync($"/cursos/crear?usuarioId={cursoDTO.UsuarioId}", curso);

            // Mostrar la respuesta
            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Código de respuesta: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Respuesta del servidor: {responseContent}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al crear curso: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Actualiza un curso existente.
    /// </summary>
    /// <param name="curso">Datos actualizados del curso.</param>
    /// <returns>True si la actualización fue exitosa, False en caso contrario.</returns>
    /// <remarks>
    /// Solo actualiza los campos modificables del curso: nombre, enlace, precio, finalizado y anotaciones.
    /// </remarks>
    public async Task<bool> ActualizarCursoAsync(CursoDTO curso)
    {
        try
        {
            var cursoParaActualizar = new
            {
                nombre = curso.Nombre,
                enlace = curso.Enlace,
                precio = curso.Precio,
                finalizado = curso.Finalizado,
                anotaciones = curso.Anotaciones
            };

            // Debug
            var jsonString = System.Text.Json.JsonSerializer.Serialize(cursoParaActualizar,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            System.Diagnostics.Debug.WriteLine($"Actualizando curso {curso.Id}:");
            System.Diagnostics.Debug.WriteLine(jsonString);

            var response = await _httpClient.PatchAsJsonAsync($"/cursos/{curso.Id}", cursoParaActualizar);

            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Respuesta: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine(responseContent);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al actualizar curso: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Elimina un curso por su identificador.
    /// </summary>
    /// <param name="id">Identificador del curso a eliminar.</param>
    /// <returns>True si la eliminación fue exitosa, False en caso contrario.</returns>
    public async Task<bool> EliminarCursoAsync(long id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/cursos/{id}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Respuesta al eliminar curso {id}: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Contenido: {responseContent}");
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al eliminar curso: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtiene todos los puntos de control asociados a un curso.
    /// </summary>
    /// <param name="cursoId">Identificador del curso.</param>
    /// <returns>Lista de puntos de control del curso. Si ocurre un error, devuelve una lista vacía.</returns>
    public async Task<List<PuntoDeControlDTO>> ObtenerPuntosDeControlPorCursoAsync(long cursoId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/puntos-de-control/curso/{cursoId}");
            if (response.IsSuccessStatusCode)
            {
                // Obtener el JSON como string primero
                var jsonString = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON recibido para puntos de control del curso {cursoId}:");
                System.Diagnostics.Debug.WriteLine(jsonString);

                // Deserializar el JSON a la lista de puntos de control
                var puntosDeControl = await response.Content.ReadFromJsonAsync<List<PuntoDeControlDTO>>()
                    ?? new List<PuntoDeControlDTO>();

                // Mostrar cantidad de puntos recibidos
                System.Diagnostics.Debug.WriteLine($"Total de puntos de control recibidos: {puntosDeControl.Count}");

                return puntosDeControl;
            }
            System.Diagnostics.Debug.WriteLine($"Error al obtener puntos de control: {response.StatusCode}");
            return new List<PuntoDeControlDTO>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al obtener puntos de control: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            return new List<PuntoDeControlDTO>();
        }
    }

    /// <summary>
    /// Crea un nuevo punto de control para un curso.
    /// </summary>
    /// <param name="punto">Datos del punto de control a crear.</param>
    /// <param name="cursoId">Identificador del curso al que pertenecerá.</param>
    /// <returns>True si la creación fue exitosa, False en caso contrario.</returns>
    public async Task<bool> CrearPuntoDeControlAsync(PuntoDeControlDTO punto, long cursoId)
    {
        try
        {
            // Serializar para ver qué estamos enviando
            var jsonString = System.Text.Json.JsonSerializer.Serialize(punto, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            System.Diagnostics.Debug.WriteLine("JSON a enviar en CrearPuntoDeControlAsync:");
            System.Diagnostics.Debug.WriteLine(jsonString);
            System.Diagnostics.Debug.WriteLine($"cursoId: {cursoId}");

            var response = await _httpClient.PostAsJsonAsync($"/puntos-de-control/crear?cursoId={cursoId}", punto);

            // Ver la respuesta del servidor
            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Código de respuesta: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Respuesta del servidor: {responseContent}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al crear punto de control: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Actualiza un punto de control existente.
    /// </summary>
    /// <param name="id">Identificador del punto de control.</param>
    /// <param name="punto">Datos actualizados del punto de control.</param>
    /// <returns>True si la actualización fue exitosa, False en caso contrario.</returns>
    public async Task<bool> ActualizarPuntoDeControlAsync(long id, PuntoDeControlDTO punto)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"/puntos-de-control/{id}", punto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al actualizar punto de control: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Elimina un punto de control.
    /// </summary>
    /// <param name="id">Identificador del punto de control a eliminar.</param>
    /// <returns>True si la eliminación fue exitosa, False en caso contrario.</returns>
    public async Task<bool> EliminarPuntoDeControlAsync(long id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/puntos-de-control/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al eliminar punto de control: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Marca un punto de control como completado o pendiente.
    /// </summary>
    /// <param name="id">Identificador del punto de control.</param>
    /// <param name="completado">True para marcar como completado, False para marcar como pendiente.</param>
    /// <returns>True si la operación fue exitosa, False en caso contrario.</returns>
    /// <remarks>
    /// Este método utiliza una operación PATCH para actualizar solo el estado de completado.
    /// </remarks>
    public async Task<bool> MarcarPuntoDeControlAsync(long? id, bool completado)
    {
        try
        {
            if (!id.HasValue) return false;

            System.Diagnostics.Debug.WriteLine($"Llamando a PATCH /puntos-de-control/{id}/completado?completado={completado}");

            var response = await _httpClient.PatchAsync(
                $"/puntos-de-control/{id}/completado?completado={completado}",
                null);

            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Respuesta del servidor: {responseContent}");
            System.Diagnostics.Debug.WriteLine($"Código de estado: {response.StatusCode}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al marcar punto de control: {ex.Message}");
            return false;
        }
    }
}
