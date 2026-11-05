using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PodoFlow.Web.Modelos.Comunicaciones;

namespace PodoFlow.Web.Servicios
{
    public class ComunicacionesApi : IComunicacionesApi
    {
        private const int TamanoLote = 100;
        private const int MaximoLotes = 20;

        private readonly HttpClient _http;

        public ComunicacionesApi(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<OpcionComunicacionDto>> OpcionesAsync(long pacienteId)
        {
            return await LeerAsync<List<OpcionComunicacionDto>>(
                $"api/OpcionComunicacion?pacienteId={pacienteId}&pagina=1&tamano={TamanoLote}") ?? [];
        }

        public Task CrearOpcionAsync(long pacienteId, int finalidadId, string canal, bool permitido)
        {
            var datos = new
            {
                PacienteId = pacienteId,
                FinalidadId = finalidadId,
                Canal = canal,
                Permitido = permitido
            };

            return EnviarAsync(HttpMethod.Post, "api/OpcionComunicacion", datos);
        }

        public Task CambiarPermisoAsync(long opcionId, bool permitido)
        {
            return EnviarAsync(HttpMethod.Put, $"api/OpcionComunicacion/{opcionId}", new { Permitido = permitido });
        }

        public async Task<List<PlantillaDto>> PlantillasAsync(bool incluirInactivas)
        {
            var plantillas = new List<PlantillaDto>();

            for (var pagina = 1; pagina <= MaximoLotes; pagina++)
            {
                var lote = await LeerAsync<List<PlantillaDto>>(
                    $"api/Plantilla?incluirInactivos={incluirInactivas.ToString().ToLowerInvariant()}&pagina={pagina}&tamano={TamanoLote}") ?? [];
                plantillas.AddRange(lote);

                if (lote.Count < TamanoLote)
                    break;
            }

            return plantillas;
        }

        public Task CrearPlantillaAsync(PlantillaGuardarDto dto)
        {
            return EnviarAsync(HttpMethod.Post, "api/Plantilla", DatosPlantilla(dto));
        }

        public Task ActualizarPlantillaAsync(int id, PlantillaGuardarDto dto)
        {
            return EnviarAsync(HttpMethod.Put, $"api/Plantilla/{id}", DatosPlantilla(dto));
        }

        public Task DesactivarPlantillaAsync(int id)
        {
            return EnviarAsync(HttpMethod.Delete, $"api/Plantilla/{id}", null);
        }

        public Task ReactivarPlantillaAsync(int id)
        {
            return EnviarAsync(HttpMethod.Patch, $"api/Plantilla/{id}/reactivar", null);
        }

        private static object DatosPlantilla(PlantillaGuardarDto dto)
        {
            var esCorreo = dto.Canal == CanalesComunicacion.Correo;

            return new
            {
                Nombre = dto.Nombre.Trim(),
                dto.Canal,
                Tipo = dto.Tipo.Trim().ToUpperInvariant(),
                Asunto = esCorreo ? Limpiar(dto.Asunto) : null,
                Contenido = dto.Contenido.Trim(),
                Version = Limpiar(dto.Version)
            };
        }

        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private async Task<T?> LeerAsync<T>(string ruta)
        {
            using var respuesta = await _http.GetAsync(ruta);
            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<T>();
        }

        private async Task EnviarAsync(HttpMethod metodo, string ruta, object? datos)
        {
            using var solicitud = new HttpRequestMessage(metodo, ruta);

            if (datos != null)
                solicitud.Content = JsonContent.Create(datos, datos.GetType());

            using var respuesta = await _http.SendAsync(solicitud);
            await ValidarAsync(respuesta);
        }

        private static async Task ValidarAsync(HttpResponseMessage respuesta)
        {
            if (respuesta.IsSuccessStatusCode)
                return;

            var mensaje = LeerMensaje(await respuesta.Content.ReadAsStringAsync());

            throw new InvalidOperationException(mensaje ?? respuesta.StatusCode switch
            {
                HttpStatusCode.BadRequest => "Revisa los datos ingresados.",
                HttpStatusCode.Unauthorized => "La API rechazó la sesión. Revisa el token de desarrollo de PodoFlow.Web.",
                HttpStatusCode.Forbidden => "No tienes permisos de comunicaciones para esta operación.",
                HttpStatusCode.NotFound => "No se encontró el registro solicitado.",
                HttpStatusCode.Conflict => "La operación no está permitida en el estado actual del registro.",
                _ => $"No se pudo completar la operación. Código HTTP: {(int)respuesta.StatusCode}."
            });
        }

        private static string? LeerMensaje(string contenido)
        {
            if (string.IsNullOrWhiteSpace(contenido))
                return null;

            try
            {
                using var documento = JsonDocument.Parse(contenido);
                var raiz = documento.RootElement;

                if (raiz.ValueKind != JsonValueKind.Object)
                    return null;

                if (raiz.TryGetProperty("mensaje", out var mensaje) && mensaje.ValueKind == JsonValueKind.String)
                    return mensaje.GetString();

                if (raiz.TryGetProperty("errors", out var errores) && errores.ValueKind == JsonValueKind.Object)
                {
                    foreach (var campo in errores.EnumerateObject())
                    {
                        if (campo.Value.ValueKind == JsonValueKind.Array && campo.Value.GetArrayLength() > 0)
                            return campo.Value[0].GetString();
                    }
                }
            }
            catch (JsonException)
            {
                return null;
            }

            return null;
        }
    }
}
