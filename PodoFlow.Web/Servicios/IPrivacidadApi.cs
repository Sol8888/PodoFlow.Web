using PodoFlow.Web.Modelos.Pacientes;
using PodoFlow.Web.Modelos.Privacidad;

namespace PodoFlow.Web.Servicios
{
    public interface IPrivacidadApi
    {
        Task<List<PacienteDto>> ListarPacientesActivosAsync();

        Task<PacienteDto?> ObtenerPacienteAsync(long id);

        Task<List<SolicitudDto>> ListarSolicitudesAsync(int pagina, int tamano);

        Task<SolicitudDto?> ObtenerSolicitudAsync(long id);

        Task<List<AccionSolicitudDto>> AccionesAsync(long solicitudId);

        Task<SolicitudDto> CrearSolicitudAsync(SolicitudGuardarDto dto);

        Task<SolicitudDto> CambiarEstadoSolicitudAsync(
            long id,
            string estado,
            string version,
            string? observacion,
            string? respuestaTitular,
            string? medioRespuesta);

        Task<List<EvidenciaDto>> EvidenciasAsync(long solicitudId);

        Task<EvidenciaDto> CrearEvidenciaAsync(long solicitudId, EvidenciaGuardarDto dto);

        Task<List<ExportacionDto>> ExportacionesAsync(long solicitudId);

        Task<ExportacionDto> GenerarExportacionAsync(
            long solicitudId,
            string formato,
            List<string>? secciones,
            string? observacion);

        Task<ExportacionDto> CambiarEstadoExportacionAsync(long id, string estado, string? observacion);

        Task<ArchivoExportacionDto> DescargarExportacionAsync(long id);
    }
}
