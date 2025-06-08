using System.Text.Json.Serialization;

namespace TuCurso.Models
{
    /// <summary>
    /// Representa una solicitud de amistad entre dos usuarios.
    /// </summary>
    public class SolicitudAmistad
    {
        /// <summary>
        /// Identificador único de la solicitud de amistad.
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// Usuario que envía la solicitud de amistad.
        /// </summary>
        [JsonPropertyName("emisor")]
        public UsuarioDTO Emisor { get; set; } = new();

        /// <summary>
        /// Usuario que recibe la solicitud de amistad.
        /// </summary>
        [JsonPropertyName("receptor")]
        public UsuarioDTO Receptor { get; set; } = new();

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public SolicitudAmistad() { }

        /// <summary>
        /// Constructor que inicializa una nueva solicitud con emisor y receptor.
        /// </summary>
        /// <param name="emisor">Usuario que envía la solicitud.</param>
        /// <param name="receptor">Usuario que recibe la solicitud.</param>
        public SolicitudAmistad(UsuarioDTO emisor, UsuarioDTO receptor)
        {
            Emisor = emisor;
            Receptor = receptor;
        }

        /// <summary>
        /// Compara dos solicitudes de amistad por su ID.
        /// </summary>
        /// <param name="obj">Objeto a comparar.</param>
        /// <returns>True si las solicitudes tienen el mismo ID, false en caso contrario.</returns>
        public override bool Equals(object? obj)
        {
            if (obj == this) return true;
            if (obj == null || obj.GetType() != GetType()) return false;
            var other = (SolicitudAmistad)obj;
            return Id != 0 && Id == other.Id;
        }

        /// <summary>
        /// Genera un código hash basado en el ID de la solicitud.
        /// </summary>
        /// <returns>Hash code de la solicitud.</returns>
        public override int GetHashCode()
        {
            return Id != 0 ? Id.GetHashCode() : 0;
        }
    }
}
