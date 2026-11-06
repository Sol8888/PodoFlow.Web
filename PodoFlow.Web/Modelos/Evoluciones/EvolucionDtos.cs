
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Web.Modelos.Evoluciones;

public class EvolucionDto
{
    public long Id { get; set; }
    public long AtencionId { get; set; }
    public long UsuarioId { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; } = "";
    public bool Estatus { get; set; }
}

public class EvolucionCrearDto
{
    [Range(1, long.MaxValue)]
    public long UsuarioId { get; set; }

    [Required(ErrorMessage = "Escribe la evolución clínica.")]
    [StringLength(3000, ErrorMessage = "Máximo 3000 caracteres.")]
    public string Descripcion { get; set; } = "";
}

