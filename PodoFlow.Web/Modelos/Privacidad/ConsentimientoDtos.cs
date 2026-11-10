using System.Text.RegularExpressions;

namespace PodoFlow.Web.Modelos.Privacidad
{
    public class FinalidadDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public bool RequiereConsentimiento { get; set; }
        public bool Estatus { get; set; }
    }

    public class FinalidadGuardarDto
    {
        private static readonly Regex CodigoValido = new(@"^[A-Z][A-Z0-9_]{1,49}$", RegexOptions.Compiled);

        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public bool RequiereConsentimiento { get; set; }

        public static FinalidadGuardarDto Desde(FinalidadDto finalidad) => new()
        {
            Codigo = finalidad.Codigo,
            Nombre = finalidad.Nombre,
            Descripcion = finalidad.Descripcion,
            RequiereConsentimiento = finalidad.RequiereConsentimiento
        };

        public string? Validar(bool esNueva)
        {
            if (esNueva && !CodigoValido.IsMatch(Codigo.Trim().ToUpperInvariant()))
                return "El código debe empezar con una letra y llevar solo letras, números o guion bajo (2 a 50).";

            if (string.IsNullOrWhiteSpace(Nombre))
                return "Ingresa el nombre de la finalidad.";

            if (Nombre.Trim().Length > 150)
                return "El nombre no puede pasar de 150 caracteres.";

            if (Descripcion?.Trim().Length > 1000)
                return "La descripción no puede pasar de 1000 caracteres.";

            return null;
        }
    }

    public class AvisoDto
    {
        public int Id { get; set; }
        public string Version { get; set; } = "";
        public DateOnly FechaDesde { get; set; }
        public DateOnly? FechaHasta { get; set; }
        public string? RutaArchivo { get; set; }
        public string? Huella { get; set; }
        public bool Estatus { get; set; }

        public bool EsVigente(DateOnly hoy) =>
            Estatus && FechaDesde <= hoy && (!FechaHasta.HasValue || FechaHasta.Value >= hoy);
    }

    public class AvisoGuardarDto
    {
        private static readonly Regex HuellaValida = new(@"^[0-9a-fA-F]{64}$", RegexOptions.Compiled);

        public string Version { get; set; } = "";
        public DateOnly FechaDesde { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public DateOnly? FechaHasta { get; set; }
        public string? RutaArchivo { get; set; }
        public string? Huella { get; set; }

        public static AvisoGuardarDto Desde(AvisoDto aviso) => new()
        {
            Version = aviso.Version,
            FechaDesde = aviso.FechaDesde,
            FechaHasta = aviso.FechaHasta,
            RutaArchivo = aviso.RutaArchivo,
            Huella = aviso.Huella
        };

        public string? Validar(bool esNuevo)
        {
            if (esNuevo && string.IsNullOrWhiteSpace(Version))
                return "Ingresa la versión del aviso.";

            if (esNuevo && Version.Trim().Length > 30)
                return "La versión no puede pasar de 30 caracteres.";

            if (FechaHasta.HasValue && FechaHasta.Value < FechaDesde)
                return "La fecha de fin no puede ser anterior a la de inicio.";

            if (RutaArchivo?.Trim().Length > 1000)
                return "La ruta del documento no puede pasar de 1000 caracteres.";

            if (!string.IsNullOrWhiteSpace(Huella) && !HuellaValida.IsMatch(Huella.Trim()))
                return "La huella debe ser un SHA-256: 64 caracteres hexadecimales.";

            return null;
        }
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
        public string? RepresentanteNombre { get; set; }
        public string? RepresentanteRelacion { get; set; }
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
        public string? RepresentanteNombre { get; set; }
        public string? RepresentanteRelacion { get; set; }
        public string Version { get; set; } = "";
    }

    public class AutorizacionModelo
    {
        private static readonly Regex NombreValido = new(@"^\p{L}[\p{L} .'-]*$", RegexOptions.Compiled);

        public bool EsRepresentante { get; set; }
        public string NombreRepresentante { get; set; } = "";
        public string RelacionRepresentante { get; set; } = "";
        public string Medio { get; set; } = OpcionesConsentimiento.MedioPorDefecto;

        public string? NombreParaEnviar => EsRepresentante
            ? string.Join(' ', NombreRepresentante.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            : null;

        public string? RelacionParaEnviar => EsRepresentante ? RelacionRepresentante : null;

        public string? ErrorNombre()
        {
            var nombre = NombreParaEnviar ?? "";

            if (nombre.Length == 0)
                return "Ingresa el nombre del representante.";

            if (!NombreValido.IsMatch(nombre) || nombre.Count(char.IsLetter) < 3)
                return "El nombre debe tener al menos 3 letras y no llevar números.";

            return null;
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
