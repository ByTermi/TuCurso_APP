namespace TuCurso.Services;

/// <summary>
/// Servicio que gestiona la autenticación y almacenamiento seguro de tokens en la aplicación.
/// Proporciona funcionalidad para manejar tokens JWT, datos de usuario y estado de autenticación.
/// </summary>
public class AuthTokenService
{
    // Campos privados para almacenar el estado
    private string? _token;
    private DateTime? _expiration;
    private string? _userId;
    private bool _isAuthenticated;

    /// <summary>
    /// Se dispara cuando cambia el estado de autenticación del usuario.
    /// </summary>
    public event EventHandler<bool>? AuthenticationChanged;

    /// <summary>
    /// Obtiene o establece el token JWT de autenticación.
    /// Al establecer un valor, se almacena automáticamente en SecureStorage.
    /// </summary>
    public string? Token
    {
        get => _token;
        set
        {
            _token = value;
            if (value != null)
            {
                SecureStorage.SetAsync("auth_token", value);
            }
            else
            {
                SecureStorage.Remove("auth_token");
            }
        }
    }

    /// <summary>
    /// Obtiene o establece la fecha de expiración del token.
    /// Al establecer un valor, se almacena automáticamente en SecureStorage.
    /// </summary>
    public DateTime? Expiration
    {
        get => _expiration;
        set
        {
            _expiration = value;
            if (value != null)
            {
                SecureStorage.SetAsync("token_expiration", value.Value.ToString("O"));
            }
            else
            {
                SecureStorage.Remove("token_expiration");
            }
        }
    }

    /// <summary>
    /// Obtiene o establece el ID del usuario autenticado.
    /// Al establecer un valor, se almacena automáticamente en SecureStorage.
    /// </summary>
    public string? UserId
    {
        get => _userId;
        set
        {
            _userId = value;
            if (value != null)
            {
                SecureStorage.SetAsync("user_id", value);
            }
            else
            {
                SecureStorage.Remove("user_id");
            }
        }
    }

    /// <summary>
    /// Obtiene o establece el estado de autenticación del usuario.
    /// Se considera autenticado si existe un token válido y no ha expirado.
    /// </summary>
    public bool IsAuthenticated
    {
        get => !string.IsNullOrEmpty(Token) && Expiration > DateTime.UtcNow;
        private set
        {
            if (_isAuthenticated != value)
            {
                _isAuthenticated = value;
                AuthenticationChanged?.Invoke(this, value);
            }
        }
    }
        
    /// <summary>
    /// Inicializa el servicio cargando los datos de autenticación almacenados.
    /// Debe llamarse al inicio de la aplicación.
    /// </summary>
    /// <returns>Tarea asíncrona que representa la operación de inicialización.</returns>
    public async Task InitializeAsync()
    {
        _token = await SecureStorage.GetAsync("auth_token");
        _userId = await SecureStorage.GetAsync("user_id");
        var expirationStr = await SecureStorage.GetAsync("token_expiration");
        if (DateTime.TryParse(expirationStr, out var expiration))
        {
            _expiration = expiration;
        }

        // Actualizar estado de autenticación
        SetAuthentication(IsAuthenticated);
    }

    /// <summary>
    /// Establece el estado de autenticación y notifica a los observadores.
    /// Si se establece como no autenticado, limpia todos los datos almacenados.
    /// </summary>
    /// <param name="isAuthenticated">Estado de autenticación a establecer.</param>
    public void SetAuthentication(bool isAuthenticated)
    {
        if (!isAuthenticated)
        {
            // Evitamos la recursión infinita no llamando a Clear()
            _token = null;
            _expiration = null;
            _userId = null;

            // Limpiamos el almacenamiento seguro directamente
            SecureStorage.Remove("auth_token");
            SecureStorage.Remove("token_expiration");
            SecureStorage.Remove("user_id");
        }

        if (_isAuthenticated != isAuthenticated)
        {
            _isAuthenticated = isAuthenticated;
            AuthenticationChanged?.Invoke(this, isAuthenticated);
        }
    }

    /// <summary>
    /// Limpia todos los datos de autenticación almacenados.
    /// Equivalente a cerrar sesión.
    /// </summary>
    public void Clear()
    {
        SetAuthentication(false);
    }

    /// <summary>
    /// Obtiene el ID del usuario autenticado actual.
    /// </summary>
    /// <returns>ID del usuario o cadena vacía si no hay usuario autenticado.</returns>
    public string GetUserId()
    {
        return _userId ?? string.Empty;
    }
}