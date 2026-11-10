using System.Text.RegularExpressions;

namespace PodoFlow.Web.Modelos.Privacidad
{
    public class DocumentoDto
    {
        public long Id { get; set; }
        public long UsuarioId { get; set; }
        public string Tipo { get; set; } = "";
        public string? Version { get; set; }
        public string Nombre { get; set; } = "";
        public string RutaArchivo { get; set; } = "";
        public string? Huella { get; set; }
        public DateTime FechaCarga { get; set; }
        public bool Estatus { get; set; }

        public static readonly IReadOnlyDictionary<string, string> Tipos = new Dictionary<string, string>
        {
            ["RAT"] = "Registro de actividades (RAT)",
            ["EIPD"] = "Evaluación de impacto (EIPD)",
            ["MATRIZ_RIESGO"] = "Matriz de riesgos",
            ["MATRIZ_ENCARGADO"] = "Matriz de encargados",
            ["EVIDENCIA_CONTROL"] = "Evidencia de control",
            ["AVISO_PRIVACIDAD"] = "Aviso de privacidad",
            ["OTRO"] = "Otro"
        };

        public static string NombreTipo(string tipo) =>
            Tipos.TryGetValue(tipo, out var nombre) ? nombre : tipo;
    }

    public class DocumentoGuardarDto
    {
        private static readonly Regex HuellaValida = new(@"^[0-9a-fA-F]{8,128}$", RegexOptions.Compiled);

        public string Tipo { get; set; } = "";
        public string? Version { get; set; }
        public string Nombre { get; set; } = "";
        public string RutaArchivo { get; set; } = "";
        public string? Huella { get; set; }

        public static DocumentoGuardarDto Desde(DocumentoDto documento) => new()
        {
            Tipo = documento.Tipo,
            Version = documento.Version,
            Nombre = documento.Nombre,
            RutaArchivo = documento.RutaArchivo,
            Huella = documento.Huella
        };

        public string? Validar(bool esNuevo)
        {
            if (!DocumentoDto.Tipos.ContainsKey(Tipo))
                return "Selecciona el tipo de documento.";

            if (string.IsNullOrWhiteSpace(Nombre))
                return "Ingresa el nombre del documento.";

            if (Nombre.Trim().Length > 255)
                return "El nombre no puede pasar de 255 caracteres.";

            if (Version?.Trim().Length > 30)
                return "La versión no puede pasar de 30 caracteres.";

            if (!esNuevo)
                return null;

            if (string.IsNullOrWhiteSpace(RutaArchivo))
                return "Indica dónde está guardado el archivo.";

            if (RutaArchivo.Trim().Length > 1000)
                return "La ruta no puede pasar de 1000 caracteres.";

            if (!string.IsNullOrWhiteSpace(Huella) && !HuellaValida.IsMatch(Huella.Trim()))
                return "La huella debe ser hexadecimal (SHA-256: 64 caracteres).";

            return null;
        }
    }
}
