using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Web.Modelos.Privacidad
{
    public class SolicitudDto
    {
        private static readonly HashSet<string> EstadosFinales = ["CERRADA", "RECHAZADA"];
        private static readonly HashSet<string> DerechosConExportacion = ["ACCESO", "PORTABILIDAD"];

        public long Id { get; set; }

        public long PacienteId { get; set; }

        public long? UsuarioId { get; set; }

        public string TipoDerecho { get; set; } = "";

        public string TipoSolicitante { get; set; } = "";

        public string NombreSolicitante { get; set; } = "";

        public string IdentificacionSolicitante { get; set; } = "";

        public DateTime FechaRecepcion { get; set; }

        public string Estado { get; set; } = "";

        public string? MedioRespuesta { get; set; }

        public string? Detalle { get; set; }

        public string? Respuesta { get; set; }

        public DateTime? FechaCierre { get; set; }

        public DateTime FechaLimite { get; set; }

        public int? DiasRestantes { get; set; }

        public bool Vencida { get; set; }

        public string Version { get; set; } = "";

        public bool EsFinal => EstadosFinales.Contains(Estado);

        public bool PermiteExportacion => DerechosConExportacion.Contains(TipoDerecho);

        public string ClaseEstado => Estado switch
        {
            "RECIBIDA" or "VALIDANDO" => "info",
            "EN_PROCESO" => "warning",
            "RESPONDIDA" or "CERRADA" => "success",
            "RECHAZADA" => "danger",
            _ => "info"
        };
    }

    public class AccionSolicitudDto
    {
        public long Id { get; set; }

        public long SolicitudId { get; set; }

        public long UsuarioId { get; set; }

        public DateTime FechaHora { get; set; }

        public string? EstadoAnterior { get; set; }

        public string? EstadoNuevo { get; set; }

        public string Accion { get; set; } = "";

        public string? Observacion { get; set; }
    }

    public class SolicitudGuardarDto
    {
        [Range(1, long.MaxValue, ErrorMessage = "Selecciona el paciente titular de los datos.")]
        public long PacienteId { get; set; }

        [Required(ErrorMessage = "Selecciona el derecho que se ejerce.")]
        public string TipoDerecho { get; set; } = "ACCESO";

        [Required]
        public string TipoSolicitante { get; set; } = "TITULAR";

        [Required(ErrorMessage = "Ingresa el nombre del solicitante.")]
        [StringLength(200)]
        public string NombreSolicitante { get; set; } = "";

        public string? TipoIdentificacion { get; set; } = "CEDULA";

        [Required(ErrorMessage = "Ingresa la identificación del solicitante.")]
        [StringLength(50)]
        public string IdentificacionSolicitante { get; set; } = "";

        [StringLength(50)]
        public string? MedioRespuesta { get; set; }

        [StringLength(3000)]
        public string? Detalle { get; set; }
    }
}
