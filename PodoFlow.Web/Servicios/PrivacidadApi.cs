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
        private const int MaximoRegistros = 1000;
        private const int TamanoLote = 100;

        private static readonly TimeZoneInfo ZonaEcuador = ObtenerZonaEcuador();

        private readonly HttpClient _http;
        private readonly IPacientesApi _pacientes;
        private readonly long? _usuarioId;

        public PrivacidadApi(HttpClient http, IPacientesApi pacientes, IConfiguration configuration)
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

        public static DateOnly HoyEcuador()
        {
            return DateOnly.FromDateTime(HoraEcuador(DateTime.UtcNow));
        }

        public static DateTime UtcDesdeEcuador(DateTime horaEcuador)
        {
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(horaEcuador, DateTimeKind.Unspecified), ZonaEcuador);
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

        public async Task<List<FinalidadDto>> ListarFinalidadesAsync()
        {
            using var respuesta = await _http.GetAsync("api/Finalidad?incluirInactivos=true&pagina=1&tamano=100");
            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<List<FinalidadDto>>() ?? [];
        }

        public async Task CrearFinalidadAsync(FinalidadGuardarDto dto)
        {
            var datos = new
            {
                Codigo = dto.Codigo.Trim().ToUpperInvariant(),
                Nombre = dto.Nombre.Trim(),
                Descripcion = Limpiar(dto.Descripcion),
                dto.RequiereConsentimiento
            };

            using var respuesta = await _http.PostAsJsonAsync("api/Finalidad", datos);
            await ValidarAsync(respuesta);
        }

        public async Task ActualizarFinalidadAsync(int id, FinalidadGuardarDto dto)
        {
            var datos = new
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = Limpiar(dto.Descripcion),
                dto.RequiereConsentimiento
            };

            using var respuesta = await _http.PutAsJsonAsync($"api/Finalidad/{id}", datos);
            await ValidarAsync(respuesta);
        }

        public async Task CambiarEstatusFinalidadAsync(int id, bool activar)
        {
            using var respuesta = activar
                ? await _http.PatchAsync($"api/Finalidad/{id}/reactivar", null)
                : await _http.DeleteAsync($"api/Finalidad/{id}");
            await ValidarAsync(respuesta);
        }

        public async Task<List<AvisoDto>> ListarAvisosAsync()
        {
            using var respuesta = await _http.GetAsync("api/Aviso?incluirInactivos=true&pagina=1&tamano=100");
            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<List<AvisoDto>>() ?? [];
        }

        public async Task CrearAvisoAsync(AvisoGuardarDto dto)
        {
            var datos = new
            {
                Version = dto.Version.Trim(),
                dto.FechaDesde,
                dto.FechaHasta,
                RutaArchivo = Limpiar(dto.RutaArchivo),
                Huella = Limpiar(dto.Huella)?.ToLowerInvariant()
            };

            using var respuesta = await _http.PostAsJsonAsync("api/Aviso", datos);
            await ValidarAsync(respuesta);
        }

        public async Task ActualizarAvisoAsync(int id, AvisoGuardarDto dto)
        {
            var datos = new
            {
                dto.FechaDesde,
                dto.FechaHasta,
                RutaArchivo = Limpiar(dto.RutaArchivo),
                Huella = Limpiar(dto.Huella)?.ToLowerInvariant()
            };

            using var respuesta = await _http.PutAsJsonAsync($"api/Aviso/{id}", datos);
            await ValidarAsync(respuesta);
        }

        public async Task CambiarEstatusAvisoAsync(int id, bool activar)
        {
            using var respuesta = activar
                ? await _http.PatchAsync($"api/Aviso/{id}/reactivar", null)
                : await _http.DeleteAsync($"api/Aviso/{id}");
            await ValidarAsync(respuesta);
        }

        public async Task CrearConsentimientoAsync(ConsentimientoCrearDto dto)
        {
            dto.UsuarioId = _usuarioId;

            using var respuesta = await _http.PostAsJsonAsync("api/Consentimiento", dto);
            await ValidarAsync(respuesta);
        }

        public async Task<List<ConsentimientoDto>> ConsentimientosAsync(long pacienteId)
        {
            using var respuesta = await _http.GetAsync($"api/Consentimiento/paciente/{pacienteId}");
            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<List<ConsentimientoDto>>() ?? [];
        }

        public async Task<List<EventoConsentimientoDto>> EventosConsentimientoAsync(long consentimientoId)
        {
            using var respuesta = await _http.GetAsync($"api/Consentimiento/{consentimientoId}/eventos");
            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<List<EventoConsentimientoDto>>() ?? [];
        }

        public async Task CambiarEstadoConsentimientoAsync(long id, ConsentimientoCambiarEstadoDto dto)
        {
            dto.UsuarioId = _usuarioId;

            using var respuesta = await _http.PatchAsync(
                $"api/Consentimiento/{id}/estado", JsonContent.Create(dto));
            await ValidarAsync(respuesta);
        }

        private async Task<List<T>> ListarPaginadoAsync<T>(string ruta)
        {
            var separador = ruta.Contains('?') ? '&' : '?';
            var registros = new List<T>();

            for (var pagina = 1; registros.Count < MaximoRegistros; pagina++)
            {
                using var respuesta = await _http.GetAsync($"{ruta}{separador}pagina={pagina}&tamano={TamanoLote}");
                await ValidarAsync(respuesta);

                var lote = await respuesta.Content.ReadFromJsonAsync<List<T>>() ?? [];
                registros.AddRange(lote);

                if (lote.Count < TamanoLote)
                    break;
            }

            return registros;
        }

        private async Task EnviarAsync(Task<HttpResponseMessage> peticion)
        {
            using var respuesta = await peticion;
            await ValidarAsync(respuesta);
        }

        private Task CambiarEstatusAsync(string ruta, bool activar)
        {
            return EnviarAsync(activar
                ? _http.PatchAsync($"{ruta}/reactivar", null)
                : _http.DeleteAsync(ruta));
        }

        public Task<List<TratamientoDto>> ListarTratamientosAsync()
        {
            return ListarPaginadoAsync<TratamientoDto>("api/TratamientoDatos?incluirInactivos=true");
        }

        public Task CrearTratamientoAsync(TratamientoGuardarDto dto)
        {
            var datos = DatosTratamiento(dto, UsuarioId);

            return EnviarAsync(_http.PostAsJsonAsync("api/TratamientoDatos", new
            {
                Codigo = dto.Codigo.Trim().ToUpperInvariant(),
                datos.FinalidadId,
                datos.UsuarioId,
                datos.Nombre,
                datos.Modulo,
                datos.Descripcion,
                datos.CategoriasTitulares,
                datos.BaseLegitimacion,
                datos.PlazoConservacion,
                datos.MedidasSeguridad,
                datos.FechaRevision
            }));
        }

        public Task ActualizarTratamientoAsync(int id, TratamientoGuardarDto dto)
        {
            return EnviarAsync(_http.PutAsJsonAsync(
                $"api/TratamientoDatos/{id}", DatosTratamiento(dto, dto.UsuarioId ?? _usuarioId)));
        }

        public Task CambiarEstatusTratamientoAsync(int id, bool activar)
        {
            return CambiarEstatusAsync($"api/TratamientoDatos/{id}", activar);
        }

        private static TratamientoDatosEnvio DatosTratamiento(TratamientoGuardarDto dto, long? usuarioId) => new(
            dto.FinalidadId,
            usuarioId,
            dto.Nombre.Trim(),
            dto.Modulo.Trim().ToUpperInvariant(),
            Limpiar(dto.Descripcion),
            Limpiar(dto.CategoriasTitulares),
            Limpiar(dto.BaseLegitimacion),
            Limpiar(dto.PlazoConservacion),
            Limpiar(dto.MedidasSeguridad),
            dto.FechaRevision);

        private sealed record TratamientoDatosEnvio(
            int FinalidadId,
            long? UsuarioId,
            string Nombre,
            string Modulo,
            string? Descripcion,
            string? CategoriasTitulares,
            string? BaseLegitimacion,
            string? PlazoConservacion,
            string? MedidasSeguridad,
            DateOnly? FechaRevision);

        public Task<List<CategoriaDatoDto>> ListarCategoriasDatoAsync()
        {
            return ListarPaginadoAsync<CategoriaDatoDto>("api/CategoriaDato?incluirInactivos=true");
        }

        public Task CrearCategoriaDatoAsync(CategoriaDatoGuardarDto dto)
        {
            return EnviarAsync(_http.PostAsJsonAsync("api/CategoriaDato", DatosCategoria(dto)));
        }

        public Task ActualizarCategoriaDatoAsync(int id, CategoriaDatoGuardarDto dto)
        {
            return EnviarAsync(_http.PutAsJsonAsync($"api/CategoriaDato/{id}", DatosCategoria(dto)));
        }

        public Task CambiarEstatusCategoriaDatoAsync(int id, bool activar)
        {
            return CambiarEstatusAsync($"api/CategoriaDato/{id}", activar);
        }

        private static object DatosCategoria(CategoriaDatoGuardarDto dto) => new
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = Limpiar(dto.Descripcion),
            dto.EsSensible
        };

        public Task<List<EncargadoDto>> ListarEncargadosAsync()
        {
            return ListarPaginadoAsync<EncargadoDto>("api/Encargado?incluirInactivos=true");
        }

        public Task CrearEncargadoAsync(EncargadoGuardarDto dto)
        {
            return EnviarAsync(_http.PostAsJsonAsync("api/Encargado", DatosEncargado(dto)));
        }

        public Task ActualizarEncargadoAsync(int id, EncargadoGuardarDto dto)
        {
            return EnviarAsync(_http.PutAsJsonAsync($"api/Encargado/{id}", DatosEncargado(dto)));
        }

        public Task CambiarEstatusEncargadoAsync(int id, bool activar)
        {
            return CambiarEstatusAsync($"api/Encargado/{id}", activar);
        }

        private static object DatosEncargado(EncargadoGuardarDto dto) => new
        {
            Nombre = dto.Nombre.Trim(),
            Servicio = dto.Servicio.Trim(),
            RolTratamiento = Limpiar(dto.RolTratamiento),
            Descripcion = Limpiar(dto.Descripcion),
            Riesgo = Limpiar(dto.Riesgo),
            MedidasContractuales = Limpiar(dto.MedidasContractuales),
            MedidasTecnicas = Limpiar(dto.MedidasTecnicas)
        };

        public Task<List<TratamientoCategoriaDto>> CategoriasDeTratamientoAsync(int tratamientoId)
        {
            return ListarPaginadoAsync<TratamientoCategoriaDto>($"api/TratamientoCategoria?tratamientoId={tratamientoId}");
        }

        public Task AsignarCategoriaAsync(int tratamientoId, int categoriaId)
        {
            return EnviarAsync(_http.PostAsJsonAsync("api/TratamientoCategoria", new
            {
                TratamientoId = tratamientoId,
                CategoriaId = categoriaId
            }));
        }

        public Task QuitarCategoriaAsync(int tratamientoId, int categoriaId)
        {
            return EnviarAsync(_http.DeleteAsync($"api/TratamientoCategoria/{tratamientoId}/{categoriaId}"));
        }

        public Task<List<TratamientoEncargadoDto>> EncargadosDeTratamientoAsync(int tratamientoId)
        {
            return ListarPaginadoAsync<TratamientoEncargadoDto>($"api/TratamientoEncargado?tratamientoId={tratamientoId}");
        }

        public Task AsignarEncargadoAsync(int tratamientoId, int encargadoId)
        {
            return EnviarAsync(_http.PostAsJsonAsync("api/TratamientoEncargado", new
            {
                TratamientoId = tratamientoId,
                EncargadoId = encargadoId
            }));
        }

        public Task QuitarEncargadoAsync(int tratamientoId, int encargadoId)
        {
            return EnviarAsync(_http.DeleteAsync($"api/TratamientoEncargado/{tratamientoId}/{encargadoId}"));
        }

        public Task<List<RiesgoDto>> ListarRiesgosAsync()
        {
            return ListarPaginadoAsync<RiesgoDto>("api/Riesgo");
        }

        public Task CrearRiesgoAsync(RiesgoGuardarDto dto)
        {
            return EnviarAsync(_http.PostAsJsonAsync("api/Riesgo", new
            {
                dto.TratamientoId,
                Descripcion = dto.Descripcion.Trim(),
                dto.Probabilidad,
                dto.Impacto,
                MedidasMitigacion = Limpiar(dto.MedidasMitigacion),
                dto.RiesgoResidual,
                dto.FechaRevision
            }));
        }

        public Task ActualizarRiesgoAsync(long id, RiesgoGuardarDto dto)
        {
            return EnviarAsync(_http.PutAsJsonAsync($"api/Riesgo/{id}", new
            {
                Descripcion = dto.Descripcion.Trim(),
                dto.Probabilidad,
                dto.Impacto,
                MedidasMitigacion = Limpiar(dto.MedidasMitigacion),
                dto.RiesgoResidual,
                dto.FechaRevision
            }));
        }

        public Task CambiarEstadoRiesgoAsync(long id, string estado, string? medidasMitigacion, byte? riesgoResidual)
        {
            return EnviarAsync(_http.PatchAsync($"api/Riesgo/{id}/estado", JsonContent.Create(new
            {
                Estado = estado,
                MedidasMitigacion = Limpiar(medidasMitigacion),
                RiesgoResidual = riesgoResidual
            })));
        }

        public Task<List<EvaluacionImpactoDto>> ListarEvaluacionesAsync()
        {
            return ListarPaginadoAsync<EvaluacionImpactoDto>("api/EvaluacionImpacto");
        }

        public async Task<EvaluacionImpactoDto> CrearEvaluacionAsync(EvaluacionImpactoGuardarDto dto)
        {
            using var respuesta = await _http.PostAsJsonAsync("api/EvaluacionImpacto", new
            {
                UsuarioId,
                Version = dto.Version.Trim(),
                dto.Fecha,
                Alcance = dto.Alcance.Trim(),
                Descripcion = Limpiar(dto.Descripcion),
                Conclusion = Limpiar(dto.Conclusion),
                dto.NivelResidual
            });
            await ValidarAsync(respuesta);

            return await LeerAsync<EvaluacionImpactoDto>(respuesta);
        }

        public Task ActualizarEvaluacionAsync(long id, EvaluacionImpactoGuardarDto dto)
        {
            return EnviarAsync(_http.PutAsJsonAsync($"api/EvaluacionImpacto/{id}", new
            {
                Version = dto.Version.Trim(),
                dto.Fecha,
                Alcance = dto.Alcance.Trim(),
                Descripcion = Limpiar(dto.Descripcion),
                Conclusion = Limpiar(dto.Conclusion),
                dto.NivelResidual
            }));
        }

        public Task CambiarEstadoEvaluacionAsync(long id, string estado)
        {
            return EnviarAsync(_http.PatchAsync(
                $"api/EvaluacionImpacto/{id}/estado", JsonContent.Create(new { Estado = estado })));
        }

        public Task<List<EvaluacionRiesgoDto>> ListarRiesgosEvaluadosAsync()
        {
            return ListarPaginadoAsync<EvaluacionRiesgoDto>("api/EvaluacionRiesgo");
        }

        public Task AgregarRiesgoEvaluacionAsync(long evaluacionId, long riesgoId, string? observacion)
        {
            return EnviarAsync(_http.PostAsJsonAsync("api/EvaluacionRiesgo", new
            {
                EvaluacionId = evaluacionId,
                RiesgoId = riesgoId,
                Observacion = Limpiar(observacion)
            }));
        }

        public async Task<int> CopiarRiesgosEvaluacionAsync(long origenId, long destinoId, List<long> excluidos)
        {
            using var respuesta = await _http.PostAsJsonAsync("api/EvaluacionRiesgo/copiar", new
            {
                EvaluacionOrigenId = origenId,
                EvaluacionDestinoId = destinoId,
                RiesgosExcluidos = excluidos
            });
            await ValidarAsync(respuesta);

            return (await LeerAsync<List<EvaluacionRiesgoDto>>(respuesta)).Count;
        }

        public Task<List<DocumentoDto>> ListarDocumentosAsync()
        {
            return ListarPaginadoAsync<DocumentoDto>("api/Documento?incluirInactivos=true");
        }

        public Task CrearDocumentoAsync(DocumentoGuardarDto dto)
        {
            return EnviarAsync(_http.PostAsJsonAsync("api/Documento", new
            {
                UsuarioId,
                dto.Tipo,
                Version = Limpiar(dto.Version),
                Nombre = dto.Nombre.Trim(),
                RutaArchivo = dto.RutaArchivo.Trim(),
                Huella = Limpiar(dto.Huella)?.ToLowerInvariant()
            }));
        }

        public Task ActualizarDocumentoAsync(long id, DocumentoGuardarDto dto)
        {
            return EnviarAsync(_http.PutAsJsonAsync($"api/Documento/{id}", new
            {
                dto.Tipo,
                Version = Limpiar(dto.Version),
                Nombre = dto.Nombre.Trim()
            }));
        }

        public Task CambiarEstatusDocumentoAsync(long id, bool activar)
        {
            return CambiarEstatusAsync($"api/Documento/{id}", activar);
        }

        public Task<List<IncidenteDto>> ListarIncidentesAsync()
        {
            return ListarPaginadoAsync<IncidenteDto>("api/Incidente");
        }

        public async Task<IncidenteDto?> ObtenerIncidenteAsync(long id)
        {
            using var respuesta = await _http.GetAsync($"api/Incidente/{id}");

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
                return null;

            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<IncidenteDto>();
        }

        public async Task<IncidenteDto> CrearIncidenteAsync(IncidenteGuardarDto dto)
        {
            using var respuesta = await _http.PostAsJsonAsync("api/Incidente", new
            {
                UsuarioId,
                FechaDeteccion = UtcDesdeEcuador(dto.FechaDeteccion),
                Tipo = dto.Tipo.Trim().ToUpperInvariant(),
                Modulo = dto.Modulo.Trim().ToUpperInvariant(),
                Descripcion = dto.Descripcion.Trim(),
                DatosAfectados = Limpiar(dto.DatosAfectados),
                ImpactoPreliminar = Limpiar(dto.ImpactoPreliminar)
            });
            await ValidarAsync(respuesta);

            return await LeerAsync<IncidenteDto>(respuesta);
        }

        public Task ActualizarIncidenteAsync(long id, IncidenteGuardarDto dto, string version)
        {
            return EnviarAsync(_http.PutAsJsonAsync($"api/Incidente/{id}", new
            {
                FechaDeteccion = UtcDesdeEcuador(dto.FechaDeteccion),
                Tipo = dto.Tipo.Trim().ToUpperInvariant(),
                Modulo = dto.Modulo.Trim().ToUpperInvariant(),
                Descripcion = dto.Descripcion.Trim(),
                DatosAfectados = Limpiar(dto.DatosAfectados),
                ImpactoPreliminar = Limpiar(dto.ImpactoPreliminar),
                Version = version
            }));
        }

        public async Task<IncidenteDto> CambiarEstadoIncidenteAsync(
            long id,
            string estado,
            string? accion,
            string? observacion,
            string version)
        {
            using var respuesta = await _http.PatchAsync($"api/Incidente/{id}/estado", JsonContent.Create(new
            {
                Estado = estado,
                UsuarioId,
                Accion = Limpiar(accion),
                Observacion = Limpiar(observacion),
                Version = version
            }));
            await ValidarAsync(respuesta);

            return await LeerAsync<IncidenteDto>(respuesta);
        }

        public async Task<List<AccionIncidenteDto>> AccionesIncidenteAsync(long incidenteId)
        {
            using var respuesta = await _http.GetAsync($"api/Incidente/{incidenteId}/acciones");
            await ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<List<AccionIncidenteDto>>() ?? [];
        }

        public Task RegistrarAccionIncidenteAsync(long incidenteId, AccionIncidenteGuardarDto dto)
        {
            return EnviarAsync(_http.PostAsJsonAsync($"api/Incidente/{incidenteId}/acciones", new
            {
                UsuarioId,
                dto.Tipo,
                Accion = dto.Accion.Trim(),
                Observacion = Limpiar(dto.Observacion)
            }));
        }
    }
}
