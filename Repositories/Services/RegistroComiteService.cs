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
        // CRUD Comités Mejorado
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
                    FechaRegistro = model.FechaCreacion,
                    FechaEleccion = model.FechaEleccion,
                    NumeroResolucion = model.NumeroResolucion,
                    FechaResolucion = model.FechaResolucion,
                    RegionSaludId = model.RegionSaludId,
                    ProvinciaId = model.ProvinciaId,
                    DistritoId = model.DistritoId,
                    CorregimientoId = model.CorregimientoId,
                    TipoTramite = (int)model.TipoTramiteEnum,
                    NumeroNota = model.NumeroNota,
                };

                _context.TbComite.Add(entity);
                await _context.SaveChangesAsync();

                // Agregar miembros limitados a 7
                if (model.Miembros != null && model.Miembros.Any())
                {
                    var miembros = model.Miembros.Take(7).ToList();
                    await AgregarMiembrosComite(entity.DcomiteId, miembros);
                }

                // Guardar archivos si existen
                if (model.Archivos != null && model.Archivos.Any())
                {
                    foreach (var archivo in model.Archivos)
                    {
                        await AgregarArchivo(entity.DcomiteId, archivo);
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
                entity.FechaEleccion = model.FechaEleccion;
                entity.NumeroResolucion = model.NumeroResolucion;
                entity.FechaResolucion = model.FechaResolucion;
                entity.RegionSaludId = model.RegionSaludId;
                entity.ProvinciaId = model.ProvinciaId;
                entity.DistritoId = model.DistritoId;
                entity.CorregimientoId = model.CorregimientoId;
                entity.TipoTramite = (int)model.TipoTramiteEnum;
                entity.NumeroNota = model.NumeroNota;

                await _context.SaveChangesAsync();

                // Actualizar miembros
                if (model.Miembros != null)
                {
                    var miembros = model.Miembros.Take(7).ToList(); // máximo 7
                    await AgregarMiembrosComite(entity.DcomiteId, miembros);
                }

                // Actualizar archivos
                if (model.Archivos != null)
                {
                    foreach (var archivo in model.Archivos)
                    {
                        // Si no tiene ID, es nuevo
                        if (archivo.ComiteArchivoId == 0)
                            await AgregarArchivo(entity.DcomiteId, archivo);
                    }
                }

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
                    ComiteId = c.DcomiteId,
                    NombreComiteSalud = c.NombreComiteSalud,
                    Comunidad = c.Comunidad,
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
                .Where(c => c.DcomiteId == comiteId)
                .Select(c => new ComiteModel
                {
                    ComiteId = c.DcomiteId,
                    NombreComiteSalud = c.NombreComiteSalud,
                    Comunidad = c.Comunidad,
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
                    ComiteId = c.DcomiteId,
                    NombreComiteSalud = c.NombreComiteSalud,
                    Comunidad = c.Comunidad,
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

        public async Task<ResultModel> AgregarMiembrosComite(int comiteId, List<MiembroComiteModel> miembros)
        {
            // Limitar a 7 miembros
            if (miembros.Count > 7)
                return new ResultModel { Success = false, Message = "No se pueden agregar más de 7 miembros." };

            var miembrosExistentes = await _context.TbMiembrosComite
                .Where(m => m.DcomiteId == comiteId)
                .ToListAsync();

            // Eliminar los que ya no estén en la lista nueva
            foreach (var miembroDb in miembrosExistentes)
            {
                if (!miembros.Any(m => m.CedulaMiembro == miembroDb.CedulaMiembro))
                {
                    _context.TbMiembrosComite.Remove(miembroDb);
                }
            }

            // Agregar o actualizar los miembros
            foreach (var miembro in miembros)
            {
                var existing = miembrosExistentes.FirstOrDefault(m => m.CedulaMiembro == miembro.CedulaMiembro);
                if (existing != null)
                {
                    existing.NombreMiembro = miembro.NombreMiembro;
                    existing.ApellidoMiembro = miembro.ApellidoMiembro;
                    existing.CargoId = miembro.CargoId;
                }
                else
                {
                    await AgregarMiembro(comiteId, miembro);
                }
            }

            await _context.SaveChangesAsync();
            return new ResultModel { Success = true, Message = "Miembros actualizados correctamente" };
        }

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
            if (archivo == null) return new ResultModel { Success = false, Message = "No se recibió archivo" };

            var rutaCategoria = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "comites",
                comiteId.ToString());
            if (!Directory.Exists(rutaCategoria)) Directory.CreateDirectory(rutaCategoria);

            var nombreArchivo = Guid.NewGuid() + Path.GetExtension(archivo.Name);
            var filePath = Path.Combine(rutaCategoria, nombreArchivo);

            await using var stream = archivo.OpenReadStream(10 * 1024 * 1024); // 10MB
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await stream.CopyToAsync(fileStream);

            var entity = new TbArchivosComite
            {
                ComiteId = comiteId,
                NombreOriginal = archivo.Name,
                NombreArchivoGuardado = nombreArchivo,
                Url = $"/uploads/comites/{comiteId}/{nombreArchivo}",
                Categoria = "RESOLUCION",
                FechaSubida = DateTime.Now,
                Version = 1,
                IsActivo = true
            };

            _context.TbArchivosComite.Add(entity);
            await _context.SaveChangesAsync();

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