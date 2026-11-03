
using PodoFlow.Web.Modelos.Pacientes;

namespace PodoFlow.Web.Servicios;

public interface IPacientesApi
{
    // PACIENTES

    Task<List<PacienteDto>> ListarAsync(
        bool incluirInactivos = false,
        int pagina = 1,
        int tamano = 10);

    Task<PacienteDto?> ObtenerAsync(long id);

    Task<PacienteDto> CrearAsync(
        PacienteGuardarDto dto);

    Task ActualizarAsync(
        long id,
        PacienteGuardarDto dto);

    Task DesactivarAsync(
        long id,
        string motivo);

    Task ReactivarAsync(long id);

    // DATOS DE CONTACTO

    Task<List<ContactoPacienteDto>> ContactosAsync(
        long pacienteId);

    Task<ContactoPacienteDto> CrearContactoAsync(
        long pacienteId,
        ContactoGuardarDto dto);

    Task ActualizarContactoAsync(
        long contactoId,
        ContactoGuardarDto dto);

    Task DesactivarContactoAsync(long contactoId);

    Task ReactivarContactoAsync(long contactoId);

    // CITAS DEL PACIENTE

    Task<List<CitaPacienteDto>> CitasAsync(
        long pacienteId);

    // PRESTACIONES DEL PACIENTE

    Task<List<PrestacionPacienteDto>> PrestacionesAsync(
        long pacienteId);
}
