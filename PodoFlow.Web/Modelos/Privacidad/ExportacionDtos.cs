namespace PodoFlow.Web.Modelos.Privacidad
{
    public class ExportacionDto
    {
        public long Id { get; set; }

        public long SolicitudId { get; set; }

        public long UsuarioId { get; set; }

        public string Formato { get; set; } = "";

        public string RutaArchivo { get; set; } = "";

        public string? Huella { get; set; }

        public DateTime FechaGeneracion { get; set; }

        public DateTime? FechaEntrega { get; set; }

        public string Estado { get; set; } = "";

        public string ClaseEstado => Estado switch
        {
            "ENTREGADA" => "success",
            "ANULADA" => "danger",
            _ => "info"
        };

        public string HuellaCorta => string.IsNullOrEmpty(Huella) || Huella.Length <= 16
            ? Huella ?? "Sin huella"
            : $"{Huella[..8]}…{Huella[^6..]}";
    }

    public class ArchivoExportacionDto
    {
        public byte[] Contenido { get; set; } = [];

        public string NombreArchivo { get; set; } = "";

        public string TipoContenido { get; set; } = "";
    }
}
