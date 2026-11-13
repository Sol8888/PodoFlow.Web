namespace PodoFlow.Web.Modelos.Privacidad
{
    public class DocumentoDto
    {
        public const long MaximoBytes = 10 * 1024 * 1024;

        public const string Formatos = ".pdf,.docx,.xlsx";

        public long Id { get; set; }
        public long UsuarioId { get; set; }
        public string Tipo { get; set; } = "";
        public string? Version { get; set; }
        public string Nombre { get; set; } = "";
        public string RutaArchivo { get; set; } = "";
        public string? Huella { get; set; }
        public DateTime FechaCarga { get; set; }
        public bool Estatus { get; set; }
        public bool TieneArchivo { get; set; }

        public string Formato => Path.GetExtension(RutaArchivo).TrimStart('.').ToUpperInvariant();

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

        public static string? ValidarArchivo(string nombre, long tamano)
        {
            var extension = Path.GetExtension(nombre).ToLowerInvariant();

            if (extension is not (".pdf" or ".docx" or ".xlsx"))
                return "Solo se permiten archivos PDF, DOCX y XLSX.";

            if (tamano <= 0 || tamano > MaximoBytes)
                return "El archivo debe tener entre 1 byte y 10 MB.";

            return null;
        }
    }

    public class DocumentoGuardarDto
    {
        public string Tipo { get; set; } = "";
        public string? Version { get; set; }
        public string Nombre { get; set; } = "";

        public static DocumentoGuardarDto Desde(DocumentoDto documento) => new()
        {
            Tipo = documento.Tipo,
            Version = documento.Version,
            Nombre = documento.Nombre
        };

        public string? Validar()
        {
            if (!DocumentoDto.Tipos.ContainsKey(Tipo))
                return "Selecciona el tipo de documento.";

            if (string.IsNullOrWhiteSpace(Nombre))
                return "Ingresa el nombre del documento.";

            if (Nombre.Trim().Length > 255)
                return "El nombre no puede pasar de 255 caracteres.";

            if (Version?.Trim().Length > 30)
                return "La versión no puede pasar de 30 caracteres.";

            return null;
        }
    }
}
