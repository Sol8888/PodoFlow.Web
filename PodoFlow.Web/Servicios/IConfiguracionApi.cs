using PodoFlow.Web.Modelos.Configuracion;

namespace PodoFlow.Web.Servicios
{
    public interface IConfiguracionApi
    {
        Task<List<ServicioDetalleDto>> ListarServiciosAsync();

        Task CrearServicioAsync(ServicioGuardarDto dto);

        Task ActualizarServicioAsync(int id, ServicioGuardarDto dto);

        Task DesactivarServicioAsync(int id);
    }
}
