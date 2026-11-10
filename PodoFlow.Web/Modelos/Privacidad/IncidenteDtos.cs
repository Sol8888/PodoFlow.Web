namespace PodoFlow.Web.Modelos.Privacidad
{
    public class IncidenteDto
    {
        public long Id { get; set; }
        public long UsuarioId { get; set; }
        public DateTime FechaDeteccion { get; set; }
        public string Tipo { get; set; } = "";
        public string Modulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string? DatosAfectados { get; set; }
        public string? ImpactoPreliminar { get; set; }
        public string Estado { get; set; } = "";
        public DateTime? FechaCierre { get; set; }
        public string Version { get; set; } = "";

        public static readonly IReadOnlyDictionary<string, string[]> Transiciones = new Dictionary<string, string[]>
        {
            ["ABIERTO"] = ["ANALISIS", "CONTENIDO", "CERRADO"],
            ["ANALISIS"] = ["CONTENIDO", "CERRADO"],
            ["CONTENIDO"] = ["RECUPERADO"],
            ["RECUPERADO"] = ["CERRADO"],
            ["CERRADO"] = []
        };

        public static readonly IReadOnlyDictionary<string, string> NombresEstado = new Dictionary<string, string>
        {
            ["ABIERTO"] = "Abierto",
            ["ANALISIS"] = "En análisis",
            ["CONTENIDO"] = "Contenido",
            ["RECUPERADO"] = "Recuperado",
            ["CERRADO"] = "Cerrado"
        };

        public IEnumerable<string> EstadosSiguientes =>
            Transiciones.TryGetValue(Estado, out var siguientes) ? siguientes : [];

        public bool EsCerrado => Estado == "CERRADO";

        public static string NombreEstado(string estado) =>
            NombresEstado.TryGetValue(estado, out var nombre) ? nombre : estado;

        public static string ClaseEstado(string estado) => estado switch
        {
            "ABIERTO" => "pf-status danger",
            "ANALISIS" or "CONTENIDO" => "pf-status warning",
            "RECUPERADO" => "pf-status info",
            _ => "pf-status success"
        };
    }

    public class IncidenteGuardarDto
    {
        public DateTime FechaDeteccion { get; set; }
        public string Tipo { get; set; } = "";
        public string Modulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string? DatosAfectados { get; set; }
        public string? ImpactoPreliminar { get; set; }

        public static IncidenteGuardarDto Desde(IncidenteDto incidente, DateTime fechaLocal) => new()
        {
            FechaDeteccion = fechaLocal,
            Tipo = incidente.Tipo,
            Modulo = incidente.Modulo,
            Descripcion = incidente.Descripcion,
            DatosAfectados = incidente.DatosAfectados,
            ImpactoPreliminar = incidente.ImpactoPreliminar
        };

        public string? Validar(DateTime ahoraLocal)
        {
            if (FechaDeteccion == default)
                return "Indica cuándo se detectó el incidente.";

            if (FechaDeteccion > ahoraLocal.AddMinutes(5))
                return "La fecha de detección no puede estar en el futuro.";

            if (string.IsNullOrWhiteSpace(Tipo))
                return "Indica el tipo de incidente.";

            if (Tipo.Trim().Length > 100)
                return "El tipo no puede pasar de 100 caracteres.";

            if (string.IsNullOrWhiteSpace(Modulo))
                return "Indica el módulo afectado.";

            if (Modulo.Trim().Length > 80)
                return "El módulo no puede pasar de 80 caracteres.";

            if (string.IsNullOrWhiteSpace(Descripcion))
                return "Describe qué pasó.";

            if (Descripcion.Trim().Length > 3000)
                return "La descripción no puede pasar de 3000 caracteres.";

            if (DatosAfectados?.Trim().Length > 2000)
                return "Los datos afectados no pueden pasar de 2000 caracteres.";

            if (ImpactoPreliminar?.Trim().Length > 1500)
                return "El impacto preliminar no puede pasar de 1500 caracteres.";

            return null;
        }
    }

    public class AccionIncidenteDto
    {
        public long Id { get; set; }
        public long IncidenteId { get; set; }
        public long UsuarioId { get; set; }
        public DateTime FechaHora { get; set; }
        public string Tipo { get; set; } = "";
        public string Accion { get; set; } = "";
        public string? Observacion { get; set; }
    }

    public class AccionIncidenteGuardarDto
    {
        public static readonly IReadOnlyDictionary<string, string> Tipos = new Dictionary<string, string>
        {
            ["CONTENCION"] = "Contención",
            ["INVESTIGACION"] = "Investigación",
            ["NOTIFICACION_AUTORIDAD"] = "Notificación a la autoridad",
            ["NOTIFICACION_TITULAR"] = "Notificación a los titulares",
            ["RECUPERACION"] = "Recuperación",
            ["SEGUIMIENTO"] = "Seguimiento",
            ["OTRO"] = "Otra acción"
        };

        public string Tipo { get; set; } = "";
        public string Accion { get; set; } = "";
        public string? Observacion { get; set; }

        public static string NombreTipo(string tipo) => tipo switch
        {
            "REGISTRO" => "Registro",
            "CAMBIO_ESTADO" => "Cambio de estado",
            _ => Tipos.TryGetValue(tipo, out var nombre) ? nombre : tipo
        };

        public string? Validar()
        {
            if (!Tipos.ContainsKey(Tipo))
                return "Selecciona el tipo de acción.";

            if (string.IsNullOrWhiteSpace(Accion))
                return "Describe la acción realizada.";

            if (Accion.Trim().Length > 2000)
                return "La acción no puede pasar de 2000 caracteres.";

            if (Observacion?.Trim().Length > 1500)
                return "La observación no puede pasar de 1500 caracteres.";

            return null;
        }
    }
}
