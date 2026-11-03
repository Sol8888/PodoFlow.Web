
namespace PodoFlow.Web.Modelos.Pacientes;

public class PrestacionPacienteDto
{
    public long Id { get; set; }

    public long PacienteId { get; set; }

    public long? CitaId { get; set; }

    public long? AtencionId { get; set; }

    public int ServicioId { get; set; }

    public DateTime FechaPrestacion { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = "";
}
