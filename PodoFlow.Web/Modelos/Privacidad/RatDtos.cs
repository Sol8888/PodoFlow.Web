using System.Text.RegularExpressions;

namespace PodoFlow.Web.Modelos.Privacidad
{
    public class TratamientoDto
    {
        public int Id { get; set; }
        public int FinalidadId { get; set; }
        public long? UsuarioId { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Modulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? CategoriasTitulares { get; set; }
        public string? BaseLegitimacion { get; set; }
        public string? PlazoConservacion { get; set; }
        public string? MedidasSeguridad { get; set; }
        public DateOnly? FechaRevision { get; set; }
        public bool Estatus { get; set; }
    }

    public class TratamientoGuardarDto
    {
        private static readonly Regex CodigoValido = new(@"^[A-Z][A-Z0-9_]{1,49}$", RegexOptions.Compiled);

        public string Codigo { get; set; } = "";
        public int FinalidadId { get; set; }
        public long? UsuarioId { get; set; }
        public string Nombre { get; set; } = "";
        public string Modulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? CategoriasTitulares { get; set; }
        public string? BaseLegitimacion { get; set; }
        public string? PlazoConservacion { get; set; }
        public string? MedidasSeguridad { get; set; }
        public DateOnly? FechaRevision { get; set; }

        public static TratamientoGuardarDto Desde(TratamientoDto tratamiento) => new()
        {
            Codigo = tratamiento.Codigo,
            FinalidadId = tratamiento.FinalidadId,
            UsuarioId = tratamiento.UsuarioId,
            Nombre = tratamiento.Nombre,
            Modulo = tratamiento.Modulo,
            Descripcion = tratamiento.Descripcion,
            CategoriasTitulares = tratamiento.CategoriasTitulares,
            BaseLegitimacion = tratamiento.BaseLegitimacion,
            PlazoConservacion = tratamiento.PlazoConservacion,
            MedidasSeguridad = tratamiento.MedidasSeguridad,
            FechaRevision = tratamiento.FechaRevision
        };

        public string? Validar(bool esNuevo)
        {
            if (esNuevo && !CodigoValido.IsMatch(Codigo.Trim().ToUpperInvariant()))
                return "El código debe empezar con una letra y llevar solo letras, números o guion bajo (2 a 50).";

            if (FinalidadId <= 0)
                return "Selecciona la finalidad del tratamiento.";

            if (string.IsNullOrWhiteSpace(Nombre))
                return "Ingresa el nombre del tratamiento.";

            if (Nombre.Trim().Length > 200)
                return "El nombre no puede pasar de 200 caracteres.";

            if (string.IsNullOrWhiteSpace(Modulo))
                return "Indica el módulo del sistema donde se hace el tratamiento.";

            if (Modulo.Trim().Length > 80)
                return "El módulo no puede pasar de 80 caracteres.";

            if (Descripcion?.Trim().Length > 2000 || MedidasSeguridad?.Trim().Length > 2000)
                return "La descripción y las medidas de seguridad no pueden pasar de 2000 caracteres.";

            if (BaseLegitimacion?.Trim().Length > 1000)
                return "La base de legitimación no puede pasar de 1000 caracteres.";

            if (CategoriasTitulares?.Trim().Length > 500 || PlazoConservacion?.Trim().Length > 500)
                return "Los titulares y el plazo de conservación no pueden pasar de 500 caracteres.";

            return null;
        }
    }

    public class CategoriaDatoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public bool EsSensible { get; set; }
        public bool Estatus { get; set; }
    }

    public class CategoriaDatoGuardarDto
    {
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public bool EsSensible { get; set; }

        public static CategoriaDatoGuardarDto Desde(CategoriaDatoDto categoria) => new()
        {
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            EsSensible = categoria.EsSensible
        };

        public string? Validar()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
                return "Ingresa el nombre de la categoría.";

            if (Nombre.Trim().Length > 150)
                return "El nombre no puede pasar de 150 caracteres.";

            if (Descripcion?.Trim().Length > 1000)
                return "La descripción no puede pasar de 1000 caracteres.";

            return null;
        }
    }

    public class EncargadoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Servicio { get; set; } = "";
        public string? RolTratamiento { get; set; }
        public string? Descripcion { get; set; }
        public string? Riesgo { get; set; }
        public string? MedidasContractuales { get; set; }
        public string? MedidasTecnicas { get; set; }
        public bool Estatus { get; set; }
    }

    public class EncargadoGuardarDto
    {
        public string Nombre { get; set; } = "";
        public string Servicio { get; set; } = "";
        public string? RolTratamiento { get; set; }
        public string? Descripcion { get; set; }
        public string? Riesgo { get; set; }
        public string? MedidasContractuales { get; set; }
        public string? MedidasTecnicas { get; set; }

        public static EncargadoGuardarDto Desde(EncargadoDto encargado) => new()
        {
            Nombre = encargado.Nombre,
            Servicio = encargado.Servicio,
            RolTratamiento = encargado.RolTratamiento,
            Descripcion = encargado.Descripcion,
            Riesgo = encargado.Riesgo,
            MedidasContractuales = encargado.MedidasContractuales,
            MedidasTecnicas = encargado.MedidasTecnicas
        };

        public string? Validar()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
                return "Ingresa el nombre del encargado.";

            if (string.IsNullOrWhiteSpace(Servicio))
                return "Indica el servicio que presta el encargado.";

            if (Nombre.Trim().Length > 200 || Servicio.Trim().Length > 200)
                return "El nombre y el servicio no pueden pasar de 200 caracteres.";

            if (RolTratamiento?.Trim().Length > 100)
                return "El rol no puede pasar de 100 caracteres.";

            if (Descripcion?.Trim().Length > 1500)
                return "La descripción no puede pasar de 1500 caracteres.";

            if (Riesgo?.Trim().Length > 1000)
                return "El riesgo no puede pasar de 1000 caracteres.";

            if (MedidasContractuales?.Trim().Length > 2000 || MedidasTecnicas?.Trim().Length > 2000)
                return "Las medidas no pueden pasar de 2000 caracteres.";

            return null;
        }
    }

    public class TratamientoCategoriaDto
    {
        public int TratamientoId { get; set; }
        public int CategoriaId { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public bool Estatus { get; set; }
    }

    public class TratamientoEncargadoDto
    {
        public int TratamientoId { get; set; }
        public int EncargadoId { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public bool Estatus { get; set; }
    }
}
