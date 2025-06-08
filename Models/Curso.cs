using System.Text.Json.Serialization;

namespace TuCurso.Models;

/// <summary>
/// Representa un curso en la aplicación, con sus propiedades y métodos de conversión DTO.
/// </summary>
public class Curso
{
    /// <summary>
    /// Identificador único del curso.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Nombre o título del curso.
    /// </summary>
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Enlace o URL del curso.
    /// </summary>
    [JsonPropertyName("enlace")]
    public string Enlace { get; set; } = string.Empty;

    /// <summary>
    /// Precio del curso.
    /// </summary>
    [JsonPropertyName("precio")]
    public double Precio { get; set; }

    /// <summary>
    /// Indica si el curso ha sido completado.
    /// </summary>
    [JsonPropertyName("finalizado")]
    public bool Finalizado { get; set; }

    /// <summary>
    /// Notas o comentarios adicionales sobre el curso.
    /// </summary>
    [JsonPropertyName("anotaciones")]
    public string Anotaciones { get; set; } = string.Empty;

    /// <summary>
    /// Constructor por defecto.
    /// </summary>
    public Curso() { }

    /// <summary>
    /// Constructor que inicializa todas las propiedades principales del curso.
    /// </summary>
    /// <param name="nombre">Nombre del curso.</param>
    /// <param name="enlace">URL o enlace al curso.</param>
    /// <param name="precio">Precio del curso.</param>
    /// <param name="finalizado">Estado de finalización del curso.</param>
    /// <param name="anotaciones">Notas adicionales sobre el curso.</param>
    public Curso(string nombre, string enlace, double precio, bool finalizado, string anotaciones)
    {
        Nombre = nombre;
        Enlace = enlace;
        Precio = precio;
        Finalizado = finalizado;
        Anotaciones = anotaciones;
    }

    /// <summary>
    /// Crea una instancia de Curso a partir de un DTO.
    /// </summary>
    /// <param name="dto">DTO con los datos del curso.</param>
    /// <returns>Nueva instancia de Curso con los datos del DTO.</returns>
    public static Curso FromDTO(CursoDTO dto)
    {
        return new Curso
        {
            Id = dto.Id,
            Nombre = dto.Nombre,
            Enlace = dto.Enlace,
            Precio = dto.Precio,
            Finalizado = dto.Finalizado,
            Anotaciones = dto.Anotaciones
        };
    }

    /// <summary>
    /// Convierte la instancia actual a un DTO.
    /// </summary>
    /// <returns>DTO con los datos del curso actual.</returns>
    public CursoDTO ToDTO()
    {
        return new CursoDTO
        {
            Id = this.Id,
            Nombre = this.Nombre,
            Enlace = this.Enlace,
            Precio = this.Precio,
            Finalizado = this.Finalizado,
            Anotaciones = this.Anotaciones
        };
    }
}
