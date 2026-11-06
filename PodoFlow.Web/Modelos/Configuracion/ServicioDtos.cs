namespace PodoFlow.Web.Modelos.Configuracion
{
    public class ServicioDetalleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public short DuracionMinutos { get; set; }
        public bool Estatus { get; set; }
    }

    public class ServicioGuardarDto
    {
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public short DuracionMinutos { get; set; }
        public bool Estatus { get; set; } = true;
    }
}
