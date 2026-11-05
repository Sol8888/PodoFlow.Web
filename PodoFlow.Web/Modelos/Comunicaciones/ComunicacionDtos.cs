using System.Text.RegularExpressions;

namespace PodoFlow.Web.Modelos.Comunicaciones
{
    public class OpcionComunicacionDto
    {
        public long Id { get; set; }

        public long PacienteId { get; set; }

        public int FinalidadId { get; set; }

        public long? ConsentimientoId { get; set; }

        public string Canal { get; set; } = "";

        public bool Permitido { get; set; }

        public DateTime FechaActualizacion { get; set; }

        public bool Estatus { get; set; }
    }

    public static class CanalesComunicacion
    {
        public const string Correo = "CORREO";
        public const string WhatsApp = "WHATSAPP";

        public static readonly (string Codigo, string Nombre)[] Todos =
        [
            (Correo, "Correo"),
            (WhatsApp, "WhatsApp")
        ];

        public static string NombreDe(string canal) =>
            Todos.FirstOrDefault(c => c.Codigo == canal).Nombre ?? canal;

        public static string[] TiposContacto(string canal) =>
            canal == WhatsApp ? [WhatsApp, "CELULAR"] : [canal];
    }

    public class PlantillaDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public string Canal { get; set; } = "";

        public string Tipo { get; set; } = "";

        public string? Asunto { get; set; }

        public string Contenido { get; set; } = "";

        public string Version { get; set; } = "";

        public DateTime FechaCreacion { get; set; }

        public bool Estatus { get; set; }
    }

    public class PlantillaGuardarDto
    {
        public string Nombre { get; set; } = "";

        public string Canal { get; set; } = CanalesComunicacion.Correo;

        public string Tipo { get; set; } = "";

        public string? Asunto { get; set; }

        public string Contenido { get; set; } = "";

        public string? Version { get; set; }

        public static PlantillaGuardarDto Desde(PlantillaDto plantilla) => new()
        {
            Nombre = plantilla.Nombre,
            Canal = plantilla.Canal,
            Tipo = plantilla.Tipo,
            Asunto = plantilla.Asunto,
            Contenido = plantilla.Contenido,
            Version = plantilla.Version
        };

        public string? Validar()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
                return "Ingresa el nombre de la plantilla.";

            if (string.IsNullOrWhiteSpace(Tipo))
                return "Ingresa el tipo de plantilla.";

            if (Canal == CanalesComunicacion.Correo && string.IsNullOrWhiteSpace(Asunto))
                return "Las plantillas de correo necesitan asunto.";

            if (string.IsNullOrWhiteSpace(Contenido))
                return "Ingresa el contenido de la plantilla.";

            if (Canal == CanalesComunicacion.WhatsApp)
            {
                if (Contenido.Trim().Length > VariablesPlantilla.MaximoWhatsApp)
                    return $"El contenido para WhatsApp no puede pasar de {VariablesPlantilla.MaximoWhatsApp} caracteres.";

                if (VariablesPlantilla.CantidadNumeradas(Contenido) > VariablesPlantilla.Disponibles)
                    return $"WhatsApp solo admite las variables {{{{1}}}} a {{{{{VariablesPlantilla.Disponibles}}}}}.";
            }

            return null;
        }
    }

    public static class VariablesPlantilla
    {
        public const int MaximoWhatsApp = 4096;

        public static readonly string[] TiposSugeridos = ["RECORDATORIO_CITA", "CONFIRMACION_CITA", "FIDELIZACION"];

        public static readonly (string Nombre, string Ejemplo)[] Valores =
        [
            ("paciente", "Ana"),
            ("fecha", "20/11/2026"),
            ("hora", "10:30")
        ];

        public static int Disponibles => Valores.Length;

        private static readonly Regex Numeradas = new(@"\{\{(\d+)\}\}", RegexOptions.Compiled);

        public static string Vista(string? texto)
        {
            var resultado = texto ?? "";

            for (var i = 0; i < Valores.Length; i++)
            {
                resultado = resultado
                    .Replace($"{{{{{i + 1}}}}}", Valores[i].Ejemplo)
                    .Replace($"{{{{{Valores[i].Nombre}}}}}", Valores[i].Ejemplo);
            }

            return resultado;
        }

        public static int CantidadNumeradas(string? texto) =>
            Numeradas.Matches(texto ?? "")
                .Select(m => int.TryParse(m.Groups[1].Value, out var numero) ? numero : 0)
                .DefaultIfEmpty(0)
                .Max();

        public static string NombreMeta(string nombre) =>
            Regex.Replace(nombre.Trim().ToLowerInvariant(), @"[^a-z0-9_]+", "_");

        public static string SiguienteVersion(string version)
        {
            var mayor = version.Split('.')[0];

            return int.TryParse(mayor, out var numero) ? $"{numero + 1}.0" : "";
        }
    }
}
