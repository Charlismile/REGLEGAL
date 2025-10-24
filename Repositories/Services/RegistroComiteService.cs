﻿using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Data;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services
{
    public class RegistroComiteService : IRegistroComite
    {
        private readonly IDbContextFactory<DbContextLegal> _contextFactory;
        private readonly ILogger<RegistroComiteService> _logger;
        private readonly IHistorialRegistro _historialRegistro;

        public RegistroComiteService(IDbContextFactory<DbContextLegal> contextFactory,
            ILogger<RegistroComiteService> logger, IHistorialRegistro historialRegistro)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _historialRegistro = historialRegistro;
        }

        // ===============================
        // 🔹 CREAR COMITÉ
        // ===============================
        public async Task<ResultModel> CrearComite(ComiteModel model)
        {
            if (string.IsNullOrWhiteSpace(model.UsuarioId))
                return new ResultModel { Success = false, Message = "Usuario no autenticado." };

            await using var context = await _contextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // =============== 1. Crear el comité ===============
                var comite = new TbComite
                {
                    TipoTramite = (int)model.TipoTramiteEnum,
                    CreadaPor = model.UsuarioId, // usa el usuario autenticado
                    NombreComiteSalud = model.NombreComiteSalud?.Trim() ?? string.Empty,
                    Comunidad = model.Comunidad?.Trim(),
                    FechaRegistro = DateTime.UtcNow,
                    FechaEleccion = model.FechaEleccion,
                    NumeroResolucion = model.NumeroResolucion?.Trim(),
                    NumeroNota = model.NumeroNota?.Trim(),
                    FechaResolucion = model.FechaResolucion,
                    RegionSaludId = model.RegionSaludId,
                    ProvinciaId = model.ProvinciaId,
                    DistritoId = model.DistritoId,
                    CorregimientoId = model.CorregimientoId
                };

                context.TbComite.Add(comite);
                await context.SaveChangesAsync(); // Obtiene ComiteId

                // =============== 2. Crear miembros ===============
                if (model.Miembros?.Any() == true)
                {
                    foreach (var miembro in model.Miembros)
                    {
                        context.TbMiembrosComite.Add(new TbMiembrosComite
                        {
                            ComiteId = comite.ComiteId,
                            NombreMiembro = miembro.NombreMiembro?.Trim() ?? string.Empty,
                            ApellidoMiembro = miembro.ApellidoMiembro?.Trim() ?? string.Empty,
                            CedulaMiembro = miembro.CedulaMiembro?.Trim() ?? string.Empty,
                            CargoId = miembro.CargoId
                        });
                    }

                    await context.SaveChangesAsync();
                }

                // =============== 3. Crear registro formal (TbDetalleRegComite) ===============
                var detalleRegistro = new TbDetalleRegComite
                {
                    ComiteId = comite.ComiteId,
                    CreadaPor = model.UsuarioId,
                    CreadaEn = DateTime.UtcNow,
                    // Si usas numeración, agrégala aquí (similar a asociaciones)
                    NumeroRegistro = model.NumeroResolucion?.Trim(),
                    CoEstadoSolicitudId = 1 // "Enviada" - ajusta según tu catálogo de estados
                };

                context.TbDetalleRegComite.Add(detalleRegistro);
                await context.SaveChangesAsync(); // Obtiene DetRegComiteId

                // =============== 4. Registrar en historial ===============
                await _historialRegistro.RegistrarHistorialComiteAsync(
                    detRegComiteId: detalleRegistro.DetRegComiteId,
                    comiteId: comite.ComiteId,
                    estadoId: 1,
                    comentario: "Solicitud de comité creada",
                    usuarioId: model.UsuarioId
                );

                await transaction.CommitAsync();

                return new ResultModel
                {
                    Success = true,
                    Id = comite.ComiteId,
                    Message = "Comité creado correctamente.",
                    RegistroId = detalleRegistro.DetRegComiteId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear comité");
                await transaction.RollbackAsync();
                return new ResultModel { Success = false, Message = $"Error al crear comité: {ex.Message}" };
            }
        }

        // ===============================
        // 🔹 ACTUALIZAR COMITÉ
        // ===============================
        public async Task<ResultModel> ActualizarComite(ComiteModel model)
        {
            if (model.ComiteId <= 0)
                return new ResultModel { Success = false, Message = "ID de comité inválido." };

            if (string.IsNullOrWhiteSpace(model.UsuarioId))
                return new ResultModel { Success = false, Message = "Usuario no autenticado." };

            await using var context = await _contextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // =============== 1. Cargar comité con miembros ===============
                var comite = await context.TbComite
                    .Include(c => c.TbMiembrosComite)
                    .FirstOrDefaultAsync(c => c.ComiteId == model.ComiteId);

                if (comite == null)
                    return new ResultModel { Success = false, Message = "Comité no encontrado." };

                // =============== 2. Cargar registro formal ===============
                var detalleRegistro = await context.TbDetalleRegComite
                    .FirstOrDefaultAsync(d => d.ComiteId == model.ComiteId);

                if (detalleRegistro == null)
                    return new ResultModel { Success = false, Message = "Registro formal no encontrado." };

                // =============== 3. Actualizar comité ===============
                comite.NombreComiteSalud = model.NombreComiteSalud?.Trim() ?? comite.NombreComiteSalud;
                comite.Comunidad = model.Comunidad?.Trim() ?? comite.Comunidad;
                comite.NumeroResolucion = model.NumeroResolucion?.Trim() ?? comite.NumeroResolucion;
                comite.NumeroNota = model.NumeroNota?.Trim() ?? comite.NumeroNota;
                comite.FechaResolucion = model.FechaResolucion;
                comite.FechaEleccion = model.FechaEleccion;
                comite.RegionSaludId = model.RegionSaludId;
                comite.ProvinciaId = model.ProvinciaId;
                comite.DistritoId = model.DistritoId;
                comite.CorregimientoId = model.CorregimientoId;

                // =============== 4. Actualizar miembros (reemplazar) ===============
                context.TbMiembrosComite.RemoveRange(comite.TbMiembrosComite);
                if (model.Miembros?.Any() == true)
                {
                    foreach (var miembro in model.Miembros)
                    {
                        context.TbMiembrosComite.Add(new TbMiembrosComite
                        {
                            ComiteId = comite.ComiteId,
                            NombreMiembro = miembro.NombreMiembro?.Trim() ?? string.Empty,
                            ApellidoMiembro = miembro.ApellidoMiembro?.Trim() ?? string.Empty,
                            CedulaMiembro = miembro.CedulaMiembro?.Trim() ?? string.Empty,
                            CargoId = miembro.CargoId
                        });
                    }
                }

                // =============== 5. Actualizar registro formal ===============
                detalleRegistro.ModificadaPor = model.UsuarioId;
                detalleRegistro.ModificadaEn = DateTime.UtcNow;
                // Si el estado cambia, actualízalo aquí:
                // detalleRegistro.CoEstadoSolicitudId = model.NuevoEstadoId;

                context.TbComite.Update(comite);
                context.TbDetalleRegComite.Update(detalleRegistro);

                await context.SaveChangesAsync();

                // =============== 6. Registrar en historial ===============
                await _historialRegistro.RegistrarHistorialComiteAsync(
                    detRegComiteId: detalleRegistro.DetRegComiteId,
                    comiteId: comite.ComiteId,
                    estadoId: detalleRegistro.CoEstadoSolicitudId,
                    comentario: "Comité actualizado por el usuario",
                    usuarioId: model.UsuarioId
                );

                await transaction.CommitAsync();

                return new ResultModel
                {
                    Success = true,
                    Id = comite.ComiteId,
                    Message = "Comité actualizado correctamente.",
                    RegistroId = detalleRegistro.DetRegComiteId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar comité");
                await transaction.RollbackAsync();
                return new ResultModel { Success = false, Message = $"Error al actualizar comité: {ex.Message}" };
            }
        }

        // ===============================
        // 🔹 ELIMINAR COMITÉ
        // ===============================
        public async Task<ResultModel> EliminarComite(int comiteId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var result = new ResultModel();

            try
            {
                var entity = await context.TbComite.FindAsync(comiteId);

                if (entity == null)
                {
                    result.Message = "Comité no encontrado.";
                    return result;
                }

                context.TbComite.Remove(entity);
                await context.SaveChangesAsync();

                result.Success = true;
                result.Message = "Comité eliminado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar comité");
                result.Message = $"Error al eliminar comité: {ex.Message}";
            }

            return result;
        }

        // ===============================
        // 🔹 OBTENER TODOS LOS COMITÉS (CORREGIDO)
        // ===============================
        public async Task<List<ComiteModel>> ObtenerComites()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.TbComite
                .Select(c => new ComiteModel
                {
                    ComiteId = c.ComiteId,
                    NombreComiteSalud = c.NombreComiteSalud,
                    TipoTramiteEnum = (TipoTramite)c.TipoTramite // ✅ INCLUIR ESTO
                })
                .OrderBy(c => c.NombreComiteSalud)
                .ToListAsync();
        }

        // ===============================
        // 🔹 OBTENER COMITÉS POR TIPO DE TRÁMITE
        // ===============================
        public async Task<List<ComiteModel>> ObtenerComitesPorTipo(TipoTramite tipoTramite)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.TbComite
                .Where(c => c.TipoTramite == (int)tipoTramite)
                .Select(c => new ComiteModel
                {
                    ComiteId = c.ComiteId,
                    NombreComiteSalud = c.NombreComiteSalud,
                    TipoTramiteEnum = (TipoTramite)c.TipoTramite
                })
                .OrderBy(c => c.NombreComiteSalud)
                .ToListAsync();
        }

        // ===============================
        // 🔹 OBTENER COMITÉ COMPLETO
        // ===============================
        public async Task<ComiteModel?> ObtenerComiteCompletoAsync(int comiteId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var entity = await context.TbComite
                .Include(c => c.TbMiembrosComite)
                .Include(c => c.TbArchivosComite)
                .FirstOrDefaultAsync(c => c.ComiteId == comiteId);

            if (entity == null) return null;

            return new ComiteModel
            {
                ComiteId = entity.ComiteId,
                TipoTramiteEnum = (TipoTramite)entity.TipoTramite,
                NombreComiteSalud = entity.NombreComiteSalud,
                Comunidad = entity.Comunidad,
                FechaCreacion = entity.FechaRegistro ?? DateTime.Today,
                FechaEleccion = entity.FechaEleccion ?? DateTime.Today,
                NumeroResolucion = entity.NumeroResolucion,
                NumeroNota = entity.NumeroNota,
                FechaResolucion = entity.FechaResolucion ?? DateTime.Today,
                RegionSaludId = entity.RegionSaludId,
                ProvinciaId = entity.ProvinciaId,
                DistritoId = entity.DistritoId,
                CorregimientoId = entity.CorregimientoId,
                Miembros = entity.TbMiembrosComite.Select(m => new MiembroComiteModel
                {
                    MiembroId = m.MiembroId,
                    NombreMiembro = m.NombreMiembro,
                    ApellidoMiembro = m.ApellidoMiembro,
                    CedulaMiembro = m.CedulaMiembro,
                    CargoId = m.CargoId
                }).ToList()
            };
        }

        public async Task<List<HistorialComiteModel>> ObtenerHistorialComite(int comiteId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.TbDetalleRegComiteHistorial
                .Where(h => h.ComiteId == comiteId)
                .OrderByDescending(h => h.FechaCambioCo)
                .Select(h => new HistorialComiteModel
                {
                    HistorialId = h.RegComiteSolId,
                    ComiteId = h.ComiteId,
                    Accion = MapEstadoAccion(h.CoEstadoSolicitudId), // ← conversión legible
                    Comentario = h.ComentarioCo,
                    Usuario = h.UsuarioRevisorCo,
                    Fecha = h.FechaCambioCo ?? DateTime.Today,
                })
                .ToListAsync();
        }

