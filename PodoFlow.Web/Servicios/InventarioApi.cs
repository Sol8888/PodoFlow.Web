using PodoFlow.Web.Modelos.Inventario;

namespace PodoFlow.Web.Servicios
{
    public class InventarioApi : IInventarioApi
    {
        private readonly HttpClient _http;

        public InventarioApi(HttpClient http)
        {
            _http = http;
        }

        public Task<List<InsumoDto>> ListarInsumosAsync() =>
            RespuestaApi.LeerTodoAsync<InsumoDto>(_http, "api/insumos?incluirInactivos=true&");

        public Task CrearInsumoAsync(InsumoGuardarDto dto) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Post, "api/insumos", dto);

        public Task ActualizarInsumoAsync(int id, InsumoGuardarDto dto) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Put, $"api/insumos/{id}", dto);

        public Task DesactivarInsumoAsync(int id, string version) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Delete,
                $"api/insumos/{id}?version={Uri.EscapeDataString(version)}", null);

        public Task ReactivarInsumoAsync(int id, string version) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Patch,
                $"api/insumos/{id}/reactivar?version={Uri.EscapeDataString(version)}", null);

        public Task<List<MovimientoDto>> ListarMovimientosAsync(int? insumoId, string? tipo)
        {
            var ruta = "api/movimientos?";

            if (insumoId.HasValue)
                ruta += $"insumoId={insumoId.Value}&";

            if (!string.IsNullOrWhiteSpace(tipo))
                ruta += $"tipo={Uri.EscapeDataString(tipo)}&";

            return RespuestaApi.LeerTodoAsync<MovimientoDto>(_http, ruta);
        }

        public Task CrearMovimientoAsync(MovimientoCrearDto dto) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Post, "api/movimientos", dto);
    }
}
