
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Web.Modelos.Pacientes;

public class AntecedenteDto
{
    public long Id { get; set; }

    public long HistoriaId { get; set; }

    public long? UsuarioId { get; set; }

    public string Tipo { get; set; } = "";

    public string Descripcion { get; set; } = "";

    public DateTime FechaRegistro { get; set; }

    public bool Estatus { get; set; }
}

public class AntecedenteGuardarDto : IValidatableObject
{
    [Required(ErrorMessage = "Ingresa el tipo de antecedente.")]
    [StringLength(80, ErrorMessage =
        "El tipo no puede superar los 80 caracteres.")]
    public string Tipo { get; set; } = "";

    [Required(ErrorMessage = "Ingresa la descripción del antecedente.")]
    [StringLength(2000, ErrorMessage =
        "La descripción no puede superar los 2000 caracteres.")]
    public string Descripcion { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Tipo))
        {
            yield return new ValidationResult(
                "El tipo de antecedente es obligatorio.",
                new[] { nameof(Tipo) });
        }

        if (string.IsNullOrWhiteSpace(Descripcion))
        {
            yield return new ValidationResult(
                "La descripción es obligatoria.",
                new[] { nameof(Descripcion) });
        }
    }

    public void Normalizar()
    {
        Tipo = (Tipo ?? "").Trim().ToUpperInvariant();
        Descripcion = (Descripcion ?? "").Trim();
    }

    public static AntecedenteGuardarDto Desde(AntecedenteDto antecedente)
    {
        return new AntecedenteGuardarDto
        {
            Tipo = antecedente.Tipo,
            Descripcion = antecedente.Descripcion
        };
    }
}
