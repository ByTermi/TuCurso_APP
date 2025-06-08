using TuCurso.Models;

namespace TuCurso.Data
{
    /// <summary>
    /// Interfaz que define las operaciones CRUD para la gestión de apuntes en la base de datos local.
    /// </summary>
    public interface IApunteRepository
    {
        /// <summary>
        /// Obtiene todos los apuntes almacenados ordenados por fecha de creación.
        /// </summary>
        /// <returns>Una lista de apuntes ordenada por fecha de creación descendente.</returns>
        Task<List<Apunte>> GetAllApuntesAsync();

        /// <summary>
        /// Obtiene un apunte específico por su identificador.
        /// </summary>
        /// <param name="id">El identificador único del apunte.</param>
        /// <returns>El apunte encontrado o null si no existe.</returns>
        Task<Apunte> GetApunteAsync(int id);

        /// <summary>
        /// Guarda un nuevo apunte en la base de datos.
        /// </summary>
        /// <param name="apunte">El apunte a guardar.</param>
        /// <returns>El número de filas afectadas (1 si fue exitoso, 0 si falló).</returns>
        Task<int> SaveApunteAsync(Apunte apunte);

        /// <summary>
        /// Elimina un apunte de la base de datos.
        /// </summary>
        /// <param name="apunte">El apunte a eliminar.</param>
        /// <returns>El número de filas afectadas (1 si fue exitoso, 0 si falló).</returns>
        Task<int> DeleteApunteAsync(Apunte apunte);

        /// <summary>
        /// Actualiza un apunte existente en la base de datos.
        /// </summary>
        /// <param name="apunte">El apunte con los datos actualizados.</param>
        /// <returns>El número de filas afectadas (1 si fue exitoso, 0 si falló).</returns>
        Task<int> UpdateApunteAsync(Apunte apunte);
    }
}
