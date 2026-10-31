using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PodoFlow.Web.Modelos.Pacientes;
using PodoFlow.Web.Modelos.Privacidad;

namespace PodoFlow.Web.Servicios
{
    public class PrivacidadApi : IPrivacidadApi
    {
        private const int MaximoPacientes = 500;
        private const int TamanoLotePacientes = 100;

        private static readonly TimeZoneInfo ZonaEcuador = ObtenerZonaEcuador();

        private readonly HttpClient _http;
        private readonly PacientesApi _pacientes;
        private readonly long? _usuarioId;

        public PrivacidadApi(HttpClient http, PacientesApi pacientes, IConfiguration configuration)
        {
            _http = http;
            _pacientes = pacientes;
            _usuarioId = long.TryParse(configuration["AutenticacionDesarrollo:UsuarioId"], out var id) && id > 0
                ? id
                : null;
        }

        private long UsuarioId => _usuarioId
            ?? throw new InvalidOperationException(
                "No está configurado el usuario de la sesión (AutenticacionDesarrollo:UsuarioId).");

        public async Task<List<PacienteDto>> ListarPacientesActivosAsync()
        {
            var pacientes = new List<PacienteDto>();

            for (var pagina = 1; pacientes.Count < MaximoPacientes; pagina++)
            {
                var lote = await _pacientes.ListarAsync(false, pagina, TamanoLotePacientes);
                pacientes.AddRange(lote);

                if (lote.Count < TamanoLotePacientes)
                    break;
            }

            return pacientes;
        }

        public Task<PacienteDto?> ObtenerPacienteAsync(long id)
        {
            return _pacientes.ObtenerAsync(id);
        }

        public async Task<List<SolicitudDto>> ListarSolicitudesAsync(int pagina, int tamano)
        {
            using var respuesta = await _http.GetAsync($"api/Solicitud?pagina={pagina}&tamano={tamano}");
            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<List<SolicitudDto>>() ?? [];
        }

        public async Task<SolicitudDto?> ObtenerSolicitudAsync(long id)
        {
            using var respuesta = await _http.GetAsync($"api/Solicitud/{id}");

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
                return null;

            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<SolicitudDto>();
        }

        public async Task<List<AccionSolicitudDto>> AccionesAsync(long solicitudId)
        {
            using var respuesta = await _http.GetAsync($"api/Solicitud/{solicitudId}/acciones");
            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<List<AccionSolicitudDto>>() ?? [];
        }

        public async Task<SolicitudDto> CrearSolicitudAsync(SolicitudGuardarDto dto)
        {
            var esRepresentante = dto.TipoSolicitante == "REPRESENTANTE";

            var datos = new
            {
                dto.PacienteId,
                UsuarioId,
                dto.TipoDerecho,
                dto.TipoSolicitante,
                NombreSolicitante = dto.NombreSolicitante.Trim(),
                TipoIdentificacion = esRepresentante ? dto.TipoIdentificacion : null,
                IdentificacionSolicitante = dto.IdentificacionSolicitante.Trim(),
                MedioRespuesta = Limpiar(dto.MedioRespuesta),
                Detalle = Limpiar(dto.Detalle)
            };

            using var respuesta = await _http.PostAsJsonAsync("api/Solicitud", datos);
            await ValidarAsync(respuesta);

            return await LeerAsync<SolicitudDto>(respuesta);
        }

        public async Task<SolicitudDto> CambiarEstadoSolicitudAsync(
            long id,
            string estado,
            string version,
            string? observacion,
            string? respuestaTitular,
            string? medioRespuesta)
        {
            var datos = new
            {
                Estado = estado,
                UsuarioId,
                Observacion = Limpiar(observacion),
                Respuesta = Limpiar(respuestaTitular),
                MedioRespuesta = Limpiar(medioRespuesta),
                Version = version
            };

            using var respuesta = await _http.PatchAsync(
                $"api/Solicitud/{id}/estado", JsonContent.Create(datos));
            await ValidarAsync(respuesta);

            return await LeerAsync<SolicitudDto>(respuesta);
        }

        public async Task<List<EvidenciaDto>> EvidenciasAsync(long solicitudId)
        {
            using var respuesta = await _http.GetAsync($"api/EvidenciaSolicitud/solicitud/{solicitudId}");
            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<List<EvidenciaDto>>() ?? [];
        }

