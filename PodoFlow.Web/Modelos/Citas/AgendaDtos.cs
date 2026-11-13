namespace PodoFlow.Web.Modelos.Citas
{
    public class CitaDto
    {
        public long Id { get; set; }
        public long PacienteId { get; set; }
        public int ServicioId { get; set; }
        public long? UsuarioId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; } = "";
        public string Canal { get; set; } = "";
        public string? Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string Version { get; set; } = "";

        public DateTime InicioLocal => AgendaFechas.ALocal(FechaInicio);
        public DateTime FinLocal => AgendaFechas.ALocal(FechaFin);
    }

    public class CitaGuardarDto
    {
        public long PacienteId { get; set; }
        public int ServicioId { get; set; }
        public long? UsuarioId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Canal { get; set; } = "";
        public string? Observaciones { get; set; }
        public string? Version { get; set; }
        public string? MotivoCambio { get; set; }
    }

    public class CambioCitaDto
    {
        public long Id { get; set; }
        public string? EstadoAnterior { get; set; }
        public string? EstadoNuevo { get; set; }
        public DateTime? FechaAnterior { get; set; }
        public DateTime? FechaNueva { get; set; }
        public string? Motivo { get; set; }
        public DateTime FechaCambio { get; set; }
    }

    public class ServicioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public short DuracionMinutos { get; set; }
        public bool Estatus { get; set; }
    }

    public class UsuarioAgendaDto
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = "";
        public bool Estatus { get; set; }
    }

    public class HorarioDto
    {
        public int Id { get; set; }
        public long? UsuarioId { get; set; }
        public byte DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool Estatus { get; set; }
    }

    public class HorarioGuardarDto
    {
        public long? UsuarioId { get; set; }
        public byte DiaSemana { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public TimeSpan? HoraFin { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }

    public class BloqueoDto
    {
        public long Id { get; set; }
        public long? UsuarioId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Motivo { get; set; } = "";
        public bool Estatus { get; set; }

        public DateTime InicioLocal => AgendaFechas.ALocal(FechaInicio);
        public DateTime FinLocal => AgendaFechas.ALocal(FechaFin);
    }

    public class BloqueoGuardarDto
    {
        public long? UsuarioId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Motivo { get; set; } = "";
    }
    public class CatalogosAgenda
    {
        public List<ServicioDto> Servicios { get; set; } = new();
        public List<UsuarioAgendaDto> Usuarios { get; set; } = new();
        public List<Pacientes.PacienteDto> Pacientes { get; set; } = new();
        public List<HorarioDto> Horarios { get; set; } = new();
        public List<BloqueoDto> Bloqueos { get; set; } = new();

        public string NombrePaciente(long id) =>
            Pacientes.FirstOrDefault(x => x.Id == id)?.NombreCompleto ?? $"Paciente {id}";

        public string NombreServicio(int id) =>
            Servicios.FirstOrDefault(x => x.Id == id)?.Nombre ?? $"Servicio {id}";

        public string NombreUsuario(long? id) =>
            id == null
                ? "Sin asignar"
                : Usuarios.FirstOrDefault(x => x.Id == id)?.Nombre ?? $"Usuario {id}";
    }

    public sealed record SugerenciaCita(DateTime Fecha, TimeSpan Hora, long? UsuarioId);
    public static class AgendaFechas
    {
        private static readonly TimeZoneInfo Zona = ObtenerZona();

        public static DateTime Ahora => ALocal(DateTime.UtcNow);

        public static DateTime ALocal(DateTime fecha)
        {
            var utc = fecha.Kind == DateTimeKind.Local
                ? fecha.ToUniversalTime()
                : DateTime.SpecifyKind(fecha, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, Zona);
        }

        public static DateTime AUtc(DateTime local)
        {
            var sinSegundos = new DateTime(
                local.Year, local.Month, local.Day,
                local.Hour, local.Minute, 0,
                DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(sinSegundos, Zona);
        }
        public static int DiaSemana(DateTime fecha) => ((int)fecha.DayOfWeek + 6) % 7 + 1;

        public static DateTime LunesDe(DateTime fecha) => fecha.Date.AddDays(1 - DiaSemana(fecha));

        private static TimeZoneInfo ObtenerZona()
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById("America/Guayaquil", out var zona))
                return zona;

            if (TimeZoneInfo.TryFindSystemTimeZoneById("SA Pacific Standard Time", out zona))
                return zona;

            return TimeZoneInfo.CreateCustomTimeZone("Ecuador", TimeSpan.FromHours(-5), "Ecuador", "Ecuador");
        }
    }

    public static class AgendaReglas
    {
        public const int MinutosHueco = 30;

        public static readonly string[] Estados =
        [
            "SOLICITADA", "CONFIRMADA", "REPROGRAMADA", "ATENDIDA", "NO_ASISTE", "CANCELADA"
        ];

        public static readonly (string Codigo, string Nombre)[] Canales =
        [
            ("PRESENCIAL", "Presencial"),
            ("TELEFONO", "Teléfono"),
            ("WHATSAPP", "WhatsApp"),
            ("SISTEMA", "Sistema")
        ];

        public static readonly string[] Dias =
        [
            "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"
        ];

        public static string TextoEstado(string estado) => estado switch
        {
            "SOLICITADA" => "Solicitada",
            "CONFIRMADA" => "Confirmada",
            "REPROGRAMADA" => "Reprogramada",
            "ATENDIDA" => "Atendida",
            "NO_ASISTE" => "No asistió",
            "CANCELADA" => "Cancelada",
            _ => estado
        };

        public static string ClaseEstado(string estado) => estado switch
        {
            "SOLICITADA" => "solicitada",
            "CONFIRMADA" => "confirmada",
            "REPROGRAMADA" => "reprogramada",
            "ATENDIDA" => "atendida",
            "CANCELADA" => "cancelada",
            _ => "no-asiste"
        };

        public static string TextoCanal(string canal)
        {
            foreach (var opcion in Canales)
            {
                if (opcion.Codigo == canal)
                    return opcion.Nombre;
            }

            return canal;
        }

        public static bool EsFinal(string estado) => estado is "ATENDIDA" or "CANCELADA" or "NO_ASISTE";

        public static bool OcupaHorario(string estado) => estado is not ("CANCELADA" or "NO_ASISTE");

        public static string[] Siguientes(string estado) => estado switch
        {
            "SOLICITADA" => ["CONFIRMADA", "NO_ASISTE", "CANCELADA"],
            "CONFIRMADA" => ["ATENDIDA", "NO_ASISTE", "CANCELADA"],
            "REPROGRAMADA" => ["CONFIRMADA", "NO_ASISTE", "CANCELADA"],
            _ => []
        };

        public const int MinutosAntesDeAtender = 10;

        public static bool PuedeAtenderse(string estado, DateTime inicioLocal, DateTime finLocal)
        {
            var ahora = AgendaFechas.Ahora;

            return !EsFinal(estado) &&
                   ahora >= inicioLocal.AddMinutes(-MinutosAntesDeAtender) &&
                   ahora <= finLocal;
        }

        public static bool PuedeAtenderse(CitaDto cita) =>
            PuedeAtenderse(cita.Estado, cita.InicioLocal, cita.FinLocal);

        public static bool EsVencida(CitaDto cita) =>
            !EsFinal(cita.Estado) && AgendaFechas.Ahora > cita.FinLocal;

        public static bool PuedeModificarse(CitaDto cita) =>
            !EsFinal(cita.Estado) &&
            AgendaFechas.Ahora < cita.InicioLocal.AddMinutes(-MinutosAntesDeAtender);

        public static string[] Acciones(CitaDto cita)
        {
            if (PuedeAtenderse(cita))
                return ["NO_ASISTE", "CANCELADA"];

            if (!PuedeModificarse(cita))
                return [];

            return cita.Estado == "CONFIRMADA"
                ? ["CANCELADA"]
                : ["CONFIRMADA", "CANCELADA"];
        }

        public static IEnumerable<HorarioDto> HorariosDelDia(
            IEnumerable<HorarioDto> horarios, DateTime fecha, Func<HorarioDto, bool> aplica)
        {
            var dia = AgendaFechas.DiaSemana(fecha);
            var fechaDia = fecha.Date;

            return horarios.Where(x =>
                x.Estatus &&
                x.DiaSemana == dia &&
                (x.FechaDesde == null || x.FechaDesde.Value.Date <= fechaDia) &&
                (x.FechaHasta == null || x.FechaHasta.Value.Date >= fechaDia) &&
                aplica(x));
        }

        public static List<TimeSpan> HuecosLibres(
            DateTime fecha,
            int duracion,
            long? usuarioId,
            CatalogosAgenda catalogos,
            IEnumerable<CitaDto> citas,
            long? excluirCitaId)
        {
            var libres = new SortedSet<TimeSpan>();

            if (duracion <= 0)
                return libres.ToList();

            var ahora = AgendaFechas.Ahora;
            var largo = TimeSpan.FromMinutes(duracion);
            var paso = TimeSpan.FromMinutes(MinutosHueco);

            var horarios = HorariosDelDia(
                catalogos.Horarios, fecha, x => x.UsuarioId == usuarioId || x.UsuarioId == null);

            foreach (var horario in horarios)
            {
                for (var hora = horario.HoraInicio; hora + largo <= horario.HoraFin; hora += paso)
                {
                    var inicio = fecha.Date + hora;
                    var fin = inicio + largo;

                    if (inicio <= ahora)
                        continue;

                    var bloqueado = catalogos.Bloqueos.Any(x =>
                        x.Estatus &&
                        (x.UsuarioId == null || x.UsuarioId == usuarioId) &&
                        x.InicioLocal < fin && inicio < x.FinLocal);

                    var ocupado = citas.Any(x =>
                        x.Id != excluirCitaId &&
                        OcupaHorario(x.Estado) &&
                        (usuarioId == null || x.UsuarioId == null || x.UsuarioId == usuarioId) &&
                        x.InicioLocal < fin && inicio < x.FinLocal);

                    if (!bloqueado && !ocupado)
                        libres.Add(hora);
                }
            }

            return libres.ToList();
        }
    }
}
