using SQLite;
using System.ComponentModel.DataAnnotations;

namespace TuCurso.Models
{
    /// <summary>
    /// Representa un apunte o nota almacenada localmente en la aplicación.
    /// </summary>
    public class Apunte
    {
        /// <summary>
        /// Identificador único del apunte en la base de datos local.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Nombre o título del apunte.
        /// </summary>
        [Required]
        public string Nombre { get; set; }

        /// <summary>
        /// Descripción opcional del apunte.
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Ruta del archivo en el sistema de archivos local.
        /// </summary>
        [Required]
        public string RutaArchivo { get; set; }

        /// <summary>
        /// Indica si el archivo físico referenciado por RutaArchivo existe.
        /// </summary>
        [Ignore]
        public bool ArchivoExiste => File.Exists(RutaArchivo);

        /// <summary>
        /// Fecha y hora de creación del apunte.
        /// </summary>
        public DateTime FechaCreacion { get; set; }
    }
}
