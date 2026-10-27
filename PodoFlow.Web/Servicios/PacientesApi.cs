
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

    public async Task<List<PacienteDto>> ListarAsync(
        bool incluirInactivos = false,
        int pagina = 1,
        int tamano = 10)
    {
        string url =
            $"api/pacientes?" +
            $"incluirInactivos={incluirInactivos.ToString().ToLowerInvariant()}" +
            $"&pagina={pagina}" +
            $"&tamano={tamano}";

        return await _http
            .GetFromJsonAsync<List<PacienteDto>>(url)
            ?? new List<PacienteDto>();
    }

    public async Task<PacienteDto?> ObtenerAsync(long id)
    {
        using var respuesta =
            await _http.GetAsync($"api/pacientes/{id}");

        if (respuesta.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content
            .ReadFromJsonAsync<PacienteDto>();
    }

    public async Task<List<ContactoPacienteDto>> ContactosAsync(
        long pacienteId)
    {
        var url =
            $"api/datos-contacto?pacienteId={pacienteId}" +
            "&incluirInactivos=true&pagina=1&tamano=100";

        return await _http.GetFromJsonAsync<
            List<ContactoPacienteDto>>(url) ?? [];
    }

    public async Task<List<CitaPacienteDto>> CitasAsync(
        long pacienteId)
    {
        var resultado = new List<CitaPacienteDto>();

        for (int pagina = 1; pagina <= 50; pagina++)
        {
            var lote = await _http.GetFromJsonAsync<
                List<CitaPacienteDto>>(
                    $"api/citas?pacienteId={pacienteId}" +
                    $"&pagina={pagina}&tamano=100") ?? [];

            resultado.AddRange(lote);

            if (lote.Count < 100)
                return resultado;
        }

        throw new InvalidOperationException(
            "Hay demasiadas citas para esta consulta. " +
            "Se necesita paginación desde el frontend.");
    }

    public async Task<List<PrestacionPacienteDto>>
        PrestacionesAsync(long pacienteId)
    {
        var resultado = new List<PrestacionPacienteDto>();

        for (int pagina = 1; pagina <= 50; pagina++)
        {
            var lote = await _http.GetFromJsonAsync<
                List<PrestacionPacienteDto>>(
                    $"api/prestaciones?pacienteId={pacienteId}" +
                    $"&pagina={pagina}&tamano=100") ?? [];

            resultado.AddRange(lote);

            if (lote.Count < 100)
                return resultado;
        }

        throw new InvalidOperationException(
            "Hay demasiadas prestaciones. " +
            "Se necesita paginación desde el frontend.");
    }

    public async Task<PacienteDto> CrearAsync(PacienteGuardarDto dto)
    {
        using var respuesta =
            await _http.PostAsJsonAsync("api/pacientes", dto);

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<PacienteDto>()
            ?? throw new InvalidOperationException(
                "La API no devolvió el paciente creado.");
    }

    public async Task ActualizarAsync(
        long id, PacienteGuardarDto dto)
    {
        using var respuesta =
            await _http.PutAsJsonAsync($"api/pacientes/{id}", dto);

        await ValidarRespuestaAsync(respuesta);
    }

    public async Task DesactivarAsync(long id, string motivo)
    {
        string url = $"api/pacientes/{id}?motivo=" +
            Uri.EscapeDataString(motivo);

        using var respuesta = await _http.DeleteAsync(url);

        await ValidarRespuestaAsync(respuesta);
    }

    public async Task ReactivarAsync(long id)
    {
        using var respuesta = await _http.PatchAsync(
            $"api/pacientes/{id}/reactivar", null);

        await ValidarRespuestaAsync(respuesta);
    }

    public async Task<ContactoPacienteDto> CrearContactoAsync(
        long pacienteId, ContactoGuardarDto dto)
    {
        using var respuesta = await _http.PostAsJsonAsync(
            "api/datos-contacto",
            new
            {
                PacienteId = pacienteId,
                dto.Tipo,
                dto.Valor,
                dto.Principal
            });

        await ValidarRespuestaAsync(respuesta);

        return await respuesta.Content
            .ReadFromJsonAsync<ContactoPacienteDto>()
            ?? throw new InvalidOperationException(
                "La API no devolvió el contacto creado.");
    }

    public async Task ActualizarContactoAsync(
        long contactoId, ContactoGuardarDto dto)
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
            $"api/datos-contacto/{contactoId}/reactivar", null);

        await ValidarRespuestaAsync(respuesta);
    }

    private static async Task ValidarRespuestaAsync(
        HttpResponseMessage respuesta)
    {
        if (respuesta.IsSuccessStatusCode)
            return;

        string mensaje = respuesta.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest =>
                "Revisa los datos del formulario.",

            System.Net.HttpStatusCode.Conflict =>
                "Existe un registro duplicado o los datos fueron modificados. Recarga antes de intentar nuevamente.",

            System.Net.HttpStatusCode.NotFound =>
                "El registro no existe.",

            System.Net.HttpStatusCode.Unauthorized =>
                "Debes iniciar sesión.",

            System.Net.HttpStatusCode.Forbidden =>
                "No tienes permiso para realizar esta acción.",

            _ => "No fue posible completar la operación."
        };

        throw new InvalidOperationException(mensaje);
    }

}
