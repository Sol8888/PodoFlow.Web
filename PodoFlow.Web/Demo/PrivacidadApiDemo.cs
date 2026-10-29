using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using PodoFlow.Web.Modelos.Pacientes;
using PodoFlow.Web.Modelos.Privacidad;
using PodoFlow.Web.Servicios;

namespace PodoFlow.Web.Demo
{
    public class PrivacidadApiDemo : IPrivacidadApi
    {
        private const int PlazoDias = 15;
        private const long UsuarioDemo = 1;

        private static readonly HashSet<string> EstadosConPlazo = ["RECIBIDA", "VALIDANDO", "EN_PROCESO"];
        private static readonly HashSet<string> EstadosExportables = ["EN_PROCESO", "RESPONDIDA"];
        private static readonly HashSet<string> DerechosConExportacion = ["ACCESO", "PORTABILIDAD"];
        private static readonly HashSet<string> Formatos = ["JSON", "CSV"];

        private static readonly Dictionary<string, string[]> Transiciones = new()
        {
            ["RECIBIDA"] = ["VALIDANDO", "RECHAZADA"],
            ["VALIDANDO"] = ["EN_PROCESO", "RECHAZADA"],
            ["EN_PROCESO"] = ["RESPONDIDA", "RECHAZADA"],
            ["RESPONDIDA"] = ["CERRADA"],
            ["CERRADA"] = [],
            ["RECHAZADA"] = []
        };

        private static readonly JsonSerializerOptions OpcionesJson = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly object _bloqueo = new();
        private readonly List<PacienteDto> _pacientes;
        private readonly List<SolicitudDto> _solicitudes = new();
        private readonly List<AccionSolicitudDto> _acciones = new();
        private readonly List<EvidenciaDto> _evidencias = new();
        private readonly List<ExportacionDto> _exportaciones = new();
        private readonly Dictionary<long, byte[]> _archivos = new();

        private long _siguienteSolicitud = 1;
        private long _siguienteAccion = 1;
        private long _siguienteEvidencia = 1;
        private long _siguienteExportacion = 1;

        public PrivacidadApiDemo()
        {
            var ahora = DateTime.UtcNow;

            _pacientes =
            [
                Paciente(1, "1710034065", "María", "López Andrade", ahora.AddDays(-120)),
                Paciente(2, "0912345675", "Carlos", "Ruiz Mena", ahora.AddDays(-90)),
                Paciente(3, "0102030400", "Ana", "Torres Vega", ahora.AddDays(-60))
            ];

            var acceso = Sembrar(1, "ACCESO", "TITULAR", "María López Andrade", "1710034065", ahora.AddDays(-4), "Solicito una copia de todos mis datos.");
            Avanzar(acceso, "VALIDANDO", ahora.AddDays(-4).AddHours(2));
            Avanzar(acceso, "EN_PROCESO", ahora.AddDays(-3));
            AgregarEvidencia(acceso.Id, "IDENTIFICACION", "evidencias/cedula-maria-lopez.pdf", ahora.AddDays(-3));

            var portabilidad = Sembrar(2, "PORTABILIDAD", "REPRESENTANTE", "Lucía Ruiz Mena", "1712345678", ahora.AddDays(-14), "Pide sus datos para llevarlos a otra consulta.");
            Avanzar(portabilidad, "VALIDANDO", ahora.AddDays(-13));

            var oposicion = Sembrar(3, "OPOSICION", "TITULAR", "Ana Torres Vega", "0102030400", ahora.AddDays(-30), "No quiere recibir recordatorios por WhatsApp.");
            Avanzar(oposicion, "VALIDANDO", ahora.AddDays(-29));
            Avanzar(oposicion, "EN_PROCESO", ahora.AddDays(-28));
            Avanzar(oposicion, "RESPONDIDA", ahora.AddDays(-25), "Se desactivó el canal WhatsApp para recordatorios.", "CORREO");
            Avanzar(oposicion, "CERRADA", ahora.AddDays(-24));

            Sembrar(1, "INFORMACION", "TITULAR", "María López Andrade", "1710034065", ahora.AddDays(-1), "Consulta qué datos se usan para analítica.");

            var eliminacion = Sembrar(2, "ELIMINACION", "TITULAR", "Carlos Ruiz Mena", "0912345675", ahora.AddDays(-20), "Pide eliminar su historia clínica.");
            Avanzar(eliminacion, "RECHAZADA", ahora.AddDays(-19), "La historia clínica debe conservarse por obligación legal sanitaria.", null);
        }

