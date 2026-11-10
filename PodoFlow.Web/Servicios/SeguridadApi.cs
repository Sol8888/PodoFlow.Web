using System.Net.Http.Json;
using PodoFlow.Web.Modelos.Seguridad;

namespace PodoFlow.Web.Servicios
{
    public class SeguridadApi : ISeguridadApi
    {
        private const int TamanoLote = 100;
        private const int MaximoLotes = 20;

        private readonly HttpClient _http;

        public SeguridadApi(HttpClient http)
        {
            _http = http;
        }

        public Task<List<UsuarioDto>> ListarUsuariosAsync() =>
            LeerTodoAsync<UsuarioDto>("api/usuarios?incluirInactivos=true&");

        public Task<UsuarioDto> CrearUsuarioAsync(UsuarioGuardarDto dto) =>
            CrearAsync<UsuarioDto>("api/usuarios", dto);

        public Task ActualizarUsuarioAsync(long id, UsuarioGuardarDto dto) =>
            EnviarAsync(HttpMethod.Put, $"api/usuarios/{id}", dto);

        public Task DesactivarUsuarioAsync(long id) =>
            EnviarAsync(HttpMethod.Delete, $"api/usuarios/{id}", null);

        public Task ReactivarUsuarioAsync(long id) =>
            EnviarAsync(HttpMethod.Patch, $"api/usuarios/{id}/reactivar", null);

        public Task<List<RolDto>> ListarRolesAsync() =>
            LeerTodoAsync<RolDto>("api/roles?incluirInactivos=true&");

        public Task<RolDto> CrearRolAsync(RolGuardarDto dto) =>
            CrearAsync<RolDto>("api/roles", dto);

        public Task ActualizarRolAsync(int id, RolGuardarDto dto) =>
            EnviarAsync(HttpMethod.Put, $"api/roles/{id}", dto);

        public Task DesactivarRolAsync(int id) =>
            EnviarAsync(HttpMethod.Delete, $"api/roles/{id}", null);

        public Task ReactivarRolAsync(int id) =>
            EnviarAsync(HttpMethod.Patch, $"api/roles/{id}/reactivar", null);

        public Task<List<PermisoDto>> ListarPermisosAsync() =>
            LeerTodoAsync<PermisoDto>("api/permisos?incluirInactivos=true&");

        public Task CrearPermisoAsync(PermisoGuardarDto dto) =>
            EnviarAsync(HttpMethod.Post, "api/permisos", dto);

        public Task ActualizarPermisoAsync(int id, PermisoGuardarDto dto) =>
            EnviarAsync(HttpMethod.Put, $"api/permisos/{id}", dto);

        public Task DesactivarPermisoAsync(int id) =>
            EnviarAsync(HttpMethod.Delete, $"api/permisos/{id}", null);

        public Task ReactivarPermisoAsync(int id) =>
            EnviarAsync(HttpMethod.Patch, $"api/permisos/{id}/reactivar", null);

        public Task<List<RolPermisoDto>> ListarRolesPermisosAsync() =>
            LeerTodoAsync<RolPermisoDto>("api/roles-permisos?incluirInactivos=true&");

        public Task AsignarPermisoAsync(int rolId, int permisoId) =>
            EnviarAsync(HttpMethod.Post, "api/roles-permisos", new { RolId = rolId, PermisoId = permisoId });

        public Task QuitarPermisoAsync(int rolId, int permisoId) =>
            EnviarAsync(HttpMethod.Delete, $"api/roles-permisos/{rolId}/{permisoId}", null);

        public Task<List<UsuarioRolDto>> ListarUsuariosRolesAsync() =>
            LeerTodoAsync<UsuarioRolDto>("api/usuarios-roles?incluirInactivos=true&");

        public Task AsignarRolAsync(long usuarioId, int rolId) =>
            EnviarAsync(HttpMethod.Post, "api/usuarios-roles", new { UsuarioId = usuarioId, RolId = rolId });

        public Task QuitarRolAsync(long usuarioId, int rolId) =>
            EnviarAsync(HttpMethod.Delete, $"api/usuarios-roles/{usuarioId}/{rolId}", null);

        private async Task<List<T>> LeerTodoAsync<T>(string rutaConConsulta)
        {
            var resultado = new List<T>();

            for (var pagina = 1; pagina <= MaximoLotes; pagina++)
            {
                using var respuesta = await _http.GetAsync($"{rutaConConsulta}pagina={pagina}&tamano={TamanoLote}");
                await RespuestaApi.ValidarAsync(respuesta);

                var lote = await respuesta.Content.ReadFromJsonAsync<List<T>>() ?? [];
                resultado.AddRange(lote);

                if (lote.Count < TamanoLote)
                    break;
            }

            return resultado;
        }

        private async Task<T> CrearAsync<T>(string ruta, object datos)
        {
            using var respuesta = await _http.PostAsJsonAsync(ruta, datos);
            await RespuestaApi.ValidarAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<T>()
                ?? throw new InvalidOperationException("La API no devolvió el registro creado.");
        }

        private async Task EnviarAsync(HttpMethod metodo, string ruta, object? datos)
        {
            using var solicitud = new HttpRequestMessage(metodo, ruta);

            if (datos != null)
                solicitud.Content = JsonContent.Create(datos, datos.GetType());

            using var respuesta = await _http.SendAsync(solicitud);
            await RespuestaApi.ValidarAsync(respuesta);
        }
    }
}
