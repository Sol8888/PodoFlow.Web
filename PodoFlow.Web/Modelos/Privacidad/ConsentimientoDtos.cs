namespace PodoFlow.Web.Modelos.Privacidad
{
    public class FinalidadDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public bool RequiereConsentimiento { get; set; }
        public bool Estatus { get; set; }
    }

    public class AvisoDto
    {
        public int Id { get; set; }
        public string Version { get; set; } = "";
        public DateOnly FechaDesde { get; set; }
        public DateOnly? FechaHasta { get; set; }
        public bool Estatus { get; set; }

        public bool EsVigente(DateOnly hoy) =>
            Estatus && FechaDesde <= hoy && (!FechaHasta.HasValue || FechaHasta.Value >= hoy);
    }

    public class ConsentimientoCrearDto
    {
        public long PacienteId { get; set; }
        public int FinalidadId { get; set; }
        public int AvisoId { get; set; }
        public string Estado { get; set; } = "";
        public string Medio { get; set; } = "";
        public string? Observacion { get; set; }
        public long? UsuarioId { get; set; }
    }

    public class ConsentimientoDto
    {
        public long Id { get; set; }
        public long PacienteId { get; set; }
        public int FinalidadId { get; set; }
        public int AvisoId { get; set; }
        public string Estado { get; set; } = "";
        public string Medio { get; set; } = "";
        public DateTime FechaObtencion { get; set; }
        public DateTime? FechaRevocacion { get; set; }
        public string? Observacion { get; set; }
        public string Version { get; set; } = "";
    }

    public class EventoConsentimientoDto
    {
        public long Id { get; set; }
        public string Estado { get; set; } = "";
        public DateTime FechaHora { get; set; }
        public string Medio { get; set; } = "";
        public string? Observacion { get; set; }
    }

    public class ConsentimientoCambiarEstadoDto
    {
        public string Estado { get; set; } = "";
        public string Medio { get; set; } = "";
        public int? AvisoId { get; set; }
        public string? Observacion { get; set; }
        public long? UsuarioId { get; set; }
        public string Version { get; set; } = "";
    }

    // Lo que se elige en AutorizacionCampos, tanto al crear el paciente como en Privacidad.
    public class AutorizacionModelo
    {
        public bool EsRepresentante { get; set; }
        public string NombreRepresentante { get; set; } = "";
        public string RelacionRepresentante { get; set; } = "";
        public string Medio { get; set; } = OpcionesConsentimiento.MedioPorDefecto;

        // La identificación del representante no se guarda aquí porque la observación
        // sale en las exportaciones que recibe el paciente.
        public string ComponerObservacion(string? nota = null)
        {
            var autoriza = EsRepresentante
                ? $"Autorizado por representante: {NombreRepresentante.Trim()} ({RelacionRepresentante})"
                : "Autorizado por el titular";

            return string.IsNullOrWhiteSpace(nota) ? autoriza : $"{autoriza}. {nota.Trim()}";
        }
    }

    public static class OpcionesConsentimiento
    {
        public const string MedioPorDefecto = "PRESENCIAL";

        public static readonly (string Codigo, string Nombre)[] Medios =
        [
            ("PRESENCIAL", "Presencial"),
            ("CORREO", "Correo"),
            ("WHATSAPP", "WhatsApp"),
            ("TELEFONO", "Teléfono")
        ];

        public static readonly string[] Relaciones =
        [
            "Madre o padre",
            "Tutor o curador",
            "Cónyuge o pareja",
            "Hijo o hija",
            "Apoderado",
            "Otro"
        ];

        public static string NombreMedio(string codigo)
        {
            foreach (var medio in Medios)
            {
                if (medio.Codigo == codigo)
                    return medio.Nombre;
            }

            return codigo;
        }

        public static bool EsMenorDeEdad(DateTime? nacimiento) =>
            nacimiento.HasValue && nacimiento.Value.Date.AddYears(18) > DateTime.Today;
    }
}
