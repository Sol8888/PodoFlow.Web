using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PodoFlow.Web.Modelos.Citas;
using PodoFlow.Web.Modelos.Pacientes;

namespace PodoFlow.Web.Servicios
{
    public class AgendaApi : IAgendaApi
    {
        private const int TamanoLote = 100;
        private const int MaximoLotes = 20;

        private readonly HttpClient _http;
        private readonly IPacientesApi _pacientes;

        public AgendaApi(HttpClient http, IPacientesApi pacientes)
        {
            _http = http;
            _pacientes = pacientes;
        }

        public async Task<CatalogosAgenda> CargarCatalogosAsync()
        {
            var servicios = LeerAsync<List<ServicioDto>>("api/servicios");
            var usuarios = LeerTodoAsync<UsuarioAgendaDto>("api/usuarios?");
            var pacientes = ListarPacientesAsync();
            var horarios = ListarHorariosAsync();
            var bloqueos = ListarBloqueosAsync();

            await Task.WhenAll(servicios, usuarios, pacientes, horarios, bloqueos);

            return new CatalogosAgenda
            {
                Servicios = await servicios ?? [],
                Usuarios = await usuarios,
                Pacientes = await pacientes,
                Horarios = await horarios,
                Bloqueos = await bloqueos
            };
        }

        public async Task<List<CitaDto>> ListarCitasAsync(long? usuarioId, string? estado, int pagina, int tamano)
        {
            var ruta = $"api/citas?pagina={pagina}&tamano={tamano}";

            if (usuarioId.HasValue)
                ruta += $"&usuarioId={usuarioId.Value}";

            if (!string.IsNullOrWhiteSpace(estado))
                ruta += $"&estado={Uri.EscapeDataString(estado)}";

            return await LeerAsync<List<CitaDto>>(ruta) ?? [];
        }

        public async Task<List<CitaDto>> CitasEntreAsync(DateTime desde, DateTime hasta)
        {
            var citas = new List<CitaDto>();

            for (var pagina = 1; pagina <= MaximoLotes; pagina++)
            {
                var lote = await ListarCitasAsync(null, null, pagina, TamanoLote);

                citas.AddRange(lote.Where(x => x.InicioLocal < hasta && desde < x.FinLocal));

                // La API las devuelve de la más reciente a la más antigua.
                if (lote.Count < TamanoLote || lote[^1].InicioLocal < desde)
                    return citas;
            }

            throw new InvalidOperationException("Hay demasiadas citas para consultar este rango de fechas.");
        }

        public async Task<CitaDto?> ObtenerCitaAsync(long id)
        {
            using var respuesta = await _http.GetAsync($"api/citas/{id}");

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
                return null;

            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<CitaDto>();
        }

        public async Task<CitaDto> CrearCitaAsync(CitaGuardarDto dto)
        {
            using var respuesta = await _http.PostAsJsonAsync("api/citas", dto);
            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<CitaDto>()
                ?? throw new InvalidOperationException("La API no devolvió la cita creada.");
        }

        public async Task ActualizarCitaAsync(long id, CitaGuardarDto dto)
        {
            using var respuesta = await _http.PutAsJsonAsync($"api/citas/{id}", dto);
            await ValidarAsync(respuesta);
        }

        public async Task CambiarEstadoCitaAsync(long id, string estado, string motivo, string version)
        {
            var datos = new { NuevoEstado = estado, Motivo = motivo, Version = version };

            using var respuesta = await _http.PatchAsync(
                $"api/citas/{id}/estado", JsonContent.Create(datos));
            await ValidarAsync(respuesta);
        }

        public Task<List<CambioCitaDto>> CambiosCitaAsync(long citaId) =>
            LeerTodoAsync<CambioCitaDto>($"api/cambios-cita?citaId={citaId}&");

        public Task<List<HorarioDto>> ListarHorariosAsync() =>
            LeerTodoAsync<HorarioDto>("api/horarios-atencion?incluirInactivos=true&");

        public Task CrearHorarioAsync(HorarioGuardarDto dto) => EnviarAsync(HttpMethod.Post, "api/horarios-atencion", dto);

        public Task ActualizarHorarioAsync(int id, HorarioGuardarDto dto) =>
            EnviarAsync(HttpMethod.Put, $"api/horarios-atencion/{id}", dto);

        public Task DesactivarHorarioAsync(int id) =>
            EnviarAsync(HttpMethod.Delete, $"api/horarios-atencion/{id}", null);

        public Task ReactivarHorarioAsync(int id) =>
            EnviarAsync(HttpMethod.Patch, $"api/horarios-atencion/{id}/reactivar", null);

        public Task<List<BloqueoDto>> ListarBloqueosAsync() =>
            LeerTodoAsync<BloqueoDto>("api/bloqueos-agenda?incluirInactivos=true&");

        public Task CrearBloqueoAsync(BloqueoGuardarDto dto) => EnviarAsync(HttpMethod.Post, "api/bloqueos-agenda", dto);

        public Task ActualizarBloqueoAsync(long id, BloqueoGuardarDto dto) =>
            EnviarAsync(HttpMethod.Put, $"api/bloqueos-agenda/{id}", dto);

        public Task DesactivarBloqueoAsync(long id) =>
            EnviarAsync(HttpMethod.Delete, $"api/bloqueos-agenda/{id}", null);

        public Task ReactivarBloqueoAsync(long id) =>
            EnviarAsync(HttpMethod.Patch, $"api/bloqueos-agenda/{id}/reactivar", null);

        private async Task<List<PacienteDto>> ListarPacientesAsync()
        {
            var pacientes = new List<PacienteDto>();

            for (var pagina = 1; pagina <= MaximoLotes; pagina++)
            {
                var lote = await _pacientes.ListarAsync(true, pagina, TamanoLote);
                pacientes.AddRange(lote);

                if (lote.Count < TamanoLote)
                    break;
            }

            return pacientes;
        }

        private async Task<List<T>> LeerTodoAsync<T>(string rutaConConsulta)
        {
            var resultado = new List<T>();

            for (var pagina = 1; pagina <= MaximoLotes; pagina++)
            {
                var lote = await LeerAsync<List<T>>($"{rutaConConsulta}pagina={pagina}&tamano={TamanoLote}") ?? [];
                resultado.AddRange(lote);

                if (lote.Count < TamanoLote)
                    break;
            }

            return resultado;
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
                HttpStatusCode.Forbidden => "No tienes permisos para esta operación.",
                HttpStatusCode.NotFound => "No se encontró el registro solicitado.",
                HttpStatusCode.Conflict => "La operación no está permitida en el estado actual del registro.",
                _ => $"No se pudo completar la operación. Código HTTP: {(int)respuesta.StatusCode}."
            });
        }
        private static string? LeerMensaje(string contenido)
        {
            if (string.IsNullOrWhiteSpace(contenido))
                return null;

            var texto = contenido.Trim();

            if (!texto.StartsWith('{') && !texto.StartsWith('"'))
                return texto.Length <= 300 && !texto.StartsWith('<') ? texto : null;

            try
            {
                using var documento = JsonDocument.Parse(texto);
                var raiz = documento.RootElement;

                if (raiz.ValueKind == JsonValueKind.String)
                    return raiz.GetString();

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
