using System.Net;
using System.Text.Json;

namespace PodoFlow.Web.Servicios
{
    public static class RespuestaApi
    {
        public static async Task ValidarAsync(HttpResponseMessage respuesta)
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
                HttpStatusCode.Conflict => "El registro cambió o ya existe. Vuelve a cargar la información.",
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
