namespace PodoFlow.Web.Modelos.Inventario
{
    public class InsumoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Unidad { get; set; } = "";
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public bool BajoMinimo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool Estatus { get; set; }
        public string Version { get; set; } = "";
    }

    public class InsumoGuardarDto
    {
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Unidad { get; set; } = "";
        public decimal StockMinimo { get; set; }
        public string? Version { get; set; }
    }

    public class MovimientoDto
    {
        public long Id { get; set; }
        public int InsumoId { get; set; }
        public long? AtencionId { get; set; }
        public long UsuarioId { get; set; }
        public string Tipo { get; set; } = "";
        public decimal Cantidad { get; set; }
        public decimal StockAnterior { get; set; }
        public decimal StockNuevo { get; set; }
        public DateTime FechaHora { get; set; }
        public string Motivo { get; set; } = "";
    }

    public class MovimientoCrearDto
    {
        public int InsumoId { get; set; }
        public long? AtencionId { get; set; }
        public long UsuarioId { get; set; }
        public string Tipo { get; set; } = "";
        public decimal Cantidad { get; set; }
        public string Motivo { get; set; } = "";
    }

    public static class TiposMovimiento
    {
        public static readonly (string Codigo, string Texto, bool Suma)[] Todos =
        [
            ("INGRESO", "Ingreso", true),
            ("CONSUMO", "Consumo", false),
            ("AJUSTE_POS", "Ajuste positivo", true),
            ("AJUSTE_NEG", "Ajuste negativo", false)
        ];

        public static string Texto(string codigo) =>
            Todos.FirstOrDefault(x => x.Codigo == codigo).Texto ?? codigo;

        public static bool Suma(string codigo) =>
            Todos.FirstOrDefault(x => x.Codigo == codigo).Suma;
    }
}
