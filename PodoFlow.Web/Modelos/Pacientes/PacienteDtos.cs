
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Web.Modelos.Pacientes;

public class PacienteDto
{
    public long Id { get; set; }

    public string TipoIdentificacion { get; set; } = "";

    public string Identificacion { get; set; } = "";

    public string Nombres { get; set; } = "";

    public string Apellidos { get; set; } = "";

    public DateTime? FechaNacimiento { get; set; }

    public string? Genero { get; set; }

    public string? Direccion { get; set; }

    public bool Estatus { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public string Version { get; set; } = "";

    public string NombreCompleto =>
        $"{Nombres} {Apellidos}".Trim();
}

public class PacienteGuardarDto : IValidatableObject
{
    [Required(ErrorMessage = "Selecciona un tipo de identificación.")]
    public string TipoIdentificacion { get; set; } = "CEDULA";

    [Required(ErrorMessage = "Ingresa la identificación.")]
    [StringLength(30, MinimumLength = 5,
        ErrorMessage = "La identificación debe tener entre 5 y 30 caracteres.")]
    public string Identificacion { get; set; } = "";

    [Required(ErrorMessage = "Ingresa los nombres.")]
    [StringLength(100)]
    public string Nombres { get; set; } = "";

    [Required(ErrorMessage = "Ingresa los apellidos.")]
    [StringLength(100)]
    public string Apellidos { get; set; } = "";

    [DataType(DataType.Date)]
    public DateTime? FechaNacimiento { get; set; }

    [StringLength(30)]
    public string? Genero { get; set; }

    [StringLength(300)]
    public string? Direccion { get; set; }

    public string Version { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Identificacion))
        {
            yield return new ValidationResult(
                "Ingresa una identificación válida.",
                new[] { nameof(Identificacion) });
        }

        if (string.IsNullOrWhiteSpace(Nombres))
        {
            yield return new ValidationResult(
                "Los nombres no pueden estar vacíos.",
                new[] { nameof(Nombres) });
        }

        if (string.IsNullOrWhiteSpace(Apellidos))
        {
            yield return new ValidationResult(
                "Los apellidos no pueden estar vacíos.",
                new[] { nameof(Apellidos) });
        }

        if (FechaNacimiento.HasValue &&
            FechaNacimiento.Value.Date > DateTime.Today)
        {
            yield return new ValidationResult(
                "La fecha de nacimiento no puede ser futura.",
                new[] { nameof(FechaNacimiento) });
        }
    }

    public void Normalizar()
    {
        TipoIdentificacion =
            (TipoIdentificacion ?? "").Trim().ToUpperInvariant();

        Identificacion = (Identificacion ?? "").Trim();
        Nombres = (Nombres ?? "").Trim();
        Apellidos = (Apellidos ?? "").Trim();

        Genero = string.IsNullOrWhiteSpace(Genero)
            ? null
            : Genero.Trim();

        Direccion = string.IsNullOrWhiteSpace(Direccion)
            ? null
            : Direccion.Trim();
    }

    public static PacienteGuardarDto Desde(PacienteDto paciente)
    {
        return new PacienteGuardarDto
        {
            TipoIdentificacion = paciente.TipoIdentificacion,
            Identificacion = paciente.Identificacion,
            Nombres = paciente.Nombres,
            Apellidos = paciente.Apellidos,
            FechaNacimiento = paciente.FechaNacimiento,
            Genero = paciente.Genero,
            Direccion = paciente.Direccion,
            Version = paciente.Version
        };
    }
}
