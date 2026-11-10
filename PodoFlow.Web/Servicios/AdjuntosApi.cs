using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using PodoFlow.Web.Modelos.Atenciones;

namespace PodoFlow.Web.Servicios
{
    public class AdjuntosApi : IAdjuntosApi
    {
        public const long MaximoBytes = 10 * 1024 * 1024;

        private const int TamanoLote = 100;
        private const int MaximoLotes = 20;

        private readonly HttpClient _http;

        public AdjuntosApi(HttpClient http)
        {
            _http = http;
        }

        public static string? TipoPorExtension(string nombreArchivo) =>
            Path.GetExtension(nombreArchivo).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => null
            };

        public async Task<List<AdjuntoDto>> ListarAsync(long atencionId)
        {
            var resultado = new List<AdjuntoDto>();

            for (var pagina = 1; pagina <= MaximoLotes; pagina++)
            {
                using var respuesta = await _http.GetAsync(
                    $"api/adjuntos?atencionId={atencionId}&incluirInactivos=true&pagina={pagina}&tamano={TamanoLote}");
                await RespuestaApi.ValidarAsync(respuesta);

                var lote = await respuesta.Content.ReadFromJsonAsync<List<AdjuntoDto>>() ?? [];
                resultado.AddRange(lote);

                if (lote.Count < TamanoLote)
                    break;
            }

            return resultado;
        }

        public async Task<AdjuntoDto> SubirAsync(long atencionId, long usuarioId, IBrowserFile archivo, string? nombre)
        {
            var tipo = TipoPorExtension(archivo.Name)
                ?? throw new InvalidOperationException("Solo se permiten archivos JPG, PNG y PDF.");

            if (archivo.Size <= 0 || archivo.Size > MaximoBytes)
                throw new InvalidOperationException("El archivo debe tener entre 1 byte y 10 MB.");

            await using var flujo = archivo.OpenReadStream(MaximoBytes);

            using var contenidoArchivo = new StreamContent(flujo);
            contenidoArchivo.Headers.ContentType = new MediaTypeHeaderValue(tipo);

            using var formulario = new MultipartFormDataContent
            {
                { new StringContent(usuarioId.ToString()), "UsuarioId" },
                { contenidoArchivo, "Archivo", archivo.Name }
            };

            if (!string.IsNullOrWhiteSpace(nombre))
                formulario.Add(new StringContent(nombre.Trim()), "Nombre");

            using var respuesta = await _http.PostAsync($"api/adjuntos/archivos/atencion/{atencionId}", formulario);
            await RespuestaApi.ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<AdjuntoDto>()
                ?? throw new InvalidOperationException("La API no devolvió el adjunto registrado.");
        }

        public async Task<AdjuntoArchivo> DescargarAsync(long adjuntoId)
        {
            using var respuesta = await _http.GetAsync($"api/adjuntos/{adjuntoId}/archivo");
            await RespuestaApi.ValidarAsync(respuesta);

            var contenido = await respuesta.Content.ReadAsByteArrayAsync();
            var tipo = respuesta.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var nombre = respuesta.Content.Headers.ContentDisposition?.FileNameStar
                ?? respuesta.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? $"adjunto-{adjuntoId}";

            return new AdjuntoArchivo(contenido, tipo, nombre);
        }

        public async Task DesactivarAsync(long adjuntoId)
        {
            using var respuesta = await _http.DeleteAsync($"api/adjuntos/{adjuntoId}");
            await RespuestaApi.ValidarAsync(respuesta);
        }

        public async Task ReactivarAsync(long adjuntoId)
        {
            using var respuesta = await _http.PatchAsync($"api/adjuntos/{adjuntoId}/reactivar", null);
            await RespuestaApi.ValidarAsync(respuesta);
        }
    }
}
