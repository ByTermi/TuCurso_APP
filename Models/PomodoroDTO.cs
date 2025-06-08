using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TuCurso.Models
{
    /// <summary>
    /// DTO que representa una sesión de Pomodoro para la gestión del tiempo.
    /// </summary>
    public class PomodoroDTO
    {
        /// <summary>
        /// Identificador único del pomodoro. Puede ser nulo para nuevas sesiones.
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// Fecha y hora de inicio de la sesión.
        /// </summary>
        [JsonPropertyName("fechaHoraInicial")]
        public DateTime FechaHoraInicial { get; set; }

        /// <summary>
        /// Fecha y hora de finalización prevista de la sesión.
        /// </summary>
        [JsonPropertyName("fechaHoraDestino")]
        public DateTime FechaHoraDestino { get; set; }

        /// <summary>
        /// Identificador del usuario que creó la sesión.
        /// </summary>
        [JsonPropertyName("usuarioId")]
        public long UsuarioId { get; set; }

        /// <summary>
        /// Duración total de la sesión en formato "hh:mm:ss".
        /// </summary>
        [JsonIgnore]
        public string Duracion
        {
            get
            {
                var diferencia = FechaHoraDestino - FechaHoraInicial;
                return $"{diferencia.Hours:00} horas {diferencia.Minutes:00} minutos y {diferencia.Seconds:00} segundos";
            }
        }
    }
}
