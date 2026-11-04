
namespace PodoFlow.Web.Modelos.Pacientes;

public class HistoriaPacienteDto
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
