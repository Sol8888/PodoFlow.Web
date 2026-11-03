
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Web.Modelos.Pacientes;

public class ContactoPacienteDto
{
    public long Id { get; set; }

    public long PacienteId { get; set; }

    public string Tipo { get; set; } = "";

    public string Valor { get; set; } = "";

    public bool Principal { get; set; }

    public bool Verificado { get; set; }

    public bool Estatus { get; set; }
}

public class ContactoGuardarDto : IValidatableObject
{
    [Required(ErrorMessage = "Selecciona un tipo de contacto.")]
    [StringLength(20)]
    public string Tipo { get; set; } = "TELEFONO";

    [Required(ErrorMessage = "Ingresa el valor del contacto.")]
    [StringLength(320)]
    public string Valor { get; set; } = "";

    public bool Principal { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Valor))
        {
            yield return new ValidationResult(
                "Ingresa un contacto válido.",
                new[] { nameof(Valor) });
        }

        if (string.Equals(
                Tipo,
                "CORREO",
                StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(Valor))
        {
            var validador = new EmailAddressAttribute();

            if (!validador.IsValid(Valor))
            {
                yield return new ValidationResult(
                    "Ingresa un correo electrónico válido.",
                    new[] { nameof(Valor) });
            }
        }
    }

    public void Normalizar()
    {
        Tipo = (Tipo ?? "").Trim().ToUpperInvariant();
        Valor = (Valor ?? "").Trim();
    }
}
