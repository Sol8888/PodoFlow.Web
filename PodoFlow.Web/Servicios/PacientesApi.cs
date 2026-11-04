
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PodoFlow.Web.Modelos.Pacientes;
using PodoFlow.Web.Modelos.Atenciones;

namespace PodoFlow.Web.Servicios;

public class PacientesApi : IPacientesApi
{
    private readonly HttpClient _http;

    public PacientesApi(HttpClient http)
    {
        _http = http;
    }

    // ============================================
    // PACIENTES
    // ============================================

    // GET: api/pacientes
    public async Task<List<PacienteDto>> ListarAsync(
        bool incluirInactivos = false,
        int pagina = 1,
        int tamano = 10)
    {
        ValidarPaginacion(pagina, tamano);

        string url =
            $"api/pacientes" +
            $"?incluirInactivos={incluirInactivos.ToString().ToLowerInvariant()}" +
            $"&pagina={pagina}" +
            $"&tamano={tamano}";

        using var respuesta = await EjecutarAsync(
            () => _http.GetAsync(url));

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<List<PacienteDto>>() ?? [];
    }

    // GET: api/pacientes/{id}
    public async Task<PacienteDto?> ObtenerAsync(long id)
    {
        ValidarId(id);

        using var respuesta = await EjecutarAsync(
            () => _http.GetAsync($"api/pacientes/{id}"));

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
            return null;

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<PacienteDto>();
    }

    // POST: api/pacientes
    public async Task<PacienteDto> CrearAsync(
        PacienteGuardarDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        dto.Normalizar();

        ValidarModelo(dto);

        using var respuesta = await EjecutarAsync(
            () => _http.PostAsJsonAsync("api/pacientes", dto));

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<PacienteDto>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió los datos del paciente creado.");
    }

    // PUT: api/pacientes/{id}
    public async Task ActualizarAsync(
        long id,
        PacienteGuardarDto dto)
    {
        ValidarId(id);
        ArgumentNullException.ThrowIfNull(dto);

        dto.Normalizar();

        ValidarModelo(dto);

        if (string.IsNullOrWhiteSpace(dto.Version))
        {
            throw new InvalidOperationException(
                "No se encontró la versión del paciente. " +
                "Actualiza la ficha antes de guardar los cambios.");
        }

        using var respuesta = await EjecutarAsync(
            () => _http.PutAsJsonAsync(
                $"api/pacientes/{id}", dto));

        await ValidarRespuestaAsync(respuesta);
    }

    // DELETE: api/pacientes/{id}?motivo=...
    // Baja lógica: no elimina físicamente al paciente.
    public async Task DesactivarAsync(
        long id,
        string motivo)
    {
        ValidarId(id);

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ArgumentException(
                "Debes ingresar el motivo de desactivación.",
                nameof(motivo));
        }

        string motivoNormalizado = motivo.Trim();

        string url =
            $"api/pacientes/{id}?motivo=" +
            Uri.EscapeDataString(motivoNormalizado);

        using var respuesta = await EjecutarAsync(
            () => _http.DeleteAsync(url));

