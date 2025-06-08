using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TuCurso.Models
{
    /// <summary>
    /// DTO para la transferencia de información de puntos de control entre la aplicación y el servidor.
    /// </summary>
    public class PuntoDeControlDTO
    {
        /// <summary>
        /// Identificador único del punto de control. Puede ser nulo para nuevos puntos.
        /// </summary>
        public long? Id { get; set; } = null;

        /// <summary>
        /// Descripción o contenido del punto de control.
        /// </summary>
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el punto de control ha sido completado.
        /// </summary>
        [JsonPropertyName("estaCompletado")]
        public bool Completado { get; set; }

        /// <summary>
        /// Fecha objetivo para completar el punto de control.
        /// </summary>
        [JsonPropertyName("fechaFinalizacionDeseada")]
        public DateTime? FechaFinalizacionDeseada { get; set; }

        /// <summary>
        /// Identificador del curso al que pertenece este punto de control.
        /// </summary>
        [JsonPropertyName("cursoId")]
        public long CursoId { get; set; }
    }
}
