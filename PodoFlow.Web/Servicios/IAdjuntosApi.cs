using Microsoft.AspNetCore.Components.Forms;
using PodoFlow.Web.Modelos.Atenciones;

namespace PodoFlow.Web.Servicios
{
    public interface IAdjuntosApi
    {
        Task<List<AdjuntoDto>> ListarAsync(long atencionId);

        Task<AdjuntoDto> SubirAsync(long atencionId, long usuarioId, IBrowserFile archivo, string? nombre);

        Task<AdjuntoArchivo> DescargarAsync(long adjuntoId);

        Task DesactivarAsync(long adjuntoId);

        Task ReactivarAsync(long adjuntoId);
    }
}