// Método auxiliar para convertir estado numérico a texto
        private string MapEstadoAccion(int estadoId)
        {
            return estadoId switch
            {
                1 => "Creado",
                2 => "Actualizado",
                3 => "Eliminado", // si aplica
                _ => $"Acción {estadoId}"
            };
        }
        // ===============================
        // 🔹 GUARDAR ARCHIVO DE RESOLUCIÓN
        // ===============================
        public async Task<ResultModel> GuardarResolucionAsync(int comiteId, IBrowserFile archivo)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var result = new ResultModel();
            try
            {
                var entity = await context.TbComite.FindAsync(comiteId);
                if (entity == null)
                {
                    result.Message = "Comité no encontrado.";
                    return result;
                }

                var nombreArchivo = $"{Guid.NewGuid()}_{archivo.Name}";
                var ruta = Path.Combine("wwwroot", "uploads", nombreArchivo);

                Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);

                await using (var stream = File.Create(ruta))
                {
                    await archivo.OpenReadStream().CopyToAsync(stream);
                }

                var nuevoArchivo = new TbArchivosComite
                {
                    ComiteId = comiteId,
                    NombreArchivoGuardado = archivo.Name,
                    Url = ruta,
                    FechaSubida = DateTime.Now
                };
                context.TbArchivosComite.Add(nuevoArchivo);
                await context.SaveChangesAsync();

                result.Success = true;
                result.Message = "Archivo guardado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar archivo");
                result.Message = $"Error al guardar archivo: {ex.Message}";
            }

            return result;
        }
    }
}