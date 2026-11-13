using PodoFlow.Web.Modelos.Citas;

namespace PodoFlow.Web.Servicios
{
    public interface IAgendaApi
    {
        Task<CatalogosAgenda> CargarCatalogosAsync();

        Task<List<CitaDto>> ListarCitasAsync(long? usuarioId, string? estado, int pagina, int tamano);

        // Fechas en hora de Ecuador. La API no filtra por fecha, así que se recorre hasta pasar el rango.
        Task<List<CitaDto>> CitasEntreAsync(DateTime desde, DateTime hasta);

        Task<CitaDto?> ObtenerCitaAsync(long id);

        Task<CitaDto> CrearCitaAsync(CitaGuardarDto dto);

        Task ActualizarCitaAsync(long id, CitaGuardarDto dto);

        Task CambiarEstadoCitaAsync(long id, string estado, string motivo, string version);

        Task<List<CambioCitaDto>> CambiosCitaAsync(long citaId);

        Task<List<HorarioDto>> ListarHorariosAsync();

        Task CrearHorarioAsync(HorarioGuardarDto dto);

        Task ActualizarHorarioAsync(int id, HorarioGuardarDto dto);

        Task DesactivarHorarioAsync(int id);

        Task ReactivarHorarioAsync(int id);

        Task<List<BloqueoDto>> ListarBloqueosAsync();

        Task CrearBloqueoAsync(BloqueoGuardarDto dto);

        Task ActualizarBloqueoAsync(long id, BloqueoGuardarDto dto);

        Task DesactivarBloqueoAsync(long id);

        Task ReactivarBloqueoAsync(long id);
    }
}
