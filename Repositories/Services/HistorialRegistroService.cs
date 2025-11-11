using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class HistorialRegistroService: IHistorialRegistro
{
    private readonly IDbContextFactory<DbContextLegal> _contextFactory;
    private readonly ILogger<HistorialRegistroService> _logger;

    public HistorialRegistroService(IDbContextFactory<DbContextLegal> contextFactory, ILogger<HistorialRegistroService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task RegistrarHistorialComiteAsync(int detRegComiteId, int comiteId, int estadoId, string comentario, string usuarioId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var historial = new TbDetalleRegComiteHistorial
            {
                DetRegComiteId = detRegComiteId,
                ComiteId = comiteId,
                CoEstadoSolicitudId = estadoId,
                ComentarioCo = comentario,
                UsuarioRevisorCo = usuarioId,
                FechaCambioCo = DateTime.UtcNow
            };
            
            context.TbDetalleRegComiteHistorial.Add(historial);
            await context.SaveChangesAsync();
            
            _logger.LogInformation("Historial registrado exitosamente para comité {ComiteId}", comiteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar historial para comité {ComiteId}. DetRegComiteId: {DetRegComiteId}", comiteId, detRegComiteId);
            // No relanzamos la excepción para no interrumpir el flujo principal
        }
    }

    public async Task RegistrarHistorialAsociacionAsync(int detRegAsociacionId, int asociacionId, string accion, string comentario, string usuarioId)
    {
        try
        {
            _logger.LogInformation("Intentando registrar historial para asociación {AsociacionId}, DetRegAsociacionId: {DetRegAsociacionId}", asociacionId, detRegAsociacionId);
        
            await using var context = await _contextFactory.CreateDbContextAsync();
        
            var historial = new TbDetalleRegAsociacionHistorial
            {
                DetRegAsociacionId = detRegAsociacionId,
                AsociacionId = asociacionId,
                Accion = accion,
                Comentario = comentario,
                UsuarioId = usuarioId,
                FechaModificacion = DateTime.UtcNow
            };
        
            context.TbDetalleRegAsociacionHistorial.Add(historial);
            await context.SaveChangesAsync();
        
            _logger.LogInformation("✅ Historial registrado exitosamente para asociación {AsociacionId}", asociacionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al registrar historial para asociación {AsociacionId}. DetRegAsociacionId: {DetRegAsociacionId}", asociacionId, detRegAsociacionId);
        }
    }
}