        public bool EsDemostracion => true;

        public Task<List<PacienteDto>> ListarPacientesActivosAsync()
        {
            lock (_bloqueo)
                return Task.FromResult(_pacientes.Where(p => p.Estatus).ToList());
        }

        public Task<PacienteDto?> ObtenerPacienteAsync(long id)
        {
            lock (_bloqueo)
                return Task.FromResult(_pacientes.FirstOrDefault(p => p.Id == id));
        }

        public Task<List<SolicitudDto>> ListarSolicitudesAsync(int pagina, int tamano)
        {
            lock (_bloqueo)
            {
                var lista = _solicitudes
                    .OrderByDescending(s => s.FechaRecepcion)
                    .ThenByDescending(s => s.Id)
                    .Skip((pagina - 1) * tamano)
                    .Take(tamano)
                    .Select(Calcular)
                    .ToList();

                return Task.FromResult(lista);
            }
        }

        public Task<SolicitudDto?> ObtenerSolicitudAsync(long id)
        {
            lock (_bloqueo)
            {
                var solicitud = _solicitudes.FirstOrDefault(s => s.Id == id);
                return Task.FromResult(solicitud == null ? null : Calcular(solicitud));
            }
        }

        public Task<List<AccionSolicitudDto>> AccionesAsync(long solicitudId)
        {
            lock (_bloqueo)
                return Task.FromResult(_acciones.Where(a => a.SolicitudId == solicitudId).OrderBy(a => a.Id).ToList());
        }

        public Task<SolicitudDto> CrearSolicitudAsync(SolicitudGuardarDto dto)
        {
            lock (_bloqueo)
            {
                var paciente = _pacientes.FirstOrDefault(p => p.Id == dto.PacienteId && p.Estatus)
                    ?? throw new InvalidOperationException("No existe un paciente activo con ese id.");

                if (dto.TipoSolicitante == "TITULAR" && LimpiarIdentificacion(dto.IdentificacionSolicitante) != LimpiarIdentificacion(paciente.Identificacion))
                    throw new InvalidOperationException("La identificación del titular no coincide con la del paciente.");

                if (dto.TipoSolicitante == "REPRESENTANTE" && LimpiarIdentificacion(dto.IdentificacionSolicitante) == LimpiarIdentificacion(paciente.Identificacion))
                    throw new InvalidOperationException("El representante no puede tener la misma identificación que el paciente.");

                var solicitud = Sembrar(
                    paciente.Id,
                    dto.TipoDerecho,
                    dto.TipoSolicitante,
                    dto.NombreSolicitante.Trim(),
                    dto.TipoSolicitante == "TITULAR" ? paciente.Identificacion : dto.IdentificacionSolicitante.Trim(),
                    DateTime.UtcNow,
                    Limpiar(dto.Detalle));

                solicitud.MedioRespuesta = Limpiar(dto.MedioRespuesta);

                return Task.FromResult(Calcular(solicitud));
            }
        }

