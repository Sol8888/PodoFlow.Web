
namespace PodoFlow.Web.Modelos.Pacientes;

public class CitaPacienteDto
{
    public long Id { get; set; }

    public long PacienteId { get; set; }

    public int ServicioId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public string Estado { get; set; } = "";

    public string Canal { get; set; } = "";
}
