using Microsoft.AspNetCore.Components.Forms;
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

        public RegistroComiteService(IDbContextFactory<DbContextLegal> contextFactory, ILogger<RegistroComiteService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        // ===============================
        // 🔹 CREAR COMITÉ
        // ===============================
        public async Task<ResultModel> CrearComite(ComiteModel model)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var result = new ResultModel();

            try
            {
                var entity = new TbComite
                {
                    TipoTramite = (int)model.TipoTramiteEnum,
                    CreadaPor = model.CreadaPor,
                    NombreComiteSalud = model.NombreComiteSalud,
                    Comunidad = model.Comunidad,
                    FechaRegistro = model.FechaCreacion,
                    FechaEleccion = model.FechaEleccion,
                    NumeroResolucion = model.NumeroResolucion,
                    NumeroNota = model.NumeroNota,
                    FechaResolucion = model.FechaResolucion,
                    RegionSaludId = model.RegionSaludId,
                    ProvinciaId = model.ProvinciaId,
                    DistritoId = model.DistritoId,
                    CorregimientoId = model.CorregimientoId
                };

                context.TbComite.Add(entity);
                await context.SaveChangesAsync();

                // Guardar miembros asociados
                if (model.Miembros.Any())
                {
                    foreach (var miembro in model.Miembros)
                    {
                        var entityMiembro = new TbMiembrosComite
                        {
                            ComiteId = entity.ComiteId,
                            NombreMiembro = miembro.NombreMiembro,
                            ApellidoMiembro = miembro.ApellidoMiembro,
                            CargoId = miembro.CargoId,
                            CedulaMiembro = miembro.CedulaMiembro
                        };
                        context.TbMiembrosComite.Add(entityMiembro);
                    }

                    await context.SaveChangesAsync();
                }

                result.Success = true;
                result.Id = entity.ComiteId;
                result.Message = "Comité creado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear comité");
                result.Message = $"Error al crear comité: {ex.Message}";
            }

            return result;
        }

        // ===============================
        // 🔹 ACTUALIZAR COMITÉ
        // ===============================
        public async Task<ResultModel> ActualizarComite(ComiteModel model)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var result = new ResultModel();

            try
            {
                var entity = await context.TbComite
                    .Include(c => c.TbMiembrosComite)
                    .FirstOrDefaultAsync(c => c.ComiteId == model.ComiteId);

                if (entity == null)
                {
                    result.Message = "Comité no encontrado.";
                    return result;
                }

                entity.NombreComiteSalud = model.NombreComiteSalud;
                entity.Comunidad = model.Comunidad;
                entity.NumeroResolucion = model.NumeroResolucion;
                entity.NumeroNota = model.NumeroNota;
                entity.FechaResolucion = model.FechaResolucion;
                entity.FechaEleccion = model.FechaEleccion;
                entity.RegionSaludId = model.RegionSaludId;
                entity.ProvinciaId = model.ProvinciaId;
                entity.DistritoId = model.DistritoId;
                entity.CorregimientoId = model.CorregimientoId;

                // Eliminar miembros anteriores
                context.TbMiembrosComite.RemoveRange(entity.TbMiembrosComite);

                // Agregar miembros actualizados
                foreach (var miembro in model.Miembros)
                {
                    var entityMiembro = new TbMiembrosComite
                    {
                        ComiteId = entity.ComiteId,
                        NombreMiembro = miembro.NombreMiembro,
                        ApellidoMiembro = miembro.ApellidoMiembro,
                        CargoId = miembro.CargoId,
                        CedulaMiembro = miembro.CedulaMiembro
                    };
                    context.TbMiembrosComite.Add(entityMiembro);
                }

                await context.SaveChangesAsync();

                result.Success = true;
                result.Id = entity.ComiteId;
                result.Message = "Comité actualizado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar comité");
                result.Message = $"Error al actualizar comité: {ex.Message}";
            }

            return result;
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
                    TipoTramiteEnum = (TipoTramite)c.TipoTramite  // ✅ INCLUIR ESTO
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
                FechaCreacion = entity.FechaRegistro?? DateTime.Today,   
                FechaEleccion = entity.FechaEleccion?? DateTime.Today,  
                NumeroResolucion = entity.NumeroResolucion,
                NumeroNota = entity.NumeroNota,
                FechaResolucion = entity.FechaResolucion?? DateTime.Today,
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
