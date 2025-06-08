using System.Text.Json.Serialization;

namespace TuCurso.Models
{
    /// <summary>
    /// DTO para la transferencia de información de usuarios entre la aplicación y el servidor.
    /// </summary>
    public class UsuarioDTO
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción o biografía del usuario.
        /// </summary>
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// URL o identificador del icono del usuario.
        /// </summary>
        [JsonPropertyName("icono")]
        public string Icono { get; set; } = string.Empty;

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public UsuarioDTO() { }

        /// <summary>
        /// Constructor que inicializa todas las propiedades del usuario.
        /// </summary>
        public UsuarioDTO(long id, string nombre, string descripcion, string icono)
        {
            Id = id;
            Nombre = nombre;
            Descripcion = descripcion;
            Icono = icono;
        }

        /// <summary>
        /// Constructor que crea un DTO a partir de una entidad Usuario.
        /// </summary>
        /// <param name="usuario">Entidad Usuario a convertir en DTO.</param>
        public UsuarioDTO(Usuario usuario)
        {
            Id = usuario.Id;
            Nombre = usuario.Nombre;
            Descripcion = usuario.Descripcion;
            Icono = usuario.Icono;
        }
    }
}
