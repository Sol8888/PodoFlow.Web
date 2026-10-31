

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PodoFlow.Web.Modelos.Pacientes;

namespace PodoFlow.Web.Servicios;

public class PacientesApi
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
        string url =
            $"api/pacientes" +
            $"?incluirInactivos={incluirInactivos.ToString().ToLowerInvariant()}" +
            $"&pagina={pagina}" +
            $"&tamano={tamano}";

        using var respuesta = await _http.GetAsync(url);

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<List<PacienteDto>>() ?? [];
    }

    // GET: api/pacientes/{id}
    public async Task<PacienteDto?> ObtenerAsync(long id)
    {
        using var respuesta =
            await _http.GetAsync($"api/pacientes/{id}");

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
        using var respuesta = await _http.PostAsJsonAsync(
            "api/pacientes", dto);

        // Si ocurre un 400 o 409, recuperamos
        // el mensaje que devolvió PacienteController.
        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<PacienteDto>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió los datos del paciente.");
    }

    // PUT: api/pacientes/{id}
    public async Task ActualizarAsync(
        long id,
        PacienteGuardarDto dto)
    {
        using var respuesta = await _http.PutAsJsonAsync(
            $"api/pacientes/{id}", dto);

        await ValidarRespuestaAsync(respuesta);
    }

    // DELETE: api/pacientes/{id}?motivo=...
    // Baja lógica, no eliminación física.
    public async Task DesactivarAsync(
        long id,
        string motivo)
    {
        string url =
            $"api/pacientes/{id}?motivo=" +
            Uri.EscapeDataString(motivo);

        using var respuesta = await _http.DeleteAsync(url);

        await ValidarRespuestaAsync(respuesta);
    }

    // PATCH: api/pacientes/{id}/reactivar
    public async Task ReactivarAsync(long id)
    {
        using var respuesta = await _http.PatchAsync(
            $"api/pacientes/{id}/reactivar",
            content: null);

        await ValidarRespuestaAsync(respuesta);
    }

    // ============================================
    // DATOS DE CONTACTO
    // ============================================

    // GET: api/datos-contacto
    public async Task<List<ContactoPacienteDto>> ContactosAsync(
        long pacienteId)
    {
        string url =
            $"api/datos-contacto" +
            $"?pacienteId={pacienteId}" +
            $"&incluirInactivos=true" +
            $"&pagina=1" +
            $"&tamano=100";

        using var respuesta = await _http.GetAsync(url);

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<List<ContactoPacienteDto>>() ?? [];
    }

    // POST: api/datos-contacto
    public async Task<ContactoPacienteDto> CrearContactoAsync(
        long pacienteId,
        ContactoGuardarDto dto)
    {
        var datos = new
        {
            PacienteId = pacienteId,
            dto.Tipo,
            dto.Valor,
            dto.Principal
        };

        using var respuesta = await _http.PostAsJsonAsync(
            "api/datos-contacto", datos);

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
        using var respuesta = await _http.PutAsJsonAsync(
            $"api/datos-contacto/{contactoId}", dto);

        await ValidarRespuestaAsync(respuesta);
    }

    // DELETE: api/datos-contacto/{id}
    public async Task DesactivarContactoAsync(long contactoId)
    {
        using var respuesta = await _http.DeleteAsync(
            $"api/datos-contacto/{contactoId}");

        await ValidarRespuestaAsync(respuesta);
    }

    // PATCH: api/datos-contacto/{id}/reactivar
    public async Task ReactivarContactoAsync(long contactoId)
    {
        using var respuesta = await _http.PatchAsync(
            $"api/datos-contacto/{contactoId}/reactivar",
            content: null);

        await ValidarRespuestaAsync(respuesta);
    }

    // ============================================
    // CITAS DEL PACIENTE
    // ============================================

    public async Task<List<CitaPacienteDto>> CitasAsync(
        long pacienteId)
    {
        var resultado = new List<CitaPacienteDto>();

        // Consulta provisional hasta implementar
        // paginación visual de las citas.
        for (int pagina = 1; pagina <= 50; pagina++)
        {
            string url =
                $"api/citas" +
                $"?pacienteId={pacienteId}" +
                $"&pagina={pagina}" +
                $"&tamano=100";

            using var respuesta = await _http.GetAsync(url);

            await ValidarRespuestaAsync(respuesta);

            var lote = await respuesta.Content
                .ReadFromJsonAsync<List<CitaPacienteDto>>() ?? [];

            resultado.AddRange(lote);

            if (lote.Count < 100)
                return resultado;
        }

        throw new InvalidOperationException(
            "Se alcanzó el límite de citas por consulta. " +
            "Es necesario utilizar paginación.");
    }

    // ============================================
    // PRESTACIONES DEL PACIENTE
    // ============================================

    public async Task<List<PrestacionPacienteDto>>
        PrestacionesAsync(long pacienteId)
    {
        var resultado = new List<PrestacionPacienteDto>();

        for (int pagina = 1; pagina <= 50; pagina++)
        {
            string url =
                $"api/prestaciones" +
                $"?pacienteId={pacienteId}" +
                $"&pagina={pagina}" +
                $"&tamano=100";

            using var respuesta = await _http.GetAsync(url);

            await ValidarRespuestaAsync(respuesta);

            var lote = await respuesta.Content
                .ReadFromJsonAsync<List<PrestacionPacienteDto>>() ?? [];

            resultado.AddRange(lote);

            if (lote.Count < 100)
                return resultado;
        }

        throw new InvalidOperationException(
            "Se alcanzó el límite de prestaciones por consulta. " +
            "Es necesario utilizar paginación.");
    }

    // ============================================
    // MANEJO CENTRALIZADO DE ERRORES HTTP
    // ============================================

    private static async Task ValidarRespuestaAsync(
        HttpResponseMessage respuesta)
    {
        // Una respuesta 2xx significa que la operación
        // HTTP se completó correctamente.
        if (respuesta.IsSuccessStatusCode)
            return;

        // Primero distinguimos errores de acceso.
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "Debes iniciar sesión para continuar.");
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

        // Leemos el contenido que devuelve la API.
        string contenido = await respuesta.Content
            .ReadAsStringAsync();

        // Solamente mostramos mensajes de validación
        // y reglas de negocio en respuestas 400 y 409.
        if (respuesta.StatusCode == HttpStatusCode.BadRequest ||
            respuesta.StatusCode == HttpStatusCode.Conflict)
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

        // No mostramos directamente contenido HTTP 500,
        // ya que puede contener información interna.
        throw new InvalidOperationException(
            $"No se pudo completar la operación. " +
            $"Código HTTP: {(int)respuesta.StatusCode}.");
    }

    // Admite texto plano, cadenas JSON
    // y respuestas JSON con una propiedad "mensaje".
    private static string? ExtraerMensaje(string contenido)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            return null;

        string texto = contenido.Trim();

        try
        {
            using var documento = JsonDocument.Parse(texto);
            var raiz = documento.RootElement;

            if (raiz.ValueKind == JsonValueKind.String)
                return LimitarMensaje(raiz.GetString());

            if (raiz.ValueKind == JsonValueKind.Object)
            {
                if (raiz.TryGetProperty("mensaje", out var mensaje) &&
                    mensaje.ValueKind == JsonValueKind.String)
                {
                    return LimitarMensaje(mensaje.GetString());
                }

                if (raiz.TryGetProperty("message", out var message) &&
                    message.ValueKind == JsonValueKind.String)
                {
                    return LimitarMensaje(message.GetString());
                }

                if (raiz.TryGetProperty("title", out var titulo) &&
                    titulo.ValueKind == JsonValueKind.String)
                {
                    return LimitarMensaje(titulo.GetString());
                }
            }
        }
        catch (JsonException)
        {
            // El backend también puede enviar texto plano.
        }

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
}
