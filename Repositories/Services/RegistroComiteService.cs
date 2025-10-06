using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services
{
    public class RegistroComiteService : IRegistroComite
    {
        private readonly DbContextLegal _context;

        public RegistroComiteService(DbContextLegal context)
        {
            _context = context;
        }

        // ========================
        // CRUD Comités
        // ========================

        public async Task<ResultModel> CrearComite(ComiteModel model)
        {
            try
            {
                var entity = new TbComite
                {
                    NombreComiteSalud = model.NombreComiteSalud,
                    Comunidad = model.Comunidad,
                    CreadaPor = model.CreadaPor,
                    TipoTramiteId = (int)model.TipoTramiteEnum,
                    FechaRegistro = model.FechaCreacion,
                    FechaEleccion = model.FechaEleccion,
                    NumeroResolucion = model.NumeroResolucion,
                    FechaResolucion = model.FechaResolucion,
                    RegionSaludId = model.RegionSaludId,
                    ProvinciaId = model.ProvinciaId,
                    DistritoId = model.DistritoId,
                    CorregimientoId = model.CorregimientoId
                };

                _context.TbComite.Add(entity);
                await _context.SaveChangesAsync();

                // Agregar miembros si existen
                if (model.Miembros.Any())
                {
                    foreach (var miembro in model.Miembros)
                    {
                        await AgregarMiembro(entity.ComiteId, miembro);
                    }
                }

                return new ResultModel { Success = true, Message = "Comité creado correctamente" };
            }
            catch (Exception ex)
            {
                return new ResultModel { Success = false, Message = ex.Message };
            }
        }

        public async Task<ResultModel> ActualizarComite(ComiteModel model)
        {
            try
            {
                var entity = await _context.TbComite.FindAsync(model.ComiteId);
                if (entity == null) return new ResultModel { Success = false, Message = "Comité no encontrado" };

                entity.NombreComiteSalud = model.NombreComiteSalud;
                entity.Comunidad = model.Comunidad;
                entity.TipoTramiteId = (int)model.TipoTramiteEnum;
                entity.FechaEleccion = model.FechaEleccion;
                entity.NumeroResolucion = model.NumeroResolucion;
                entity.FechaResolucion = model.FechaResolucion;
                entity.RegionSaludId = model.RegionSaludId;
                entity.ProvinciaId = model.ProvinciaId;
                entity.DistritoId = model.DistritoId;
                entity.CorregimientoId = model.CorregimientoId;

                await _context.SaveChangesAsync();
                return new ResultModel { Success = true, Message = "Comité actualizado correctamente" };
            }
            catch (Exception ex)
            {
                return new ResultModel { Success = false, Message = ex.Message };
            }
        }

        public async Task<ResultModel> EliminarComite(int comiteId)
        {
            try
            {
                var entity = await _context.TbComite.FindAsync(comiteId);
                if (entity == null) return new ResultModel { Success = false, Message = "Comité no encontrado" };

                _context.TbComite.Remove(entity);
                await _context.SaveChangesAsync();
                return new ResultModel { Success = true, Message = "Comité eliminado correctamente" };
            }
            catch (Exception ex)
            {
                return new ResultModel { Success = false, Message = ex.Message };
            }
        }

        public async Task<List<ComiteModel>> ObtenerComites()
        {
            return await _context.TbComite
                .Select(c => new ComiteModel
                {
                    ComiteId = c.ComiteId,
                    NombreComiteSalud = c.NombreComiteSalud,
                    Comunidad = c.Comunidad,
                    TipoTramiteEnum = c.TipoTramiteId != null ? (TipoTramite)c.TipoTramiteId : TipoTramite.Personeria,
                    FechaCreacion = c.FechaRegistro ?? DateTime.Now,
                    FechaEleccion = c.FechaEleccion ?? DateTime.Now,
                    NumeroResolucion = c.NumeroResolucion,
                    FechaResolucion = c.FechaResolucion ?? DateTime.Now
                }).ToListAsync();
        }

        public async Task<ComiteModel?> ObtenerComiteCompletoAsync(int comiteId)
        {
            return await _context.TbComite
                .Include(c => c.TbMiembrosComite)
                .Include(c => c.TbArchivosComite)
                .Where(c => c.ComiteId == comiteId)
                .Select(c => new ComiteModel
                {
                    ComiteId = c.ComiteId,
                    NombreComiteSalud = c.NombreComiteSalud,
                    Comunidad = c.Comunidad,
                    TipoTramiteEnum = c.TipoTramiteId != null ? (TipoTramite)c.TipoTramiteId : TipoTramite.Personeria,
                    FechaCreacion = c.FechaRegistro ?? DateTime.Now,
                    FechaEleccion = c.FechaEleccion ?? DateTime.Now,
                    NumeroResolucion = c.NumeroResolucion,
                    FechaResolucion = c.FechaResolucion ?? DateTime.Now,
                    Miembros = c.TbMiembrosComite.Select(m => new MiembroComiteModel
                    {
                        MiembroId = m.DmiembroId,
                        NombreMiembro = m.NombreMiembro,
                        ApellidoMiembro = m.ApellidoMiembro,
                        CedulaMiembro = m.CedulaMiembro,
                        CargoId = m.CargoId
                    }).ToList(),
                    Archivos = c.TbArchivosComite.Select(a => new CArchivoModel
                    {
                        ComiteArchivoId = a.ArchivoId,
                        NombreArchivo = a.NombreOriginal,
                        RutaArchivo = a.Url
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<ComiteModel?> ObtenerUltimoComiteConMiembrosAsync()
        {
            return await _context.TbComite
                .Include(c => c.TbMiembrosComite)
                .OrderByDescending(c => c.FechaRegistro)
                .Select(c => new ComiteModel
                {
                    ComiteId = c.ComiteId,
                    NombreComiteSalud = c.NombreComiteSalud,
                    Comunidad = c.Comunidad,
                    TipoTramiteEnum = c.TipoTramiteId != null ? (TipoTramite)c.TipoTramiteId : TipoTramite.Personeria,
                    FechaCreacion = c.FechaRegistro ?? DateTime.Now,
                    Miembros = c.TbMiembrosComite.Select(m => new MiembroComiteModel
                    {
                        MiembroId = m.DmiembroId,
                        NombreMiembro = m.NombreMiembro,
                        ApellidoMiembro = m.ApellidoMiembro,
                        CedulaMiembro = m.CedulaMiembro,
                        CargoId = m.CargoId
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        // ========================
        // Miembros
        // ========================

        public async Task<TbMiembrosComite> AgregarMiembro(int comiteId, MiembroComiteModel miembro)
        {
            var entity = new TbMiembrosComite
            {
                DcomiteId = comiteId,
                NombreMiembro = miembro.NombreMiembro,
                ApellidoMiembro = miembro.ApellidoMiembro,
                CedulaMiembro = miembro.CedulaMiembro,
                CargoId = miembro.CargoId
            };
            _context.TbMiembrosComite.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ResultModel> ActualizarMiembro(MiembroComiteModel miembro)
        {
            var entity = await _context.TbMiembrosComite.FindAsync(miembro.MiembroId);
            if (entity == null) return new ResultModel { Success = false, Message = "Miembro no encontrado" };

            entity.NombreMiembro = miembro.NombreMiembro;
            entity.ApellidoMiembro = miembro.ApellidoMiembro;
            entity.CedulaMiembro = miembro.CedulaMiembro;
            entity.CargoId = miembro.CargoId;

            await _context.SaveChangesAsync();
            return new ResultModel { Success = true, Message = "Miembro actualizado correctamente" };
        }

        public async Task<ResultModel> EliminarMiembro(int miembroId)
        {
            var entity = await _context.TbMiembrosComite.FindAsync(miembroId);
            if (entity == null) return new ResultModel { Success = false, Message = "Miembro no encontrado" };

            _context.TbMiembrosComite.Remove(entity);
            await _context.SaveChangesAsync();
            return new ResultModel { Success = true, Message = "Miembro eliminado correctamente" };
        }

        public async Task<List<MiembroComiteModel>> ObtenerMiembros(int comiteId)
        {
            return await _context.TbMiembrosComite
                .Where(m => m.DcomiteId == comiteId)
                .Select(m => new MiembroComiteModel
                {
                    MiembroId = m.DmiembroId,
                    NombreMiembro = m.NombreMiembro,
                    ApellidoMiembro = m.ApellidoMiembro,
                    CedulaMiembro = m.CedulaMiembro,
                    CargoId = m.CargoId
                }).ToListAsync();
        }

        public async Task<List<CargoModel>> ObtenerCargos()
        {
            return await _context.TbCargosMiembrosComite
                .Select(c => new CargoModel
                {
                    CargoId = c.CargoId,
                    NombreCargo = c.NombreCargo
                }).ToListAsync();
        }

        // ========================
        // Archivos
        // ========================

        public async Task<ResultModel> AgregarArchivo(int comiteId, CArchivoModel archivo)
        {
            var entity = new TbArchivosComite
            {
                ComiteId = comiteId,
                NombreOriginal = archivo.NombreArchivo,
                Url = archivo.RutaArchivo
            };
            _context.TbArchivosComite.Add(entity);
            await _context.SaveChangesAsync();
            return new ResultModel { Success = true, Message = "Archivo agregado correctamente" };
        }

        public async Task<ResultModel> EliminarArchivo(int archivoId)
        {
            var entity = await _context.TbArchivosComite.FindAsync(archivoId);
            if (entity == null) return new ResultModel { Success = false, Message = "Archivo no encontrado" };

            _context.TbArchivosComite.Remove(entity);
            await _context.SaveChangesAsync();
            return new ResultModel { Success = true, Message = "Archivo eliminado correctamente" };
        }

        public async Task<List<CArchivoModel>> ObtenerArchivos(int comiteId)
        {
            return await _context.TbArchivosComite
                .Where(a => a.ComiteId == comiteId)
                .Select(a => new CArchivoModel
                {
                    ComiteArchivoId = a.ArchivoId,
                    NombreArchivo = a.NombreOriginal,
                    RutaArchivo = a.Url
                }).ToListAsync();
        }

        public async Task<ResultModel> GuardarResolucionAsync(int comiteId, IBrowserFile archivo)
        {
            // Implementar guardado físico + registro
            return new ResultModel { Success = true, Message = "Resolución guardada correctamente" };
        }

        // ========================
        // Historial
        // ========================

        public async Task<List<DetalleRegComiteModel>> ObtenerDetalleHistorial(int comiteId)
        {
            return await _context.TbDatosMiembrosHistorial
                .Where(h => h.DcomiteId == comiteId)
                .Select(h => new DetalleRegComiteModel
                {
                    DMiembroId = h.DmiembroId,
                    NombreMiembro = h.NombreMiembro,
                    CargoId = h.CargoId,
                    FechaCambio = h.FechaCambio 
                }).ToListAsync();
        }

        public async Task GuardarHistorialMiembros(int comiteId, List<MiembroComiteModel> miembros)
        {
            foreach (var miembro in miembros)
            {
                var historial = new TbDatosMiembrosHistorial
                {
                    DcomiteId = comiteId,
                    DmiembroId = miembro.MiembroId,
                    NombreMiembro = miembro.NombreMiembro,
                    CargoId = miembro.CargoId,
                    FechaCambio = DateTime.Now
                };
                _context.TbDatosMiembrosHistorial.Add(historial);
            }
            await _context.SaveChangesAsync();
        }
    }
}
