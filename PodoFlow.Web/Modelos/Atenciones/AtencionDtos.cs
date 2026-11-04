
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Web.Modelos.Atenciones;

public class AtencionDto
{
    public long Id { get; set; }
    public long HistoriaId { get; set; }
    public long? CitaId { get; set; }
    public long UsuarioId { get; set; }

    public DateTime Fecha { get; set; }

    public string? MotivoConsulta { get; set; }
    public string? ValoracionClinica { get; set; }
    public string? Diagnostico { get; set; }
    public string? Tratamiento { get; set; }
    public string? Observaciones { get; set; }

    public string Estado { get; set; } = "";

    public DateTime? FechaModificacion { get; set; }
    public string Version { get; set; } = "";
}


public class AtencionCrearDto
{
    [Range(1, long.MaxValue,
        ErrorMessage = "Ingresa un profesional responsable válido.")]
    public long UsuarioId { get; set; }

    [Required(ErrorMessage = "Ingresa el motivo de consulta.")]
    [StringLength(1000)]
    public string MotivoConsulta { get; set; } = "";

    [StringLength(3000)]
    public string? ValoracionClinica { get; set; }

    [StringLength(2000)]
    public string? Diagnostico { get; set; }

    [StringLength(3000)]
    public string? Tratamiento { get; set; }

    [StringLength(3000)]
    public string? Observaciones { get; set; }
}
