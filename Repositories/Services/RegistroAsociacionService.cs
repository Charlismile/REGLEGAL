using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

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
                NombreAsociacion = model.NombreAsociacion,
                FechaResolucion = model.FechaResolucion
            };

            // Representante y Apoderado
            if (!string.IsNullOrEmpty(model.NombreRepLegal))
                entity.RepresentanteLegal = new TbRepresentanteLegal { NombreRepLegal = model.NombreRepLegal };

            if (!string.IsNullOrEmpty(model.NombreApoAbogado))
                entity.ApoderadoLegal = new TbApoderadoLegal { NombreApoAbogado = model.NombreApoAbogado };

            _context.TbAsociacion.Add(entity);
            await _context.SaveChangesAsync();
            
            // Guardar archivos si existen
            if (model.Archivos != null && model.Archivos.Any())
            {
                foreach (var archivo in model.Archivos)
                    await AgregarArchivo(entity.AsociacionId, archivo);
            }

            return new ResultModel { Success = true, Message = "Asociación creada correctamente." };
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

            entity.NombreAsociacion = model.NombreAsociacion;
            entity.FechaResolucion = model.FechaResolucion;

            // Actualizar representante
            if (entity.RepresentanteLegal != null)
                entity.RepresentanteLegal.NombreRepLegal = model.NombreRepLegal;
            else if (!string.IsNullOrEmpty(model.NombreRepLegal))
                entity.RepresentanteLegal = new TbRepresentanteLegal { NombreRepLegal = model.NombreRepLegal };

            // Actualizar apoderado
            if (entity.ApoderadoLegal != null)
                entity.ApoderadoLegal.NombreApoAbogado = model.NombreApoAbogado;
            else if (!string.IsNullOrEmpty(model.NombreApoAbogado))
                entity.ApoderadoLegal = new TbApoderadoLegal { NombreApoAbogado = model.NombreApoAbogado };

            await _context.SaveChangesAsync();

            // Actualizar archivos
            if (model.Archivos != null)
            {
                foreach (var archivo in model.Archivos)
                {
                    if (archivo.AsociacionArchivoId == 0)
                        await AgregarArchivo(entity.AsociacionId, archivo);
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
            NombreApoAbogado = a.ApoderadoLegal?.NombreApoAbogado,
            FechaResolucion = a.FechaResolucion,
            Archivos = a.TbArchivosAsociacion.Select(f => new AArchivoModel
            {
                AsociacionArchivoId = f.ArchivoId,
                NombreArchivo = f.NombreOriginal,
                RutaArchivo = f.Url
            }).ToList()
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
                NombreApoAbogado = a.ApoderadoLegal != null ? a.ApoderadoLegal.NombreApoAbogado : null,
                FechaResolucion = a.FechaResolucion
            }).ToListAsync();
    }
    

    // ========================
    // Archivos
    // ========================
    public async Task<ResultModel> AgregarArchivo(int asociacionId, AArchivoModel archivo)
    {
        var entity = new TbArchivosAsociacion
        {
            AsociacionId = asociacionId,
            NombreOriginal = archivo.NombreArchivo,
            Url = archivo.RutaArchivo
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
                RutaArchivo = a.Url
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
                CreadaEn = h.FechaModificacion,
                CreadaPor = h.UsuarioId.ToString(),
                NumRegAsecuencia = h.FechaResolucion.GetHashCode(),
                NumRegAcompleta = h.FechaModificacion.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToListAsync();
    }
}
