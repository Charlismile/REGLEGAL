using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services
{
    public class RegistroAsociacionService : IRegistroAsociacion
    {
        private readonly DbContextLegal _context;

        public RegistroAsociacionService(DbContextLegal context)
        {
            _context = context;
        }

        // ========================
        // CRUD Básico
        // ========================
        public async Task<ResultModel> CrearAsociacion(AsociacionModel model)
        {
            try
            {
                var entity = new TbAsociacion
                {
                    NombreAsociacion = model.NombreAsociacion?.Trim() ?? string.Empty,
                    FechaResolucion = model.FechaResolucion ?? DateTime.Now,
                    Folio = model.Folio, 
                    Actividad = model.Actividad,
                    NumeroResolucion = model.NumeroResolucion
                };

                // Representante y Apoderado
                if (!string.IsNullOrWhiteSpace(model.NombreRepLegal))
                {
                    entity.RepresentanteLegal = new TbRepresentanteLegal
                    {
                        NombreRepLegal = model.NombreRepLegal.Trim(),
                        CedulaRepLegal = model.CedulaRepLegal?.Trim() ?? string.Empty,
                        CargoRepLegal = model.CargoRepLegal ?? string.Empty,
                        TelefonoRepLegal = model.TelefonoRepLegal,
                        DireccionRepLegal = model.DireccionRepLegal,
                        ApellidoRepLegal = model.ApellidoRepLegal
                    };
                }

                if (!string.IsNullOrWhiteSpace(model.NombreApoAbogado))
                {
                    entity.ApoderadoLegal = new TbApoderadoLegal
                    {
                        NombreApoAbogado = model.NombreApoAbogado.Trim(),
                        CedulaApoAbogado = model.CedulaApoAbogado?.Trim() ?? string.Empty,
                        TelefonoApoAbogado = model.TelefonoApoAbogado,
                        DireccionApoAbogado = model.DireccionApoAbogado,
                        CorreoApoAbogado = model.CorreoApoAbogado,
                        ApellidoApoAbogado = model.ApellidoApoAbogado
                        // ApoderadoFirmaId si lo proporcionas en model
                    };
                }

                _context.TbAsociacion.Add(entity);
                await _context.SaveChangesAsync();

                // Guardar archivos si existen
                if (model.Archivos != null && model.Archivos.Any())
                {
                    foreach (var archivo in model.Archivos)
                    {
                        // si el archivo viene con NombreArchivoGuardado nativo, mantenerlo; si no, generarlo.
                        if (string.IsNullOrWhiteSpace(archivo.NombreArchivoGuardado))
                        {
                            archivo.NombreArchivoGuardado = GenerateStoredFileName(archivo.NombreArchivo);
                        }

                        // si no tiene categoría, asignar 'General'
                        if (string.IsNullOrWhiteSpace(archivo.Categoria))
                            archivo.Categoria = "General";

                        await AgregarArchivo(entity.AsociacionId, archivo);
                    }
                }

                return new ResultModel { Success = true, Message = "Asociación creada correctamente.", AsociacionId = entity.AsociacionId};
            }
            catch (Exception ex)
            {
                return new ResultModel { Success = false, Message = ex.Message };
            }
        }

        public async Task<ResultModel> ActualizarAsociacion(AsociacionModel model)
        {
            try
            {
                var entity = await _context.TbAsociacion
                    .Include(a => a.RepresentanteLegal)
                    .Include(a => a.ApoderadoLegal)
                    .Include(a => a.TbArchivosAsociacion)
                    .FirstOrDefaultAsync(a => a.AsociacionId == model.AsociacionId);

                if (entity == null)
                    return new ResultModel { Success = false, Message = "Asociación no encontrada." };

                entity.NombreAsociacion = model.NombreAsociacion?.Trim() ?? entity.NombreAsociacion;
                entity.FechaResolucion = model.FechaResolucion ?? entity.FechaResolucion;
                entity.Folio = model.Folio != 0 ? model.Folio : entity.Folio;
                entity.Actividad = model.Actividad ?? entity.Actividad;
                entity.NumeroResolucion = model.NumeroResolucion ?? entity.NumeroResolucion;

                // Actualizar representante
                if (entity.RepresentanteLegal != null)
                {
                    entity.RepresentanteLegal.NombreRepLegal = model.NombreRepLegal ?? entity.RepresentanteLegal.NombreRepLegal;
                    entity.RepresentanteLegal.ApellidoRepLegal = model.ApellidoRepLegal ?? entity.RepresentanteLegal.ApellidoRepLegal;
                    entity.RepresentanteLegal.CedulaRepLegal = model.CedulaRepLegal?.Trim() ?? entity.RepresentanteLegal.CedulaRepLegal;
                    entity.RepresentanteLegal.CargoRepLegal = model.CargoRepLegal ?? entity.RepresentanteLegal.CargoRepLegal;
                    entity.RepresentanteLegal.TelefonoRepLegal = model.TelefonoRepLegal ?? entity.RepresentanteLegal.TelefonoRepLegal;
                    entity.RepresentanteLegal.DireccionRepLegal = model.DireccionRepLegal ?? entity.RepresentanteLegal.DireccionRepLegal;
                    
                }
                else if (!string.IsNullOrEmpty(model.NombreRepLegal))
                {
                    entity.RepresentanteLegal = new TbRepresentanteLegal
                    {
                        NombreRepLegal = model.NombreRepLegal.Trim(),
                        ApellidoRepLegal = model.ApellidoRepLegal.Trim(),
                        CedulaRepLegal = model.CedulaRepLegal?.Trim() ?? string.Empty,
                        CargoRepLegal = model.CargoRepLegal ?? string.Empty,
                        TelefonoRepLegal = model.TelefonoRepLegal,
                        DireccionRepLegal = model.DireccionRepLegal
                    };
                }

                // Actualizar apoderado
                if (entity.ApoderadoLegal != null)
                {
                    entity.ApoderadoLegal.NombreApoAbogado = model.NombreApoAbogado ?? entity.ApoderadoLegal.NombreApoAbogado;
                    entity.ApoderadoLegal.ApellidoApoAbogado = model.ApellidoApoAbogado ?? entity.ApoderadoLegal.ApellidoApoAbogado;
                    entity.ApoderadoLegal.CedulaApoAbogado = model.CedulaApoAbogado?.Trim() ?? entity.ApoderadoLegal.CedulaApoAbogado;
                    entity.ApoderadoLegal.TelefonoApoAbogado = model.TelefonoApoAbogado ?? entity.ApoderadoLegal.TelefonoApoAbogado;
                    entity.ApoderadoLegal.DireccionApoAbogado = model.DireccionApoAbogado ?? entity.ApoderadoLegal.DireccionApoAbogado;
                    entity.ApoderadoLegal.CorreoApoAbogado = model.CorreoApoAbogado ?? entity.ApoderadoLegal.CorreoApoAbogado;
                    
                }
                else if (!string.IsNullOrEmpty(model.NombreApoAbogado))
                {
                    entity.ApoderadoLegal = new TbApoderadoLegal
                    {
                        NombreApoAbogado = model.NombreApoAbogado.Trim(),
                        ApellidoApoAbogado = model.ApellidoApoAbogado.Trim(),
                        CedulaApoAbogado = model.CedulaApoAbogado?.Trim() ?? string.Empty,
                        TelefonoApoAbogado = model.TelefonoApoAbogado,
                        DireccionApoAbogado = model.DireccionApoAbogado,
                        CorreoApoAbogado = model.CorreoApoAbogado
                        
                    };
                }

                await _context.SaveChangesAsync();

                // Actualizar archivos: agregar nuevos
                if (model.Archivos != null && model.Archivos.Any())
                {
                    foreach (var archivo in model.Archivos)
                    {
                        if (archivo.AsociacionArchivoId == 0)
                        {
                            if (string.IsNullOrWhiteSpace(archivo.NombreArchivoGuardado))
                                archivo.NombreArchivoGuardado = GenerateStoredFileName(archivo.NombreArchivo);
                            if (string.IsNullOrWhiteSpace(archivo.Categoria))
                                archivo.Categoria = "General";

                            await AgregarArchivo(entity.AsociacionId, archivo);
                        }
                        else
                        {
                            
                        }
                    }
                }

                return new ResultModel { Success = true, Message = "Asociación actualizada correctamente." };
            }
            catch (Exception ex)
            {
                return new ResultModel { Success = false, Message = ex.Message };
            }
        }

        public async Task<ResultModel> EliminarAsociacion(int id)
        {
            try
            {
                var entity = await _context.TbAsociacion.FindAsync(id);
                if (entity == null)
                    return new ResultModel { Success = false, Message = "Asociación no encontrada." };

                _context.TbAsociacion.Remove(entity);
                await _context.SaveChangesAsync();
                return new ResultModel { Success = true, Message = "Asociación eliminada correctamente." };
            }
            catch (Exception ex)
            {
                return new ResultModel { Success = false, Message = ex.Message };
            }
        }

        public async Task<AsociacionModel?> ObtenerPorId(int id)
        {
            var a = await _context.TbAsociacion
                .Include(a => a.RepresentanteLegal)
                .Include(a => a.ApoderadoLegal)
                .Include(a => a.TbArchivosAsociacion)
                .FirstOrDefaultAsync(a => a.AsociacionId == id);

            if (a == null) return null;

            return new AsociacionModel
            {
                AsociacionId = a.AsociacionId,
                NombreAsociacion = a.NombreAsociacion,
                NombreRepLegal = a.RepresentanteLegal?.NombreRepLegal,
                ApellidoRepLegal = a.RepresentanteLegal?.ApellidoRepLegal,
                CedulaRepLegal = a.RepresentanteLegal?.CedulaRepLegal,
                CargoRepLegal = a.RepresentanteLegal?.CargoRepLegal,
                NombreApoAbogado = a.ApoderadoLegal?.NombreApoAbogado,
                CedulaApoAbogado = a.ApoderadoLegal?.CedulaApoAbogado,
                FechaResolucion = a.FechaResolucion ?? DateTime.Now,
                Folio = a.Folio,
                Actividad = a.Actividad,
                NumeroResolucion = a.NumeroResolucion,
                Archivos = a.TbArchivosAsociacion?.Select(f => new AArchivoModel
                {
                    AsociacionArchivoId = f.ArchivoId,
                    NombreArchivo = f.NombreOriginal,
                    NombreArchivoGuardado = f.NombreArchivoGuardado,
                    RutaArchivo = f.Url,
                    Categoria = f.Categoria,
                    SubidoEn = f.FechaSubida
                }).ToList() ?? new List<AArchivoModel>()
            };
        }

        public async Task<List<AsociacionModel>> ObtenerTodas()
        {
            return await _context.TbAsociacion
                .Include(a => a.RepresentanteLegal)
                .Include(a => a.ApoderadoLegal)
                .Select(a => new AsociacionModel
                {
                    AsociacionId = a.AsociacionId,
                    NombreAsociacion = a.NombreAsociacion,
                    NombreRepLegal = a.RepresentanteLegal != null ? a.RepresentanteLegal.NombreRepLegal : null,
                    ApellidoRepLegal = a.RepresentanteLegal != null ? a.RepresentanteLegal.ApellidoRepLegal : null,
                    CedulaRepLegal = a.RepresentanteLegal != null ? a.RepresentanteLegal.CedulaRepLegal : null,
                    CargoRepLegal = a.RepresentanteLegal != null ? a.RepresentanteLegal.CargoRepLegal : null,
                    NombreApoAbogado = a.ApoderadoLegal != null ? a.ApoderadoLegal.NombreApoAbogado : null,
                    ApellidoApoAbogado = a.ApoderadoLegal != null ? a.ApoderadoLegal.ApellidoApoAbogado : null,
                    CedulaApoAbogado = a.ApoderadoLegal != null ? a.ApoderadoLegal.CedulaApoAbogado : null,
                    FechaResolucion = a.FechaResolucion ?? DateTime.Now,
                    Folio = a.Folio,
                    Actividad = a.Actividad,
                    NumeroResolucion = a.NumeroResolucion
                }).ToListAsync();
        }


        // ========================
        // Archivos
        // ========================
        public async Task<ResultModel> AgregarArchivo(int asociacionId, AArchivoModel archivo)
        {
            // Normalizar/llenar campos faltantes
            var nombreOriginal = archivo.NombreArchivo ?? "archivo";
            var nombreGuardado = archivo.NombreArchivoGuardado ?? GenerateStoredFileName(nombreOriginal);
            var categoria = string.IsNullOrWhiteSpace(archivo.Categoria) ? "General" : archivo.Categoria;

            var entity = new TbArchivosAsociacion
            {
                AsociacionId = asociacionId,
                Categoria = categoria,
                NombreOriginal = nombreOriginal,
                NombreArchivoGuardado = nombreGuardado,
                Url = archivo.RutaArchivo ?? string.Empty,
                FechaSubida = archivo.SubidoEn,
                Version = archivo.Version != 0 ? archivo.Version : 1,
                IsActivo = archivo.IsActivo 
            };

            _context.TbArchivosAsociacion.Add(entity);
            await _context.SaveChangesAsync();
            return new ResultModel { Success = true, Message = "Archivo agregado correctamente." };
        }

        public async Task<ResultModel> EliminarArchivo(int archivoId)
        {
            var entity = await _context.TbArchivosAsociacion.FindAsync(archivoId);
            if (entity == null) return new ResultModel { Success = false, Message = "Archivo no encontrado." };

            _context.TbArchivosAsociacion.Remove(entity);
            await _context.SaveChangesAsync();
            return new ResultModel { Success = true, Message = "Archivo eliminado correctamente." };
        }

        public async Task<List<AArchivoModel>> ObtenerArchivos(int asociacionId)
        {
            return await _context.TbArchivosAsociacion
                .Where(a => a.AsociacionId == asociacionId)
                .Select(a => new AArchivoModel
                {
                    AsociacionArchivoId = a.ArchivoId,
                    NombreArchivo = a.NombreOriginal,
                    NombreArchivoGuardado = a.NombreArchivoGuardado,
                    RutaArchivo = a.Url,
                    Categoria = a.Categoria,
                    SubidoEn = a.FechaSubida,
                    Version = a.Version,
                    IsActivo = a.IsActivo
                }).ToListAsync();
        }

        // ========================
        // Historial
        // ========================
        public async Task<List<DetalleRegAsociacionModel>> ObtenerDetalleHistorial(int asociacionId)
        {
            return await _context.TbDetalleRegAsociacionHistorial
                .Where(h => h.AsociacionId == asociacionId)
                .Select(h => new DetalleRegAsociacionModel
                {
                    DetalleRegAsociacionId = h.HistorialId,
                    CreadaEn = h.FechaModificacion ?? DateTime.Now,
                    CreadaPor = h.UsuarioId,
                    NumRegAsecuencia = h.FechaResolucion.HasValue ? h.FechaResolucion.Value.DayOfYear : 0,
                    NumRegAcompleta = h.NumeroResolucion ?? string.Empty,
                    Accion = h.Accion,
                    Comentario = h.Comentario
                })
                .ToListAsync();
        }

        // ------------------------
        // Helpers
        // ------------------------
        private static string GenerateStoredFileName(string originalName)
        {
            var safe = Path.GetFileNameWithoutExtension(originalName)
                .Replace(' ', '_')
                .Replace("ñ", "n")
                .Replace("Ñ", "N");
            var ext = Path.GetExtension(originalName);
            return $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}_{safe}{ext}";
        }

    }
}
