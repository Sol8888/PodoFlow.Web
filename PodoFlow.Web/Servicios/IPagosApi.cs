using PodoFlow.Web.Modelos.Pagos;

namespace PodoFlow.Web.Servicios
{
    public interface IPagosApi
    {
        Task<List<PrestacionDto>> ListarPrestacionesAsync(long? pacienteId, string? estado);

        Task CrearPrestacionAsync(PrestacionCrearDto dto);

        Task ActualizarPrestacionAsync(long id, PrestacionActualizarDto dto);

        Task AnularPrestacionAsync(long id, string motivo);

        Task<List<CobroDto>> ListarCobrosAsync(long? prestacionId);

        Task RegistrarCobroAsync(CobroCrearDto dto);

        Task AnularCobroAsync(long id, string motivo);

        Task<List<FormaPagoDto>> ListarFormasPagoAsync();

        Task CrearFormaPagoAsync(FormaPagoGuardarDto dto);

        Task ActualizarFormaPagoAsync(int id, FormaPagoGuardarDto dto);

        Task DesactivarFormaPagoAsync(int id);

        Task ReactivarFormaPagoAsync(int id);
    }
}
