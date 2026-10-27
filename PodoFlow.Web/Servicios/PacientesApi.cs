
using System.Net;
using System.Net.Http.Json;
using PodoFlow.Web.Modelos.Pacientes;

namespace PodoFlow.Web.Servicios;

public class PacientesApi
{
    private readonly HttpClient _http;

    public PacientesApi(HttpClient http)
    {
        _http = http;
    }

    // ===========================
    // PACIENTES
    // ===========================

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


    public async Task<PacienteDto> CrearAsync(
        PacienteGuardarDto dto)
    {
        using var respuesta = await _http.PostAsJsonAsync(
            "api/pacientes", dto);

        if (respuesta.StatusCode ==
            System.Net.HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException(
                "Ya existe un paciente con esa identificación. " +
                "Busca su ficha en el directorio para evitar duplicados.");
        }

        if (respuesta.StatusCode ==
            System.Net.HttpStatusCode.BadRequest)
        {
            throw new InvalidOperationException(
                "Los datos del paciente no cumplen las " +
                "validaciones del sistema. Revisa el formulario.");
        }

        if (respuesta.StatusCode ==
            System.Net.HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "Debes iniciar sesión para registrar pacientes.");
        }

        if (respuesta.StatusCode ==
            System.Net.HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException(
                "No tienes permisos para registrar pacientes.");
        }

        if (!respuesta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"No se pudo registrar el paciente. " +
                $"Respuesta HTTP: {(int)respuesta.StatusCode}.");
        }

        return await respuesta.Content
            .ReadFromJsonAsync<PacienteDto>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió los datos del paciente.");
    }


    public async Task ActualizarAsync(
        long id,
        PacienteGuardarDto dto)
    {
        using var respuesta = await _http.PutAsJsonAsync(
            $"api/pacientes/{id}", dto);

        await ValidarRespuestaAsync(respuesta);
    }

    public async Task DesactivarAsync(
        long id,
        string motivo)
    {
        string url = $"api/pacientes/{id}?motivo=" +
            Uri.EscapeDataString(motivo);

        using var respuesta = await _http.DeleteAsync(url);

        await ValidarRespuestaAsync(respuesta);
    }

    public async Task ReactivarAsync(long id)
    {
        using var respuesta = await _http.PatchAsync(
            $"api/pacientes/{id}/reactivar",
            content: null);

        await ValidarRespuestaAsync(respuesta);
    }

    // ===========================
    // CONTACTOS
    // ===========================

    public async Task<List<ContactoPacienteDto>> ContactosAsync(
        long pacienteId)
    {
        string url =
            $"api/datos-contacto?pacienteId={pacienteId}" +
            "&incluirInactivos=true&pagina=1&tamano=100";

        using var respuesta = await _http.GetAsync(url);
        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<List<ContactoPacienteDto>>() ?? [];
    }

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
                "La API no devolvió el contacto creado.");
    }

    public async Task ActualizarContactoAsync(
        long contactoId,
        ContactoGuardarDto dto)
    {
        using var respuesta = await _http.PutAsJsonAsync(
            $"api/datos-contacto/{contactoId}", dto);

        await ValidarRespuestaAsync(respuesta);
    }

    public async Task DesactivarContactoAsync(long contactoId)
    {
        using var respuesta = await _http.DeleteAsync(
            $"api/datos-contacto/{contactoId}");

        await ValidarRespuestaAsync(respuesta);
    }

    public async Task ReactivarContactoAsync(long contactoId)
    {
        using var respuesta = await _http.PatchAsync(
            $"api/datos-contacto/{contactoId}/reactivar",
            content: null);

        await ValidarRespuestaAsync(respuesta);
    }

    // ===========================
    // CITAS
    // ===========================

    public async Task<List<CitaPacienteDto>> CitasAsync(
        long pacienteId)
    {
        var resultado = new List<CitaPacienteDto>();

        // Límite provisional.
        // Sustituiremos esto por paginación visual.
        for (int pagina = 1; pagina <= 50; pagina++)
        {
            string url =
                $"api/citas?pacienteId={pacienteId}" +
                $"&pagina={pagina}&tamano=100";

            using var respuesta = await _http.GetAsync(url);
            await ValidarRespuestaAsync(respuesta);

            var lote = await respuesta.Content
                .ReadFromJsonAsync<List<CitaPacienteDto>>() ?? [];

            resultado.AddRange(lote);

            if (lote.Count < 100)
                return resultado;
        }

        throw new InvalidOperationException(
            "Se alcanzó el límite de citas por consulta.");
    }

    // ===========================
    // PRESTACIONES
    // ===========================

    public async Task<List<PrestacionPacienteDto>>
        PrestacionesAsync(long pacienteId)
    {
        var resultado = new List<PrestacionPacienteDto>();

        for (int pagina = 1; pagina <= 50; pagina++)
        {
            string url =
                $"api/prestaciones?pacienteId={pacienteId}" +
                $"&pagina={pagina}&tamano=100";

            using var respuesta = await _http.GetAsync(url);
            await ValidarRespuestaAsync(respuesta);

            var lote = await respuesta.Content
                .ReadFromJsonAsync<List<PrestacionPacienteDto>>() ?? [];

            resultado.AddRange(lote);

            if (lote.Count < 100)
                return resultado;
        }

        throw new InvalidOperationException(
            "Se alcanzó el límite de prestaciones por consulta.");
    }

    // ===========================
    // ERRORES HTTP
    // ===========================

    private static Task ValidarRespuestaAsync(
        HttpResponseMessage respuesta)
    {
        if (respuesta.IsSuccessStatusCode)
            return Task.CompletedTask;

        string mensaje = respuesta.StatusCode switch
        {
            HttpStatusCode.BadRequest =>
                "Revisa los datos ingresados.",

            HttpStatusCode.Unauthorized =>
                "Debes iniciar sesión para continuar.",

            HttpStatusCode.Forbidden =>
                "No tienes permisos para esta operación.",

            HttpStatusCode.NotFound =>
                "No se encontró el registro solicitado.",

            HttpStatusCode.Conflict =>
                "Hay un registro duplicado o los datos " +
                "fueron modificados por otro proceso.",

            _ =>
                "No se pudo completar la operación. " +
                "Código HTTP: " +
                (int)respuesta.StatusCode
        };

        throw new InvalidOperationException(mensaje);
    }
}
