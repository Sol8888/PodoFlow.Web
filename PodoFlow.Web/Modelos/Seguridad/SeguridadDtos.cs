namespace PodoFlow.Web.Modelos.Seguridad
{
    public class UsuarioDto
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public bool Estatus { get; set; }
        public string Version { get; set; } = "";
    }

    public class UsuarioGuardarDto
    {
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
        public string? Version { get; set; }
    }

    public class RolDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Estatus { get; set; }
    }

    public class RolGuardarDto
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
    }

    public class PermisoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Modulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public bool Estatus { get; set; }
    }

    public class PermisoGuardarDto
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Modulo { get; set; } = "";
        public string? Descripcion { get; set; }
    }

    public class RolPermisoDto
    {
        public int RolId { get; set; }
        public int PermisoId { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public bool Estatus { get; set; }
    }

    public class UsuarioRolDto
    {
        public long UsuarioId { get; set; }
        public int RolId { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public bool Estatus { get; set; }
    }
}
