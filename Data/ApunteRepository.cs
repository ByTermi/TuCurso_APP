using SQLite;
using TuCurso.Models;

namespace TuCurso.Data
{
    /// <summary>
    /// Implementación de IApunteRepository que gestiona las operaciones de base de datos para los apuntes
    /// utilizando SQLite como sistema de almacenamiento local.
    /// </summary>
    public class ApunteRepository : IApunteRepository
    {
        /// <summary>
        /// Conexión asíncrona a la base de datos SQLite.
        /// </summary>
        private readonly SQLiteAsyncConnection _database;

        /// <summary>
        /// Inicializa una nueva instancia del repositorio de apuntes.
        /// </summary>
        /// <param name="dbPath">Ruta completa al archivo de la base de datos SQLite.</param>
        public ApunteRepository(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Apunte>().Wait();
        }

        /// <summary>
        /// Obtiene todos los apuntes ordenados por fecha de creación descendente.
        /// </summary>
        /// <returns>Lista de apuntes ordenada del más reciente al más antiguo.</returns>
        public async Task<List<Apunte>> GetAllApuntesAsync()
        {
            return await _database.Table<Apunte>()
                                .OrderByDescending(a => a.FechaCreacion)
                                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un apunte específico por su ID.
        /// </summary>
        /// <param name="id">ID del apunte a buscar.</param>
        /// <returns>El apunte encontrado o null si no existe.</returns>
        public async Task<Apunte> GetApunteAsync(int id)
        {
            return await _database.Table<Apunte>()
                                .Where(a => a.Id == id)
                                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Guarda un nuevo apunte en la base de datos.
        /// </summary>
        /// <param name="apunte">Apunte a guardar.</param>
        /// <returns>El número de filas afectadas.</returns>
        public async Task<int> SaveApunteAsync(Apunte apunte)
        {
            apunte.FechaCreacion = DateTime.Now;
            return await _database.InsertAsync(apunte);
        }

        /// <summary>
        /// Elimina un apunte de la base de datos.
        /// </summary>
        /// <param name="apunte">Apunte a eliminar.</param>
        /// <returns>El número de filas afectadas.</returns>
        public async Task<int> DeleteApunteAsync(Apunte apunte)
        {
            return await _database.DeleteAsync(apunte);
        }

        /// <summary>
        /// Actualiza un apunte existente en la base de datos.
        /// </summary>
        /// <param name="apunte">Apunte con los datos actualizados.</param>
        /// <returns>El número de filas afectadas.</returns>
        public async Task<int> UpdateApunteAsync(Apunte apunte)
        {
            return await _database.UpdateAsync(apunte);
        }
    }
}
