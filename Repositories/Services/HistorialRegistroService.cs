using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class HistorialRegistroService: IHistorialRegistro
{
    private readonly IDbContextFactory<DbContextLegal> _contextFactory;

    public HistorialRegistroService(IDbContextFactory<DbContextLegal> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task RegistrarHistorialAsociacionAsync(int detRegAsociacionId, int asociacionId, string accion, string comentario, string usuarioId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.TbDetalleRegAsociacionHistorial.Add(new TbDetalleRegAsociacionHistorial
        {
            DetRegAsociacionId = detRegAsociacionId,
            AsociacionId = asociacionId,
            Accion = accion,
            Comentario = comentario,
            UsuarioId = usuarioId,
            FechaModificacion = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    public async Task RegistrarHistorialComiteAsync(int detRegComiteId, int comiteId, int estadoId, string comentario, string usuarioId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.TbDetalleRegComiteHistorial.Add(new TbDetalleRegComiteHistorial
        {
            DetRegComiteId = detRegComiteId,
            ComiteId = comiteId,
            CoEstadoSolicitudId = estadoId,
            ComentarioCo = comentario,
            UsuarioRevisorCo = usuarioId,
            FechaCambioCo = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }
}