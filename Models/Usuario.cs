namespace TuCurso.Models;

public class Usuario
{
    public long Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string Email { get; set; }
    public string Pass { get; set; }
    public string Icono { get; set; }
    public string Rol { get; set; }
}