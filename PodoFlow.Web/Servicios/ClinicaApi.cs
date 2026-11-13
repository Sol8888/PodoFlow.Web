using PodoFlow.Web.Modelos.Atenciones;

namespace PodoFlow.Web.Servicios
{
    public class ClinicaApi : IClinicaApi
    {
        private readonly HttpClient _http;

        public ClinicaApi(HttpClient http)
        {
            _http = http;
        }

        public Task<List<AtencionDto>> ListarAtencionesAsync(string? estado)
        {
            var ruta = "api/atenciones?";

            if (!string.IsNullOrWhiteSpace(estado))
                ruta += $"estado={Uri.EscapeDataString(estado)}&";

            return RespuestaApi.LeerTodoAsync<AtencionDto>(_http, ruta);
        }

        public Task<List<HistoriaResumenDto>> ListarHistoriasAsync() =>
            RespuestaApi.LeerTodoAsync<HistoriaResumenDto>(_http, "api/historias?");

        public Task ActualizarAtencionAsync(long id, AtencionEditarDto dto) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Put, $"api/atenciones/{id}", dto);

        public Task CambiarEstadoAtencionAsync(long id, string nuevoEstado, string motivo, string version) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Patch, $"api/atenciones/{id}/estado",
                new { NuevoEstado = nuevoEstado, Motivo = motivo, Version = version });

        public Task CambiarEstadoHistoriaAsync(long id, string nuevoEstado, string? motivo, string version) =>
            RespuestaApi.EnviarAsync(_http, HttpMethod.Patch, $"api/historias/{id}/estado",
                new { NuevoEstado = nuevoEstado, Motivo = motivo, Version = version });
    }
}
