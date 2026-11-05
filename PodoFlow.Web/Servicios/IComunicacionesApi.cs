using PodoFlow.Web.Modelos.Comunicaciones;

namespace PodoFlow.Web.Servicios
{
    public interface IComunicacionesApi
    {
        Task<List<OpcionComunicacionDto>> OpcionesAsync(long pacienteId);

        Task CrearOpcionAsync(long pacienteId, int finalidadId, string canal, bool permitido);

        Task CambiarPermisoAsync(long opcionId, bool permitido);

        Task<List<PlantillaDto>> PlantillasAsync(bool incluirInactivas);

        Task CrearPlantillaAsync(PlantillaGuardarDto dto);

        Task ActualizarPlantillaAsync(int id, PlantillaGuardarDto dto);

        Task DesactivarPlantillaAsync(int id);

        Task ReactivarPlantillaAsync(int id);
    }
}
