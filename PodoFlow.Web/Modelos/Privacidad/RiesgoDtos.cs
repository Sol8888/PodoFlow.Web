namespace PodoFlow.Web.Modelos.Privacidad
{
    public class RiesgoDto
    {
        public long Id { get; set; }
        public int TratamientoId { get; set; }
        public string Descripcion { get; set; } = "";
        public byte Probabilidad { get; set; }
        public byte Impacto { get; set; }
        public byte Nivel { get; set; }
        public string? MedidasMitigacion { get; set; }
        public byte? RiesgoResidual { get; set; }
        public string Estado { get; set; } = "";
        public DateOnly? FechaRevision { get; set; }

        public static readonly IReadOnlyDictionary<string, string[]> Transiciones = new Dictionary<string, string[]>
        {
            ["ABIERTO"] = ["MITIGADO", "ACEPTADO", "CERRADO"],
            ["MITIGADO"] = ["ABIERTO", "ACEPTADO", "CERRADO"],
            ["ACEPTADO"] = ["ABIERTO", "CERRADO"],
            ["CERRADO"] = []
        };

        public IEnumerable<string> EstadosSiguientes =>
            Transiciones.TryGetValue(Estado, out var siguientes) ? siguientes : [];

        public bool EsEditable => Estado != "CERRADO";

        public static string Clasificacion(int nivel) => nivel switch
        {
            <= 5 => "Bajo",
            <= 12 => "Medio",
            _ => "Alto"
        };

        public static string ClaseNivel(int nivel) => nivel switch
        {
            <= 5 => "pf-status success",
            <= 12 => "pf-status warning",
            _ => "pf-status danger"
        };
    }

    public class RiesgoGuardarDto
    {
        public int TratamientoId { get; set; }
        public string Descripcion { get; set; } = "";
        public byte Probabilidad { get; set; } = 1;
        public byte Impacto { get; set; } = 1;
        public string? MedidasMitigacion { get; set; }
        public byte? RiesgoResidual { get; set; }
        public DateOnly? FechaRevision { get; set; }

        public int Nivel => Probabilidad * Impacto;

        public static RiesgoGuardarDto Desde(RiesgoDto riesgo) => new()
        {
            TratamientoId = riesgo.TratamientoId,
            Descripcion = riesgo.Descripcion,
            Probabilidad = riesgo.Probabilidad,
            Impacto = riesgo.Impacto,
            MedidasMitigacion = riesgo.MedidasMitigacion,
            RiesgoResidual = riesgo.RiesgoResidual,
            FechaRevision = riesgo.FechaRevision
        };

        public string? Validar(bool esNuevo)
        {
            if (esNuevo && TratamientoId <= 0)
                return "Selecciona el tratamiento al que afecta el riesgo.";

            if (string.IsNullOrWhiteSpace(Descripcion))
                return "Describe el riesgo.";

            if (Descripcion.Trim().Length > 2000 || MedidasMitigacion?.Trim().Length > 2000)
                return "La descripción y las medidas no pueden pasar de 2000 caracteres.";

            if (Probabilidad is < 1 or > 5 || Impacto is < 1 or > 5)
                return "La probabilidad y el impacto van de 1 a 5.";

            if (RiesgoResidual.HasValue && (RiesgoResidual.Value < 1 || RiesgoResidual.Value > Nivel))
                return $"El riesgo residual va de 1 al nivel del riesgo ({Nivel}).";

            return null;
        }
    }

    public class EvaluacionImpactoDto
    {
        public long Id { get; set; }
        public long UsuarioId { get; set; }
        public string Version { get; set; } = "";
        public DateOnly Fecha { get; set; }
        public string Alcance { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Conclusion { get; set; }
        public byte? NivelResidual { get; set; }
        public string Estado { get; set; } = "";
        public DateTime? FechaModificacion { get; set; }

        public static readonly IReadOnlyDictionary<string, string[]> Transiciones = new Dictionary<string, string[]>
        {
            ["BORRADOR"] = ["APROBADA", "CERRADA"],
            ["APROBADA"] = ["REVISADA", "CERRADA"],
            ["REVISADA"] = ["CERRADA"],
            ["CERRADA"] = []
        };

        public IEnumerable<string> EstadosSiguientes =>
            Transiciones.TryGetValue(Estado, out var siguientes) ? siguientes : [];

        public bool EsBorrador => Estado == "BORRADOR";
    }

    public class EvaluacionImpactoGuardarDto
    {
        public string Version { get; set; } = "";
        public DateOnly Fecha { get; set; }
        public string Alcance { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Conclusion { get; set; }
        public byte? NivelResidual { get; set; }

        public static EvaluacionImpactoGuardarDto Desde(EvaluacionImpactoDto evaluacion) => new()
        {
            Version = evaluacion.Version,
            Fecha = evaluacion.Fecha,
            Alcance = evaluacion.Alcance,
            Descripcion = evaluacion.Descripcion,
            Conclusion = evaluacion.Conclusion,
            NivelResidual = evaluacion.NivelResidual
        };

        public string? Validar()
        {
            if (string.IsNullOrWhiteSpace(Version))
                return "Ingresa la versión de la evaluación.";

            if (Version.Trim().Length > 30)
                return "La versión no puede pasar de 30 caracteres.";

            if (string.IsNullOrWhiteSpace(Alcance))
                return "Describe el alcance de la evaluación.";

            if (Alcance.Trim().Length > 2000)
                return "El alcance no puede pasar de 2000 caracteres.";

            if (Descripcion?.Trim().Length > 3000 || Conclusion?.Trim().Length > 3000)
                return "La descripción y la conclusión no pueden pasar de 3000 caracteres.";

            if (NivelResidual is < 1 or > 25)
                return "El nivel residual va de 1 a 25.";

            return null;
        }
    }

    public class EvaluacionRiesgoDto
    {
        public long EvaluacionId { get; set; }
        public long RiesgoId { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public string? Observacion { get; set; }
    }
}
