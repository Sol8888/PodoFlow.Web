using PodoFlow.Web.Modelos.Seguridad;

namespace PodoFlow.Web.Servicios
{
    public interface ISeguridadApi
    {
        Task<List<UsuarioDto>> ListarUsuariosAsync();

        Task<UsuarioDto> CrearUsuarioAsync(UsuarioGuardarDto dto);

        Task ActualizarUsuarioAsync(long id, UsuarioGuardarDto dto);

        Task DesactivarUsuarioAsync(long id);

        Task ReactivarUsuarioAsync(long id);

        Task<List<RolDto>> ListarRolesAsync();

        Task<RolDto> CrearRolAsync(RolGuardarDto dto);

        Task ActualizarRolAsync(int id, RolGuardarDto dto);

        Task DesactivarRolAsync(int id);

        Task ReactivarRolAsync(int id);

        Task<List<PermisoDto>> ListarPermisosAsync();

        Task CrearPermisoAsync(PermisoGuardarDto dto);

        Task ActualizarPermisoAsync(int id, PermisoGuardarDto dto);

        Task DesactivarPermisoAsync(int id);

        Task ReactivarPermisoAsync(int id);

        Task<List<RolPermisoDto>> ListarRolesPermisosAsync();

        Task AsignarPermisoAsync(int rolId, int permisoId);

        Task QuitarPermisoAsync(int rolId, int permisoId);

        Task<List<UsuarioRolDto>> ListarUsuariosRolesAsync();

        Task AsignarRolAsync(long usuarioId, int rolId);

        Task QuitarRolAsync(long usuarioId, int rolId);
    }
}
