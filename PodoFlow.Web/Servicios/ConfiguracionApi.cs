using System.Net.Http.Json;
using PodoFlow.Web.Modelos.Configuracion;

namespace PodoFlow.Web.Servicios
{
    public class ConfiguracionApi : IConfiguracionApi
    {
        private readonly HttpClient _http;

        public ConfiguracionApi(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ServicioDetalleDto>> ListarServiciosAsync()
        {
            using var respuesta = await _http.GetAsync("api/servicios");
            await RespuestaApi.ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<List<ServicioDetalleDto>>() ?? [];
        }

        public async Task CrearServicioAsync(ServicioGuardarDto dto)
        {
            using var respuesta = await _http.PostAsJsonAsync("api/servicios", dto);
            await RespuestaApi.ValidarAsync(respuesta);
        }

        public async Task ActualizarServicioAsync(int id, ServicioGuardarDto dto)
        {
            using var respuesta = await _http.PutAsJsonAsync($"api/servicios/{id}", dto);
            await RespuestaApi.ValidarAsync(respuesta);
        }

        public async Task DesactivarServicioAsync(int id)
        {
            using var respuesta = await _http.DeleteAsync($"api/servicios/{id}");
            await RespuestaApi.ValidarAsync(respuesta);
        }
    }
}
