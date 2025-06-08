using System.Globalization;

namespace TuCurso.Models;

public class LoginResult
{
    public bool Success { get; set; } = false; // Valor predeterminado para evitar null

    public string? Id { get; set; } = string.Empty; // ID
    public string? Token { get; set; } = string.Empty; // Inicializado como cadena vacía
    public string? Expiration { get; set; } = string.Empty; // Cambiado a string para coincidir con la API
    public string? ErrorMessage { get; set; } = string.Empty; // Inicializado como cadena vacía
    

    public DateTime? GetExpirationAsDateTime()
    {
        if (string.IsNullOrEmpty(Expiration))
            return null;

        // Formatos de fecha que intentaremos parsear
        string[] formats = {
            "ddd MMM dd HH:mm:ss 'CEST' yyyy", // Formato de la API: "Sat May 03 18:30:12 CEST 2025"
            "ddd MMM dd HH:mm:ss yyyy",         // Formato alternativo sin zona horaria
            "yyyy-MM-dd'T'HH:mm:ss",           // ISO format
            "O"                                 // Round-trip format
        };

        // Intentar parsear con los formatos definidos
        if (DateTime.TryParseExact(
            Expiration,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime result))
        {
            return result;
        }

        // Si ningún formato funciona, intentar un parse general
        if (DateTime.TryParse(
            Expiration,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out result))
        {
            return result;
        }

        return null;
    }
}
