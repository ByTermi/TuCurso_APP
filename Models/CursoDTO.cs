namespace TuCurso.Models;

public class CursoDTO
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Enlace { get; set; } = string.Empty;
    public double Precio { get; set; }
    public bool Finalizado { get; set; }
    public string Anotaciones { get; set; } = string.Empty;
    public long UsuarioId { get; set; }
}

