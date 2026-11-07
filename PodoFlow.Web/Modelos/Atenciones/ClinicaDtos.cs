namespace PodoFlow.Web.Modelos.Atenciones;

public class AtencionEditarDto
{
    public string? MotivoConsulta { get; set; }
    public string? ValoracionClinica { get; set; }
    public string? Diagnostico { get; set; }
    public string? Tratamiento { get; set; }
    public string? Observaciones { get; set; }
    public string Version { get; set; } = "";
}

public class HistoriaResumenDto
{
    public long Id { get; set; }
    public long PacienteId { get; set; }
    public long? UsuarioId { get; set; }
    public DateTime FechaApertura { get; set; }
    public string Estado { get; set; } = "";
    public DateTime? FechaBloqueo { get; set; }
    public string? MotivoBloqueo { get; set; }
    public string Version { get; set; } = "";
}
