namespace PodoFlow.Web.Modelos.Pagos
{
    public class PrestacionDto
    {
        public long Id { get; set; }
        public long PacienteId { get; set; }
        public long? CitaId { get; set; }
        public long? AtencionId { get; set; }
        public int ServicioId { get; set; }
        public long UsuarioId { get; set; }
        public DateTime FechaPrestacion { get; set; }
        public decimal PrecioAplicado { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "";
        public string? Observaciones { get; set; }
    }

    public class PrestacionCrearDto
    {
        public long PacienteId { get; set; }
        public long? CitaId { get; set; }
        public long? AtencionId { get; set; }
        public int ServicioId { get; set; }
        public long UsuarioId { get; set; }
        public decimal PrecioAplicado { get; set; }
        public decimal Cantidad { get; set; } = 1m;
        public string? Observaciones { get; set; }
    }

    public class PrestacionActualizarDto
    {
        public decimal PrecioAplicado { get; set; }
        public decimal Cantidad { get; set; } = 1m;
        public string? Observaciones { get; set; }
    }

    public class CobroDto
    {
        public long Id { get; set; }
        public long PrestacionId { get; set; }
        public int FormaPagoId { get; set; }
        public long UsuarioId { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Valor { get; set; }
        public string? Referencia { get; set; }
        public string? Observaciones { get; set; }
        public string Estado { get; set; } = "";
        public decimal TotalPrestacion { get; set; }
        public decimal TotalCobrado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string EstadoPrestacion { get; set; } = "";
    }

    public class CobroCrearDto
    {
        public long PrestacionId { get; set; }
        public int FormaPagoId { get; set; }
        public long UsuarioId { get; set; }
        public decimal Valor { get; set; }
        public string? Referencia { get; set; }
        public string? Observaciones { get; set; }
    }

    public class FormaPagoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public bool Estatus { get; set; }
    }

    public class FormaPagoGuardarDto
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
    }
}
