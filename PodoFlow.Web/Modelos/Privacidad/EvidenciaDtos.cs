using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Web.Modelos.Privacidad
{
    public class EvidenciaDto
    {
        public long Id { get; set; }

        public long SolicitudId { get; set; }

        public long UsuarioId { get; set; }

        public string Tipo { get; set; } = "";

        public string RutaArchivo { get; set; } = "";

        public string? Huella { get; set; }

        public DateTime FechaCarga { get; set; }
    }

    public class EvidenciaGuardarDto
    {
        [Required(ErrorMessage = "Ingresa el tipo de evidencia.")]
        [StringLength(80)]
        public string Tipo { get; set; } = "IDENTIFICACION";

        [Required(ErrorMessage = "Ingresa la ruta del archivo de evidencia.")]
        [StringLength(1000)]
        public string RutaArchivo { get; set; } = "";

        [StringLength(128)]
        public string? Huella { get; set; }

        [StringLength(2000)]
        public string? Observacion { get; set; }
    }
}
