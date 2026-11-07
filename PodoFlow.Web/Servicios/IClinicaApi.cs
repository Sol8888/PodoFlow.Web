using PodoFlow.Web.Modelos.Atenciones;

namespace PodoFlow.Web.Servicios
{
    public interface IClinicaApi
    {
        Task<List<AtencionDto>> ListarAtencionesAsync(string? estado);

        Task<List<HistoriaResumenDto>> ListarHistoriasAsync();

        Task ActualizarAtencionAsync(long id, AtencionEditarDto dto);

        Task CambiarEstadoAtencionAsync(long id, string nuevoEstado, string motivo, string version);

        Task CambiarEstadoHistoriaAsync(long id, string nuevoEstado, string? motivo, string version);
    }
}