        public async Task<EvidenciaDto> CrearEvidenciaAsync(long solicitudId, EvidenciaGuardarDto dto)
        {
            var datos = new
            {
                SolicitudId = solicitudId,
                UsuarioId,
                Tipo = dto.Tipo.Trim(),
                RutaArchivo = dto.RutaArchivo.Trim(),
                Huella = Limpiar(dto.Huella),
                Observacion = Limpiar(dto.Observacion)
            };

            using var respuesta = await _http.PostAsJsonAsync("api/EvidenciaSolicitud", datos);
            await ValidarAsync(respuesta);

            return await LeerAsync<EvidenciaDto>(respuesta);
        }

        public async Task<List<ExportacionDto>> ExportacionesAsync(long solicitudId)
        {
            using var respuesta = await _http.GetAsync($"api/Exportacion/solicitud/{solicitudId}");
            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<List<ExportacionDto>>() ?? [];
        }

        public async Task<ExportacionDto> GenerarExportacionAsync(
            long solicitudId,
            string formato,
            List<string>? secciones,
            string? observacion)
        {
            var datos = new
            {
                SolicitudId = solicitudId,
                UsuarioId,
                Formato = formato,
                Secciones = secciones,
                Observacion = Limpiar(observacion)
            };

            using var respuesta = await _http.PostAsJsonAsync("api/Exportacion", datos);
            await ValidarAsync(respuesta);

            return await LeerAsync<ExportacionDto>(respuesta);
        }

        public async Task<ExportacionDto> CambiarEstadoExportacionAsync(long id, string estado, string? observacion)
        {
            var datos = new
            {
                Estado = estado,
                UsuarioId,
                Observacion = Limpiar(observacion)
            };

            using var respuesta = await _http.PatchAsync(
                $"api/Exportacion/{id}/estado", JsonContent.Create(datos));
            await ValidarAsync(respuesta);

            return await LeerAsync<ExportacionDto>(respuesta);
        }

        public async Task<ArchivoExportacionDto> DescargarExportacionAsync(long id)
        {
            using var respuesta = await _http.GetAsync($"api/Exportacion/{id}/descargar?usuarioId={UsuarioId}");
            await ValidarAsync(respuesta);

            var cabecera = respuesta.Content.Headers.ContentDisposition;
            var nombre = (cabecera?.FileNameStar ?? cabecera?.FileName ?? $"exportacion-{id}").Trim('"');

            return new ArchivoExportacionDto
            {
                Contenido = await respuesta.Content.ReadAsByteArrayAsync(),
                NombreArchivo = Path.GetFileName(nombre),
                TipoContenido = respuesta.Content.Headers.ContentType?.MediaType ?? "application/octet-stream"
            };
        }

        public static DateTime HoraEcuador(DateTime fecha)
        {
            var utc = fecha.Kind switch
            {
                DateTimeKind.Utc => fecha,
                DateTimeKind.Local => fecha.ToUniversalTime(),
                _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
            };

            return TimeZoneInfo.ConvertTimeFromUtc(utc, ZonaEcuador);
        }

        private static async Task<T> LeerAsync<T>(HttpResponseMessage respuesta)
        {
            return await respuesta.Content.ReadFromJsonAsync<T>()
                ?? throw new InvalidOperationException("La API no devolvió los datos esperados.");
        }

        private static async Task ValidarAsync(HttpResponseMessage respuesta)
        {
            if (respuesta.IsSuccessStatusCode)
                return;

            var mensaje = await LeerMensajeAsync(respuesta);

            throw new InvalidOperationException(mensaje ?? respuesta.StatusCode switch
            {
                HttpStatusCode.BadRequest => "Revisa los datos ingresados.",
                HttpStatusCode.Unauthorized => "La API rechazó la sesión. Revisa el token de desarrollo de PodoFlow.Web.",
                HttpStatusCode.Forbidden => "No tienes permisos de privacidad para esta operación.",
                HttpStatusCode.NotFound => "No se encontró el registro solicitado.",
                HttpStatusCode.Conflict => "La operación no está permitida en el estado actual del registro.",
                _ => $"No se pudo completar la operación. Código HTTP: {(int)respuesta.StatusCode}."
            });
        }

        private static async Task<string?> LeerMensajeAsync(HttpResponseMessage respuesta)
        {
            var contenido = await respuesta.Content.ReadAsStringAsync();

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

        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static TimeZoneInfo ObtenerZonaEcuador()
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById("America/Guayaquil", out var zona))
                return zona;

            if (TimeZoneInfo.TryFindSystemTimeZoneById("SA Pacific Standard Time", out zona))
                return zona;

            return TimeZoneInfo.CreateCustomTimeZone("Ecuador", TimeSpan.FromHours(-5), "Ecuador", "Ecuador");
        }
    }
}
