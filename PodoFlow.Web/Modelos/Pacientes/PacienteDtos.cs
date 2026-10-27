
namespace PodoFlow.Web.Modelos.Pacientes;

public class PacienteDto
{
    public long Id { get; set; }
    public string TipoIdentificacion { get; set; } = "";
    public string Identificacion { get; set; } = "";
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public DateTime? FechaNacimiento { get; set; }
    public bool Estatus { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string Version { get; set; } = "";
    public string NombreCompleto =>
        $"{Nombres} {Apellidos}".Trim();
    public string? Genero { get; set; }
    public string? Direccion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}


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

public class CitaPacienteDto
{
    public long Id { get; set; }
    public long PacienteId { get; set; }
    public int ServicioId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = "";
    public string Canal { get; set; } = "";
}

public class PrestacionPacienteDto
{
    public long Id { get; set; }
    public long PacienteId { get; set; }
    public long? CitaId { get; set; }
    public long? AtencionId { get; set; }
    public int ServicioId { get; set; }
    public DateTime FechaPrestacion { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "";
}


public class PacienteGuardarDto
{
    [System.ComponentModel.DataAnnotations.Required(
        ErrorMessage = "Selecciona un tipo de identificación.")]
    public string TipoIdentificacion { get; set; } = "CEDULA";

    [System.ComponentModel.DataAnnotations.Required(
        ErrorMessage = "Ingresa la identificación.")]
    [System.ComponentModel.DataAnnotations.StringLength(30)]
    public string Identificacion { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required(
        ErrorMessage = "Ingresa los nombres.")]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string Nombres { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required(
        ErrorMessage = "Ingresa los apellidos.")]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string Apellidos { get; set; } = "";

    public DateTime? FechaNacimiento { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(30)]
    public string? Genero { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(300)]
    public string? Direccion { get; set; }

    // Solo se utiliza cuando editamos.
    public string Version { get; set; } = "";

    public static PacienteGuardarDto Desde(PacienteDto p) => new()
    {
        TipoIdentificacion = p.TipoIdentificacion,
        Identificacion = p.Identificacion,
        Nombres = p.Nombres,
        Apellidos = p.Apellidos,
        FechaNacimiento = p.FechaNacimiento,
        Genero = p.Genero,
        Direccion = p.Direccion,
        Version = p.Version
    };
}

public class ContactoGuardarDto
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string Tipo { get; set; } = "TELEFONO";

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(320)]
    public string Valor { get; set; } = "";

    public bool Principal { get; set; }
}