        public Task<SolicitudDto> CambiarEstadoSolicitudAsync(
            long id,
            string estado,
            string version,
            string? observacion,
            string? respuestaTitular,
            string? medioRespuesta)
        {
            lock (_bloqueo)
            {
                var solicitud = _solicitudes.FirstOrDefault(s => s.Id == id)
                    ?? throw new InvalidOperationException("No se encontró la solicitud.");

                if (solicitud.Version != version)
                    throw new InvalidOperationException("La solicitud fue modificada por otro usuario. Recarga la página.");

                if (!Transiciones[solicitud.Estado].Contains(estado))
                    throw new InvalidOperationException($"No se puede pasar de {solicitud.Estado} a {estado}.");

                if (estado is "RESPONDIDA" or "RECHAZADA" && string.IsNullOrWhiteSpace(respuestaTitular))
                    throw new InvalidOperationException(estado == "RECHAZADA"
                        ? "El rechazo exige el motivo en la respuesta."
                        : "Para marcarla RESPONDIDA debe registrar la respuesta.");

                if (estado == "RESPONDIDA" && string.IsNullOrWhiteSpace(medioRespuesta))
                    throw new InvalidOperationException("Para marcarla RESPONDIDA debe indicar el medio de respuesta.");

                if (estado == "RESPONDIDA"
                    && DerechosConExportacion.Contains(solicitud.TipoDerecho)
                    && !_exportaciones.Any(x => x.SolicitudId == id && x.Estado == "ENTREGADA"))
                    throw new InvalidOperationException(
                        $"Una solicitud de {solicitud.TipoDerecho} se responde entregando los datos al titular. Genere la exportación y márquela como ENTREGADA antes de responder.");

                Avanzar(solicitud, estado, DateTime.UtcNow, Limpiar(respuestaTitular), Limpiar(medioRespuesta), Limpiar(observacion));

                return Task.FromResult(Calcular(solicitud));
            }
        }

        public Task<List<EvidenciaDto>> EvidenciasAsync(long solicitudId)
        {
            lock (_bloqueo)
                return Task.FromResult(_evidencias.Where(e => e.SolicitudId == solicitudId).OrderBy(e => e.Id).ToList());
        }

        public Task<EvidenciaDto> CrearEvidenciaAsync(long solicitudId, EvidenciaGuardarDto dto)
        {
            lock (_bloqueo)
            {
                var solicitud = _solicitudes.FirstOrDefault(s => s.Id == solicitudId)
                    ?? throw new InvalidOperationException("No se encontró la solicitud.");

                if (solicitud.EsFinal)
                    throw new InvalidOperationException($"La solicitud está {solicitud.Estado} y no admite evidencias.");

                var evidencia = AgregarEvidencia(solicitudId, dto.Tipo.Trim().ToUpperInvariant(), dto.RutaArchivo.Trim(), DateTime.UtcNow);
                evidencia.Huella = Limpiar(dto.Huella);

                return Task.FromResult(evidencia);
            }
        }

        public Task<List<ExportacionDto>> ExportacionesAsync(long solicitudId)
        {
            lock (_bloqueo)
                return Task.FromResult(_exportaciones.Where(x => x.SolicitudId == solicitudId).OrderBy(x => x.Id).ToList());
        }

        public Task<ExportacionDto> GenerarExportacionAsync(
            long solicitudId,
            string formato,
            List<string>? secciones,
            string? observacion)
        {
            lock (_bloqueo)
            {
                formato = formato.Trim().ToUpperInvariant();

                if (!Formatos.Contains(formato))
                    throw new InvalidOperationException("El formato debe ser JSON o CSV.");

                var solicitud = _solicitudes.FirstOrDefault(s => s.Id == solicitudId)
                    ?? throw new InvalidOperationException("No se encontró la solicitud.");

                if (!DerechosConExportacion.Contains(solicitud.TipoDerecho))
                    throw new InvalidOperationException($"Una solicitud de {solicitud.TipoDerecho} no genera exportación. Solo ACCESO o PORTABILIDAD.");

                if (!EstadosExportables.Contains(solicitud.Estado))
                    throw new InvalidOperationException($"La solicitud está {solicitud.Estado}. Solo se exporta cuando está EN_PROCESO o RESPONDIDA.");

                if (!_evidencias.Any(e => e.SolicitudId == solicitudId))
                    throw new InvalidOperationException("La solicitud no tiene evidencia registrada de la validación de identidad.");

                var ahora = SinFraccion(DateTime.UtcNow);
                var incluidas = secciones is { Count: > 0 } ? secciones : ["PERSONALES", "CITAS", "CONSENTIMIENTOS", "COMUNICACIONES", "SOLICITUDES", "CLINICA"];
                var contenido = ArmarArchivo(solicitud, formato, incluidas, ahora);

                var exportacion = new ExportacionDto
                {
                    Id = _siguienteExportacion++,
                    SolicitudId = solicitudId,
                    UsuarioId = UsuarioDemo,
                    Formato = formato,
                    RutaArchivo = $"solicitud-{solicitudId}_{ahora:yyyyMMddHHmmss}_demo.{formato.ToLowerInvariant()}",
                    Huella = Convert.ToHexString(SHA256.HashData(contenido)).ToLowerInvariant(),
                    FechaGeneracion = ahora,
                    Estado = "GENERADA"
                };

                _exportaciones.Add(exportacion);
                _archivos[exportacion.Id] = contenido;
                Registrar(solicitudId, $"Exportación generada en {formato} con las secciones {string.Join(", ", incluidas)}", null, null, Limpiar(observacion), ahora);

                return Task.FromResult(exportacion);
            }
        }

