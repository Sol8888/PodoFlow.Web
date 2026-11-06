namespace PodoFlow.Web.Modelos.Atenciones;

public class AdjuntoDto
{
    public long Id { get; set; }
    public long AtencionId { get; set; }
    public long UsuarioId { get; set; }
    public string Nombre { get; set; } = "";
    public string? TipoMime { get; set; }
    public long? TamanoBytes { get; set; }
    public DateTime FechaCarga { get; set; }
    public bool Estatus { get; set; }
}

public sealed record AdjuntoArchivo(
    byte[] Contenido,
    string TipoMime,
    string Nombre);
