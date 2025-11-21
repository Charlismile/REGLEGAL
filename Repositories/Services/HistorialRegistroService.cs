using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
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

    public async Task RegistrarHistorialMiembrosCambioDirectivaAsync(int comiteBaseId, List<MiembroComiteModel> nuevosMiembros, string usuarioId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            // Obtener los miembros actuales del comité base (los que serán reemplazados)
            var miembrosActuales = await context.TbMiembrosComite
                .Where(m => m.ComiteId == comiteBaseId)
                .ToListAsync();

            if (miembrosActuales.Any())
            {
                var historiales = miembrosActuales.Select(miembro => new TbDatosMiembrosHistorial
                {
                    MiembroId = miembro.MiembroId,
                    CargoId = miembro.CargoId,
                    ComiteId = comiteBaseId,
                    FechaModificacion = DateTime.UtcNow,
                    NombreMiembro = miembro.NombreMiembro ?? "",
                    ApellidoMiembro = miembro.ApellidoMiembro ?? "",
                    CedulaMiembro = miembro.CedulaMiembro ?? "",
                    TelefonoMiembro = "",
                    CorreoMiembro = "",
                    FechaCambio = DateTime.UtcNow
                }).ToList();

                context.TbDatosMiembrosHistorial.AddRange(historiales);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Historial de {Cantidad} miembros anteriores guardado por cambio de directiva en comité {ComiteId}", 
                    historiales.Count, comiteBaseId);
            }

            // También registrar los nuevos miembros en el historial
            if (nuevosMiembros.Any())
            {
                var historialesNuevos = nuevosMiembros.Select(miembro => new TbDatosMiembrosHistorial
                {
                    MiembroId = 0, // Serán nuevos miembros
                    CargoId = miembro.CargoId,
                    ComiteId = comiteBaseId,
                    FechaModificacion = DateTime.UtcNow,
                    NombreMiembro = miembro.NombreMiembro ?? "",
                    ApellidoMiembro = miembro.ApellidoMiembro ?? "",
                    CedulaMiembro = miembro.CedulaMiembro ?? "",
                    TelefonoMiembro = "",
                    CorreoMiembro = "",
                    FechaCambio = DateTime.UtcNow
                }).ToList();

                context.TbDatosMiembrosHistorial.AddRange(historialesNuevos);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Historial de {Cantidad} nuevos miembros guardado por cambio de directiva en comité {ComiteId}", 
                    historialesNuevos.Count, comiteBaseId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar historial de cambio de directiva para comité base {ComiteBaseId}", comiteBaseId);
        }
    }

    // 🔹 MÉTODO PARA HISTORIAL EN JUNTA INTERVENTORA
    public async Task RegistrarHistorialMiembrosJuntaInterventoraAsync(int comiteBaseId, List<MiembroComiteModel> interventores, string usuarioId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            // Obtener los miembros actuales del comité base (los que serán intervenidos)
            var miembrosActuales = await context.TbMiembrosComite
                .Where(m => m.ComiteId == comiteBaseId)
                .ToListAsync();

            if (miembrosActuales.Any())
            {
                var historiales = miembrosActuales.Select(miembro => new TbDatosMiembrosHistorial
                {
                    MiembroId = miembro.MiembroId,
                    CargoId = miembro.CargoId,
                    ComiteId = comiteBaseId,
                    FechaModificacion = DateTime.UtcNow,
                    NombreMiembro = miembro.NombreMiembro ?? "",
                    ApellidoMiembro = miembro.ApellidoMiembro ?? "",
                    CedulaMiembro = miembro.CedulaMiembro ?? "",
                    TelefonoMiembro = "",
                    CorreoMiembro = "",
                    FechaCambio = DateTime.UtcNow
                }).ToList();

                context.TbDatosMiembrosHistorial.AddRange(historiales);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Historial de {Cantidad} miembros intervenidos guardado por junta interventora en comité {ComiteId}", 
                    historiales.Count, comiteBaseId);
            }

            // Registrar los interventores en el historial
            if (interventores.Any())
            {
                var historialesInterventores = interventores.Select(interventor => new TbDatosMiembrosHistorial
                {
                    MiembroId = 0, // Nuevos interventores
                    CargoId = interventor.CargoId,
                    ComiteId = comiteBaseId,
                    FechaModificacion = DateTime.UtcNow,
                    NombreMiembro = interventor.NombreMiembro ?? "",
                    ApellidoMiembro = interventor.ApellidoMiembro ?? "",
                    CedulaMiembro = interventor.CedulaMiembro ?? "",
                    TelefonoMiembro = "",
                    CorreoMiembro = "",
                    FechaCambio = DateTime.UtcNow
                }).ToList();

                context.TbDatosMiembrosHistorial.AddRange(historialesInterventores);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Historial de {Cantidad} interventores guardado para comité {ComiteId}", 
                    historialesInterventores.Count, comiteBaseId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar historial de junta interventora para comité base {ComiteBaseId}", comiteBaseId);
        }
    }

    // 🔹 MÉTODO GENÉRICO PARA ACTUALIZACIONES
    public async Task RegistrarHistorialMiembrosAsync(List<TbMiembrosComite> miembrosAntiguos, int comiteId, string usuarioId, string accion = "Actualización")
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var historiales = miembrosAntiguos.Select(miembro => new TbDatosMiembrosHistorial
            {
                MiembroId = miembro.MiembroId,
                CargoId = miembro.CargoId,
                ComiteId = comiteId,
                FechaModificacion = DateTime.UtcNow,
                NombreMiembro = miembro.NombreMiembro ?? "",
                ApellidoMiembro = miembro.ApellidoMiembro ?? "",
                CedulaMiembro = miembro.CedulaMiembro ?? "",
                TelefonoMiembro = "",
                CorreoMiembro = "",
                FechaCambio = DateTime.UtcNow
            }).ToList();

            if (historiales.Any())
            {
                context.TbDatosMiembrosHistorial.AddRange(historiales);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Historial de {Cantidad} miembros registrado exitosamente para comité {ComiteId} - Acción: {Accion}", 
                    historiales.Count, comiteId, accion);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar historial de miembros para comité {ComiteId}", comiteId);
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