        public Task<ExportacionDto> CambiarEstadoExportacionAsync(long id, string estado, string? observacion)
        {
            lock (_bloqueo)
            {
                var exportacion = _exportaciones.FirstOrDefault(x => x.Id == id)
                    ?? throw new InvalidOperationException("No se encontró la exportación.");

                if (exportacion.Estado != "GENERADA" || estado is not ("ENTREGADA" or "ANULADA"))
                    throw new InvalidOperationException($"No se puede pasar de {exportacion.Estado} a {estado}.");

                var ahora = DateTime.UtcNow;
                exportacion.Estado = estado;

                if (estado == "ENTREGADA")
                    exportacion.FechaEntrega = ahora;

                Registrar(exportacion.SolicitudId, $"Exportación {id} {(estado == "ENTREGADA" ? "entregada" : "anulada")}", null, null, Limpiar(observacion), ahora);

                return Task.FromResult(exportacion);
            }
        }

        public Task<ArchivoExportacionDto> DescargarExportacionAsync(long id)
        {
            lock (_bloqueo)
            {
                var exportacion = _exportaciones.FirstOrDefault(x => x.Id == id)
                    ?? throw new InvalidOperationException("No se encontró la exportación.");

                if (exportacion.Estado == "ANULADA")
                    throw new InvalidOperationException("La exportación está ANULADA y no se puede entregar.");

                var contenido = _archivos[id];
                Registrar(exportacion.SolicitudId, $"Exportación {id} descargada para entrega", null, null, null, DateTime.UtcNow);

                return Task.FromResult(new ArchivoExportacionDto
                {
                    Contenido = contenido,
                    NombreArchivo = exportacion.RutaArchivo,
                    TipoContenido = exportacion.Formato == "JSON" ? "application/json" : "text/csv"
                });
            }
        }

        private SolicitudDto Sembrar(
            long pacienteId,
            string derecho,
            string tipoSolicitante,
            string nombre,
            string identificacion,
            DateTime fecha,
            string? detalle)
        {
            var solicitud = new SolicitudDto
            {
                Id = _siguienteSolicitud++,
                PacienteId = pacienteId,
                UsuarioId = UsuarioDemo,
                TipoDerecho = derecho,
                TipoSolicitante = tipoSolicitante,
                NombreSolicitante = nombre,
                IdentificacionSolicitante = identificacion,
                FechaRecepcion = SinFraccion(fecha),
                Estado = "RECIBIDA",
                Detalle = detalle,
                Version = Guid.NewGuid().ToString("N")
            };

            _solicitudes.Add(solicitud);
            Registrar(solicitud.Id, "Solicitud registrada", null, "RECIBIDA", null, fecha);

            return solicitud;
        }

        private void Avanzar(
            SolicitudDto solicitud,
            string estado,
            DateTime fecha,
            string? respuesta = null,
            string? medio = null,
            string? observacion = null)
        {
            var anterior = solicitud.Estado;

            solicitud.Estado = estado;
            solicitud.Version = Guid.NewGuid().ToString("N");

            if (respuesta != null)
                solicitud.Respuesta = respuesta;

            if (medio != null)
                solicitud.MedioRespuesta = medio;

            if (estado is "CERRADA" or "RECHAZADA")
                solicitud.FechaCierre = SinFraccion(fecha);

            Registrar(solicitud.Id, $"Cambio de estado de {anterior} a {estado}", anterior, estado, observacion, fecha);
        }