        await ValidarRespuestaAsync(respuesta);
    }

    // PATCH: api/pacientes/{id}/reactivar
    public async Task ReactivarAsync(long id)
    {
        ValidarId(id);

        using var respuesta = await EjecutarAsync(
            () => _http.PatchAsync(
                $"api/pacientes/{id}/reactivar",
                content: null));

        await ValidarRespuestaAsync(respuesta);
    }

    // ============================================
    // DATOS DE CONTACTO
    // ============================================

    // GET: api/datos-contacto
    public async Task<List<ContactoPacienteDto>> ContactosAsync(
        long pacienteId)
    {
        ValidarId(pacienteId);

        string url =
            $"api/datos-contacto" +
            $"?pacienteId={pacienteId}" +
            $"&incluirInactivos=true" +
            $"&pagina=1" +
            $"&tamano=100";

        using var respuesta = await EjecutarAsync(
            () => _http.GetAsync(url));

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<List<ContactoPacienteDto>>() ?? [];
    }

    // POST: api/datos-contacto
    public async Task<ContactoPacienteDto> CrearContactoAsync(
        long pacienteId,
        ContactoGuardarDto dto)
    {
        ValidarId(pacienteId);
        ArgumentNullException.ThrowIfNull(dto);

        dto.Normalizar();

        ValidarModelo(dto);

        var datos = new
        {
            PacienteId = pacienteId,
            dto.Tipo,
            dto.Valor,
            dto.Principal
        };

        using var respuesta = await EjecutarAsync(
            () => _http.PostAsJsonAsync(
                "api/datos-contacto", datos));

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<ContactoPacienteDto>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió el contacto creado.");
    }

    // PUT: api/datos-contacto/{id}
    public async Task ActualizarContactoAsync(
        long contactoId,
        ContactoGuardarDto dto)
    {
        ValidarId(contactoId);
        ArgumentNullException.ThrowIfNull(dto);

        dto.Normalizar();

        ValidarModelo(dto);

        using var respuesta = await EjecutarAsync(
            () => _http.PutAsJsonAsync(
                $"api/datos-contacto/{contactoId}", dto));

        await ValidarRespuestaAsync(respuesta);
    }

    // DELETE: api/datos-contacto/{id}
    public async Task DesactivarContactoAsync(long contactoId)
    {
        ValidarId(contactoId);

        using var respuesta = await EjecutarAsync(
            () => _http.DeleteAsync(
                $"api/datos-contacto/{contactoId}"));

        await ValidarRespuestaAsync(respuesta);
    }

    // PATCH: api/datos-contacto/{id}/reactivar
    public async Task ReactivarContactoAsync(long contactoId)
    {
        ValidarId(contactoId);

        using var respuesta = await EjecutarAsync(
            () => _http.PatchAsync(
                $"api/datos-contacto/{contactoId}/reactivar",
                content: null));

        await ValidarRespuestaAsync(respuesta);
    }

    // ============================================
    // CITAS DEL PACIENTE
    // ============================================

    public async Task<List<CitaPacienteDto>> CitasAsync(
        long pacienteId)
    {
        ValidarId(pacienteId);

        var resultado = new List<CitaPacienteDto>();

        const int tamano = 100;
        const int maxPaginas = 50;

        for (int pagina = 1; pagina <= maxPaginas; pagina++)
        {
            string url =
                $"api/citas" +
                $"?pacienteId={pacienteId}" +
                $"&pagina={pagina}" +
                $"&tamano={tamano}";

            using var respuesta = await EjecutarAsync(
                () => _http.GetAsync(url));

            await ValidarRespuestaAsync(respuesta);

            var lote = await respuesta.Content
                .ReadFromJsonAsync<List<CitaPacienteDto>>() ?? [];

            resultado.AddRange(lote);

            if (lote.Count < tamano)
                return resultado;
        }

        throw new InvalidOperationException(
            "Se alcanzó el límite de citas por consulta. " +
            "Es necesario implementar paginación visual.");
    }

    // ============================================
    // PRESTACIONES DEL PACIENTE
    // ============================================

    public async Task<List<PrestacionPacienteDto>> PrestacionesAsync(
        long pacienteId)
    {
        ValidarId(pacienteId);

        var resultado = new List<PrestacionPacienteDto>();

        const int tamano = 100;
        const int maxPaginas = 50;

        for (int pagina = 1; pagina <= maxPaginas; pagina++)
        {
            string url =
                $"api/prestaciones" +
                $"?pacienteId={pacienteId}" +
                $"&pagina={pagina}" +
                $"&tamano={tamano}";

            using var respuesta = await EjecutarAsync(
                () => _http.GetAsync(url));

            await ValidarRespuestaAsync(respuesta);

            var lote = await respuesta.Content
                .ReadFromJsonAsync<List<PrestacionPacienteDto>>() ?? [];

            resultado.AddRange(lote);

            if (lote.Count < tamano)
                return resultado;
        }

        throw new InvalidOperationException(
            "Se alcanzó el límite de prestaciones por consulta. " +
            "Es necesario implementar paginación visual.");
    }

    // ============================================
    // VALIDACIONES LOCALES
    // ============================================

    private static void ValidarId(long id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "El identificador debe ser mayor que cero.");
        }
    }

    private static void ValidarPaginacion(int pagina, int tamano)
    {
        if (pagina < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pagina),
                "La página debe ser mayor o igual a uno.");
        }

        if (tamano < 1 || tamano > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tamano),
                "El tamaño de página debe estar entre 1 y 100.");
        }
    }

    private static void ValidarModelo<T>(T modelo)
        where T : class
    {
        var contexto =
            new System.ComponentModel.DataAnnotations.ValidationContext(
                modelo);

        var resultados =
            new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        bool valido =
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                modelo,
                contexto,
                resultados,
                validateAllProperties: true);

        if (!valido)
        {
            string mensaje = string.Join(
                " ",
                resultados
                    .Select(x => x.ErrorMessage)
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(mensaje)
                    ? "Revisa los datos ingresados."
                    : mensaje);
        }
    }

    // ============================================
    // ERRORES DE CONEXIÓN
    // ============================================

    private static async Task<HttpResponseMessage> EjecutarAsync(
        Func<Task<HttpResponseMessage>> solicitud)
    {
        try
        {
            return await solicitud();
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                "No fue posible conectar con PodoFlow.Api. " +
                "Verifica que el backend esté ejecutándose " +
                "y que la URL esté correctamente configurada.",
                ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new InvalidOperationException(
                "La solicitud tardó demasiado o fue cancelada. " +
                "Intenta nuevamente.",
                ex);
        }
    }

    // ============================================
    // MANEJO CENTRALIZADO DE ERRORES HTTP
    // ============================================

    private static async Task ValidarRespuestaAsync(
        HttpResponseMessage respuesta)
    {
        if (respuesta.IsSuccessStatusCode)
            return;

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "Tu sesión no es válida o ha expirado. " +
                "Inicia sesión nuevamente.");
        }

        if (respuesta.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException(
                "No tienes permisos para realizar esta operación.");
        }

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                "No se encontró el registro solicitado.");
        }

        string contenido =
            await respuesta.Content.ReadAsStringAsync();

        if (respuesta.StatusCode == HttpStatusCode.BadRequest ||
            respuesta.StatusCode == HttpStatusCode.Conflict ||
            (int)respuesta.StatusCode == 422)
        {
            string? mensajeApi = ExtraerMensaje(contenido);

            string mensajePredeterminado =
                respuesta.StatusCode == HttpStatusCode.Conflict
                    ? "El registro ya existe o fue modificado por otra persona."
                    : "Revisa los datos ingresados.";

            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(mensajeApi)
                    ? mensajePredeterminado
                    : mensajeApi);
        }

        throw new InvalidOperationException(
            "No se pudo completar la operación. " +
            $"Código HTTP: {(int)respuesta.StatusCode}.");
    }

    // ============================================
    // EXTRAER MENSAJES DE LA API
    // ============================================

    private static string? ExtraerMensaje(string contenido)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            return null;

        string texto = contenido.Trim();

        try
        {
            using var documento = JsonDocument.Parse(texto);

            JsonElement raiz = documento.RootElement;

            if (raiz.ValueKind == JsonValueKind.String)
                return LimitarMensaje(raiz.GetString());

            if (raiz.ValueKind == JsonValueKind.Object)
            {
                foreach (string propiedad in new[]
                {
                    "mensaje",
                    "message",
                    "detail",
                    "title"
                })
                {
                    if (raiz.TryGetProperty(
                            propiedad,
                            out var valor) &&
                        valor.ValueKind == JsonValueKind.String)
                    {
                        return LimitarMensaje(valor.GetString());
                    }
                }

                // Respuestas de validación de ASP.NET Core
                // con estructura ValidationProblemDetails.
                if (raiz.TryGetProperty(
                        "errors",
                        out var errores) &&
                    errores.ValueKind == JsonValueKind.Object)
                {
                    var mensajes = new List<string>();

                    foreach (var campo in errores.EnumerateObject())
                    {
                        if (campo.Value.ValueKind !=
                            JsonValueKind.Array)
                            continue;

                        foreach (var error in
                                 campo.Value.EnumerateArray())
                        {
                            if (error.ValueKind ==
                                JsonValueKind.String)
                            {
                                string? mensaje = error.GetString();

                                if (!string.IsNullOrWhiteSpace(mensaje))
                                    mensajes.Add(mensaje);
                            }
                        }
                    }

                    if (mensajes.Count > 0)
                    {
                        return LimitarMensaje(
                            string.Join(" ", mensajes));
                    }
                }
            }
        }
        catch (JsonException)
        {
            // La API puede responder con texto plano.
            // Evitamos mostrar HTML o errores internos.
        }

        if (texto.StartsWith("<", StringComparison.Ordinal))
            return null;

        return LimitarMensaje(texto.Trim('"'));
    }

    private static string? LimitarMensaje(string? mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
            return null;

        string texto = mensaje.Trim();

        return texto.Length <= 300
            ? texto
            : texto[..300] + "...";
    }

    // ============================================
    // HISTORIA CLÍNICA DEL PACIENTE
    // ============================================

    // GET: api/historias/paciente/{pacienteId}
    public async Task<HistoriaPacienteDto?>
        ObtenerHistoriaPacienteAsync(long pacienteId)
    {
        ValidarId(pacienteId);

        using var respuesta = await EjecutarAsync(
            () => _http.GetAsync(
                $"api/historias/paciente/{pacienteId}"));

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
            return null;

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<HistoriaPacienteDto>();
    }

    // ============================================
    // ANTECEDENTES DE LA HISTORIA
    // ============================================

    // GET: api/antecedentes
    public async Task<List<AntecedenteDto>> ListarAntecedentesAsync(
        long historiaId,
        bool incluirInactivos = false,
        int pagina = 1,
        int tamano = 10)
    {
        ValidarId(historiaId);
        ValidarPaginacion(pagina, tamano);

        string url =
            $"api/antecedentes" +
            $"?historiaId={historiaId}" +
            $"&incluirInactivos={incluirInactivos.ToString().ToLowerInvariant()}" +
            $"&pagina={pagina}" +
            $"&tamano={tamano}";

        using var respuesta = await EjecutarAsync(
            () => _http.GetAsync(url));

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<List<AntecedenteDto>>() ?? [];
    }


    // ============================================
    // CREAR ANTECEDENTE
    // ============================================

    public async Task<AntecedenteDto> CrearAntecedenteAsync(
        long historiaId,
        AntecedenteGuardarDto dto)
    {
        ValidarId(historiaId);
        ArgumentNullException.ThrowIfNull(dto);

        dto.Normalizar();
        ValidarModelo(dto);

        var datos = new
        {
            HistoriaId = historiaId,
            dto.Tipo,
            dto.Descripcion
        };

        using var respuesta = await EjecutarAsync(
            () => _http.PostAsJsonAsync(
                "api/antecedentes", datos));

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<AntecedenteDto>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió el antecedente creado.");
    }

    // ============================================
    // ACTUALIZAR ANTECEDENTE
    // ============================================

    public async Task ActualizarAntecedenteAsync(
        long antecedenteId,
        AntecedenteGuardarDto dto)
    {
        ValidarId(antecedenteId);
        ArgumentNullException.ThrowIfNull(dto);

        dto.Normalizar();
        ValidarModelo(dto);

        var datos = new
        {
            dto.Tipo,
            dto.Descripcion
        };

        using var respuesta = await EjecutarAsync(
            () => _http.PutAsJsonAsync(
                $"api/antecedentes/{antecedenteId}",
                datos));

        await ValidarRespuestaAsync(respuesta);
    }

    // ============================================
    // DESACTIVAR ANTECEDENTE
    // ============================================

    public async Task DesactivarAntecedenteAsync(
        long antecedenteId)
    {
        ValidarId(antecedenteId);

        using var respuesta = await EjecutarAsync(
            () => _http.DeleteAsync(
                $"api/antecedentes/{antecedenteId}"));

        await ValidarRespuestaAsync(respuesta);
    }

    // ============================================
    // REACTIVAR ANTECEDENTE
    // ============================================

    public async Task ReactivarAntecedenteAsync(
        long antecedenteId)
    {
        ValidarId(antecedenteId);

        using var respuesta = await EjecutarAsync(
            () => _http.PatchAsync(
                $"api/antecedentes/{antecedenteId}/reactivar",
                content: null));

        await ValidarRespuestaAsync(respuesta);
    }

    public async Task<HistoriaPacienteDto> CrearHistoriaPacienteAsync(long pacienteId)
    {
        ValidarId(pacienteId);

        var datos = new
        {
            PacienteId = pacienteId
        };

        using var respuesta = await EjecutarAsync(
            () => _http.PostAsJsonAsync("api/historias", datos));

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<HistoriaPacienteDto>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió la historia clínica creada.");
    }

    public async Task<List<AtencionDto>> ListarAtencionesAsync(
        long historiaId,
        int pagina = 1,
        int tamano = 10)
    {
        ValidarId(historiaId);
        ValidarPaginacion(pagina, tamano);

        string url =
            $"api/atenciones?historiaId={historiaId}" +
            $"&pagina={pagina}&tamano={tamano}";

        using var respuesta = await EjecutarAsync(
            () => _http.GetAsync(url));

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<List<AtencionDto>>() ?? [];
    }


    public async Task<AtencionDto> CrearAtencionAsync(
        long historiaId,
        AtencionCrearDto dto)
    {
        ValidarId(historiaId);

        ArgumentNullException.ThrowIfNull(dto);

        if (dto.UsuarioId <= 0)
        {
            throw new ArgumentException(
                "Selecciona un profesional responsable válido.");
        }

        if (string.IsNullOrWhiteSpace(dto.MotivoConsulta))
        {
            throw new ArgumentException(
                "El motivo de consulta es obligatorio.");
        }

        var datos = new
        {
            HistoriaId = historiaId,
            CitaId = (long?)null,
            dto.UsuarioId,
            MotivoConsulta = dto.MotivoConsulta.Trim(),
            ValoracionClinica = dto.ValoracionClinica?.Trim(),
            Diagnostico = dto.Diagnostico?.Trim(),
            Tratamiento = dto.Tratamiento?.Trim(),
            Observaciones = dto.Observaciones?.Trim()
        };

        using var respuesta = await EjecutarAsync(
            () => _http.PostAsJsonAsync(
                "api/atenciones",
                datos));

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<AtencionDto>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió la atención creada.");
    }


}

