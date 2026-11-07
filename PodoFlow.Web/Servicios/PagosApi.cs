using PodoFlow.Web.Modelos.Pagos;

namespace PodoFlow.Web.Servicios
{
    public class PagosApi : IPagosApi
    {
        private readonly HttpClient _http;

        public PagosApi(HttpClient http)
        {
            _http = http;
        }

        public Task<List<PrestacionDto>> ListarPrestacionesAsync(long? pacienteId, string? estado)
        {
            var ruta = "api/prestaciones?";

            if (pacienteId.HasValue)
                ruta += $"pacienteId={pacienteId.Value}&";

            if (!string.IsNullOrWhiteSpace(estado))
                ruta += $"estado={Uri.EscapeDataString(estado)}&";

            return RespuestaApi.LeerTodoAsync<PrestacionDto>(_http, ruta);
        }

        public Task CrearPrestacionAsync(PrestacionCrearDto dto) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Post, "api/prestaciones", dto);

        public Task ActualizarPrestacionAsync(long id, PrestacionActualizarDto dto) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Put, $"api/prestaciones/{id}", dto);

        public Task AnularPrestacionAsync(long id, string motivo) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Patch, $"api/prestaciones/{id}/estado",
                new { NuevoEstado = "ANULADA", Motivo = motivo });

        public Task<List<CobroDto>> ListarCobrosAsync(long? prestacionId) =>
            RespuestaApi.LeerTodoAsync<CobroDto>(_http,
                prestacionId.HasValue ? $"api/cobros?prestacionId={prestacionId.Value}&" : "api/cobros?");

        public Task RegistrarCobroAsync(CobroCrearDto dto) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Post, "api/cobros", dto);

        public Task AnularCobroAsync(long id, string motivo) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Patch, $"api/cobros/{id}/estado",
                new { NuevoEstado = "ANULADO", Motivo = motivo });

        public Task<List<FormaPagoDto>> ListarFormasPagoAsync() =>
            RespuestaApi.LeerTodoAsync<FormaPagoDto>(_http, "api/formas-pago?incluirInactivos=true&");

        public Task CrearFormaPagoAsync(FormaPagoGuardarDto dto) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Post, "api/formas-pago", dto);

        public Task ActualizarFormaPagoAsync(int id, FormaPagoGuardarDto dto) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Put, $"api/formas-pago/{id}", dto);

        public Task DesactivarFormaPagoAsync(int id) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Delete, $"api/formas-pago/{id}", null);

        public Task ReactivarFormaPagoAsync(int id) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Patch, $"api/formas-pago/{id}/reactivar", null);
    }
}