        private EvidenciaDto AgregarEvidencia(long solicitudId, string tipo, string ruta, DateTime fecha)
        {
            var evidencia = new EvidenciaDto
            {
                Id = _siguienteEvidencia++,
                SolicitudId = solicitudId,
                UsuarioId = UsuarioDemo,
                Tipo = tipo,
                RutaArchivo = ruta,
                FechaCarga = SinFraccion(fecha)
            };

            _evidencias.Add(evidencia);
            Registrar(solicitudId, $"Evidencia cargada: {tipo}", null, null, null, fecha);

            return evidencia;
        }

        private void Registrar(long solicitudId, string accion, string? anterior, string? nuevo, string? observacion, DateTime fecha)
        {
            _acciones.Add(new AccionSolicitudDto
            {
                Id = _siguienteAccion++,
                SolicitudId = solicitudId,
                UsuarioId = UsuarioDemo,
                FechaHora = SinFraccion(fecha),
                EstadoAnterior = anterior,
                EstadoNuevo = nuevo,
                Accion = accion,
                Observacion = observacion
            });
        }

        private byte[] ArmarArchivo(SolicitudDto solicitud, string formato, List<string> secciones, DateTime fecha)
        {
            var paciente = _pacientes.First(p => p.Id == solicitud.PacienteId);

            var datos = new Dictionary<string, object?>
            {
                ["metadatos"] = new Dictionary<string, object?>
                {
                    ["solicitudId"] = solicitud.Id,
                    ["derecho"] = solicitud.TipoDerecho,
                    ["formato"] = formato,
                    ["fechaGeneracion"] = fecha,
                    ["secciones"] = secciones,
                    ["demostracion"] = true
                },
                ["datos"] = new Dictionary<string, object?>
                {
                    ["paciente"] = new[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["id"] = paciente.Id,
                            ["tipoIdentificacion"] = paciente.TipoIdentificacion,
                            ["identificacion"] = paciente.Identificacion,
                            ["nombres"] = paciente.Nombres,
                            ["apellidos"] = paciente.Apellidos
                        }
                    }
                }
            };

            if (formato == "JSON")
                return JsonSerializer.SerializeToUtf8Bytes(datos, OpcionesJson);

            var csv = new StringBuilder("seccion,registro,campo,valor\r\n");
            csv.Append($"metadatos,1,solicitudId,{solicitud.Id}\r\n");
            csv.Append($"metadatos,1,derecho,{solicitud.TipoDerecho}\r\n");
            csv.Append($"metadatos,1,secciones,{string.Join(" | ", secciones)}\r\n");
            csv.Append("metadatos,1,demostracion,true\r\n");
            csv.Append($"paciente,1,identificacion,{paciente.Identificacion}\r\n");
            csv.Append($"paciente,1,nombres,{paciente.Nombres}\r\n");
            csv.Append($"paciente,1,apellidos,{paciente.Apellidos}\r\n");

            return [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(csv.ToString())];
        }

        private static SolicitudDto Calcular(SolicitudDto solicitud)
        {
            var ahora = DateTime.UtcNow;
            var limite = solicitud.FechaRecepcion.AddDays(PlazoDias);
            var abierta = EstadosConPlazo.Contains(solicitud.Estado);

            solicitud.FechaLimite = limite;
            solicitud.DiasRestantes = abierta ? (int)Math.Ceiling((limite - ahora).TotalDays) : null;
            solicitud.Vencida = abierta && ahora > limite;

            return solicitud;
        }

        private static PacienteDto Paciente(long id, string cedula, string nombres, string apellidos, DateTime registro)
        {
            return new PacienteDto
            {
                Id = id,
                TipoIdentificacion = "CEDULA",
                Identificacion = cedula,
                Nombres = nombres,
                Apellidos = apellidos,
                Estatus = true,
                FechaCreacion = registro
            };
        }

        private static DateTime SinFraccion(DateTime fecha)
        {
            var utc = DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
            return new DateTime(utc.Ticks - utc.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);
        }

        private static string LimpiarIdentificacion(string valor)
        {
            return new string(valor.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        }

        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
