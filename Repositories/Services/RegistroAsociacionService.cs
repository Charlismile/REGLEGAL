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

    // Crear nueva asociación
    public async Task<ResultModel> CrearAsociacion(AsociacionModel model)
    {
        try
        {
            var entity = new TbAsociacion
            {
                NombreAsociacion = model.NombreAsociacion,
                RepresentanteLegal = model.NombreRepLegal != null ? new TbRepresentanteLegal { NombreRepLegal = model.NombreRepLegal } : null,
                ApoderadoLegal = model.NombreApoAbogado != null ? new TbApoderadoLegal { NombreApoAbogado = model.NombreApoAbogado } : null,
                FechaResolucion = DateTime.Now
            };

            _context.TbAsociacion.Add(entity);
            await _context.SaveChangesAsync();

            return new ResultModel { Success = true, Message = "Asociación creada correctamente." };
        }
        catch (Exception ex)
        {
            return new ResultModel { Success = false, Message = ex.Message };
        }
    }

    // Obtener todas las asociaciones
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

    // Obtener por Id
    public async Task<AsociacionModel?> ObtenerPorId(int id)
    {
        var a = await _context.TbAsociacion
            .Include(a => a.RepresentanteLegal)
            .Include(a => a.ApoderadoLegal)
            .FirstOrDefaultAsync(a => a.AsociacionId == id);

        if (a == null) return null;

        return new AsociacionModel
        {
            AsociacionId = a.AsociacionId,
            NombreAsociacion = a.NombreAsociacion,
            NombreRepLegal = a.RepresentanteLegal != null ? a.RepresentanteLegal.NombreRepLegal : null,
            NombreApoAbogado = a.ApoderadoLegal != null ? a.ApoderadoLegal.NombreApoAbogado : null,
            FechaResolucion = a.FechaResolucion
        };
    }

    // Actualizar asociación
    public async Task<ResultModel> ActualizarAsociacion(AsociacionModel model)
    {
        try
        {
            var entity = await _context.TbAsociacion
                .Include(a => a.RepresentanteLegal)
                .Include(a => a.ApoderadoLegal)
                .FirstOrDefaultAsync(a => a.AsociacionId == model.AsociacionId);

            if (entity == null)
                return new ResultModel { Success = false, Message = "Asociación no encontrada." };

            entity.NombreAsociacion = model.NombreAsociacion;

            if (entity.RepresentanteLegal != null)
                entity.RepresentanteLegal.NombreRepLegal = model.NombreRepLegal;
            else if (model.NombreRepLegal != null)
                entity.RepresentanteLegal = new TbRepresentanteLegal { NombreRepLegal = model.NombreRepLegal };

            if (entity.ApoderadoLegal != null)
                entity.ApoderadoLegal.NombreApoAbogado = model.NombreApoAbogado;
            else if (model.NombreApoAbogado != null)
                entity.ApoderadoLegal = new TbApoderadoLegal { NombreApoAbogado = model.NombreApoAbogado };

            await _context.SaveChangesAsync();
            return new ResultModel { Success = true, Message = "Asociación actualizada correctamente." };
        }
        catch (Exception ex)
        {
            return new ResultModel { Success = false, Message = ex.Message };
        }
    }

    // Eliminar asociación
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

    // Obtener historial
    public async Task<List<DetalleRegAsociacionModel>> ObtenerDetalleHistorial(int asociacionId)
    {
        return await _context.TbDetalleRegAsociacionHistorial
            .Where(h => h.AsociacionId == asociacionId)
            .Select(h => new DetalleRegAsociacionModel
            {
                DetalleRegAsociacionId = h.HistorialId,
                CreadaEn = h.FechaModificacion,        // si quieres, usa la fecha de modificación
                CreadaPor = h.UsuarioId.ToString(),   // si quieres convertir Id a string
                NumRegAsecuencia = h.FechaResolucion.GetHashCode(),
                NumRegAcompleta = h.FechaModificacion.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToListAsync();
    }

}
