using PodoFlow.Web.Modelos.Inventario;

namespace PodoFlow.Web.Servicios
{
    public interface IInventarioApi
    {
        Task<List<InsumoDto>> ListarInsumosAsync();

        Task CrearInsumoAsync(InsumoGuardarDto dto);

        Task ActualizarInsumoAsync(int id, InsumoGuardarDto dto);

        Task DesactivarInsumoAsync(int id, string version);

        Task ReactivarInsumoAsync(int id, string version);

        Task<List<MovimientoDto>> ListarMovimientosAsync(int? insumoId, string? tipo);

        Task CrearMovimientoAsync(MovimientoCrearDto dto);
    }
}
