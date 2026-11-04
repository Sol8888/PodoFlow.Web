
using PodoFlow.Web.Modelos.Pacientes;
using PodoFlow.Web.Modelos.Atenciones;

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

    // ============================================
    // HISTORIA CLÍNICA Y ANTECEDENTES
    // ============================================

    Task<HistoriaPacienteDto?> ObtenerHistoriaPacienteAsync(
        long pacienteId);

    Task<List<AntecedenteDto>> ListarAntecedentesAsync(
        long historiaId,
        bool incluirInactivos = false,
        int pagina = 1,
        int tamano = 10);

    Task<AntecedenteDto> CrearAntecedenteAsync(
        long historiaId,
        AntecedenteGuardarDto dto);

    Task ActualizarAntecedenteAsync(
        long antecedenteId,
        AntecedenteGuardarDto dto);

    Task DesactivarAntecedenteAsync(long antecedenteId);

    Task ReactivarAntecedenteAsync(long antecedenteId);

    Task<HistoriaPacienteDto> CrearHistoriaPacienteAsync(long pacienteId);

    Task<List<AtencionDto>> ListarAtencionesAsync(
        long historiaId,
        int pagina = 1,
        int tamano = 10);


    Task<AtencionDto> CrearAtencionAsync(
        long historiaId,
        AtencionCrearDto dto);

}
