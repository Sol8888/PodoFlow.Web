using Microsoft.AspNetCore.Components.Forms;
using PodoFlow.Web.Modelos.Pacientes;
using PodoFlow.Web.Modelos.Privacidad;

namespace PodoFlow.Web.Servicios
{
    public interface IPrivacidadApi
    {
        Task<List<PacienteDto>> ListarPacientesActivosAsync();

        Task<PacienteDto?> ObtenerPacienteAsync(long id);

        Task<List<SolicitudDto>> ListarSolicitudesAsync(int pagina, int tamano);

        Task<SolicitudDto?> ObtenerSolicitudAsync(long id);

        Task<List<AccionSolicitudDto>> AccionesAsync(long solicitudId);

        Task<SolicitudDto> CrearSolicitudAsync(SolicitudGuardarDto dto);

        Task<SolicitudDto> CambiarEstadoSolicitudAsync(
            long id,
            string estado,
            string version,
            string? observacion,
            string? respuestaTitular,
            string? medioRespuesta);

        Task<List<EvidenciaDto>> EvidenciasAsync(long solicitudId);

        Task<EvidenciaDto> CrearEvidenciaAsync(long solicitudId, EvidenciaGuardarDto dto);

        Task<List<ExportacionDto>> ExportacionesAsync(long solicitudId);

        Task<ExportacionDto> GenerarExportacionAsync(
            long solicitudId,
            string formato,
            List<string>? secciones,
            string? observacion);

        Task<ExportacionDto> CambiarEstadoExportacionAsync(long id, string estado, string? observacion);

        Task<ArchivoExportacionDto> DescargarExportacionAsync(long id);

        Task<List<FinalidadDto>> ListarFinalidadesAsync();

        Task CrearFinalidadAsync(FinalidadGuardarDto dto);

        Task ActualizarFinalidadAsync(int id, FinalidadGuardarDto dto);

        Task CambiarEstatusFinalidadAsync(int id, bool activar);

        Task<List<AvisoDto>> ListarAvisosAsync();

        Task CrearAvisoAsync(AvisoGuardarDto dto);

        Task ActualizarAvisoAsync(int id, AvisoGuardarDto dto);

        Task CambiarEstatusAvisoAsync(int id, bool activar);

        Task<List<ConsentimientoDto>> ConsentimientosAsync(long pacienteId);

        Task<List<EventoConsentimientoDto>> EventosConsentimientoAsync(long consentimientoId);

        Task CrearConsentimientoAsync(ConsentimientoCrearDto dto);

        Task CambiarEstadoConsentimientoAsync(long id, ConsentimientoCambiarEstadoDto dto);

        Task<List<TratamientoDto>> ListarTratamientosAsync();

        Task CrearTratamientoAsync(TratamientoGuardarDto dto);

        Task ActualizarTratamientoAsync(int id, TratamientoGuardarDto dto);

        Task CambiarEstatusTratamientoAsync(int id, bool activar);

        Task<List<CategoriaDatoDto>> ListarCategoriasDatoAsync();

        Task CrearCategoriaDatoAsync(CategoriaDatoGuardarDto dto);

        Task ActualizarCategoriaDatoAsync(int id, CategoriaDatoGuardarDto dto);

        Task CambiarEstatusCategoriaDatoAsync(int id, bool activar);

        Task<List<EncargadoDto>> ListarEncargadosAsync();

        Task CrearEncargadoAsync(EncargadoGuardarDto dto);

        Task ActualizarEncargadoAsync(int id, EncargadoGuardarDto dto);

        Task CambiarEstatusEncargadoAsync(int id, bool activar);

        Task<List<TratamientoCategoriaDto>> CategoriasDeTratamientoAsync(int tratamientoId);

        Task AsignarCategoriaAsync(int tratamientoId, int categoriaId);

        Task QuitarCategoriaAsync(int tratamientoId, int categoriaId);

        Task<List<TratamientoEncargadoDto>> EncargadosDeTratamientoAsync(int tratamientoId);

        Task AsignarEncargadoAsync(int tratamientoId, int encargadoId);

        Task QuitarEncargadoAsync(int tratamientoId, int encargadoId);

        Task<List<RiesgoDto>> ListarRiesgosAsync();

        Task CrearRiesgoAsync(RiesgoGuardarDto dto);

        Task ActualizarRiesgoAsync(long id, RiesgoGuardarDto dto);

        Task CambiarEstadoRiesgoAsync(long id, string estado, string? medidasMitigacion, byte? riesgoResidual);

        Task<List<EvaluacionImpactoDto>> ListarEvaluacionesAsync();

        Task<EvaluacionImpactoDto> CrearEvaluacionAsync(EvaluacionImpactoGuardarDto dto);

        Task ActualizarEvaluacionAsync(long id, EvaluacionImpactoGuardarDto dto);

        Task CambiarEstadoEvaluacionAsync(long id, string estado);

        Task<List<EvaluacionRiesgoDto>> ListarRiesgosEvaluadosAsync();

        Task AgregarRiesgoEvaluacionAsync(long evaluacionId, long riesgoId, string? observacion);

        Task<int> CopiarRiesgosEvaluacionAsync(long origenId, long destinoId, List<long> excluidos);

        Task<List<DocumentoDto>> ListarDocumentosAsync();

        Task SubirDocumentoAsync(DocumentoGuardarDto dto, IBrowserFile archivo);

        Task<ArchivoExportacionDto> DescargarDocumentoAsync(long id);

        Task ActualizarDocumentoAsync(long id, DocumentoGuardarDto dto);

        Task CambiarEstatusDocumentoAsync(long id, bool activar);

        Task<List<IncidenteDto>> ListarIncidentesAsync();

        Task<IncidenteDto?> ObtenerIncidenteAsync(long id);

        Task<IncidenteDto> CrearIncidenteAsync(IncidenteGuardarDto dto);

        Task ActualizarIncidenteAsync(long id, IncidenteGuardarDto dto, string version);

        Task<IncidenteDto> CambiarEstadoIncidenteAsync(long id, string estado, string? accion, string? observacion, string version);

        Task<List<AccionIncidenteDto>> AccionesIncidenteAsync(long incidenteId);

        Task RegistrarAccionIncidenteAsync(long incidenteId, AccionIncidenteGuardarDto dto);
    }
}
