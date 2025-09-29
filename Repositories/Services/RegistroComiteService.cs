using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace REGISTROLEGAL.Repositories.Services;

public class RegistroComiteService : IRegistroComite
{
    private readonly DbContextLegal _context;

    public RegistroComiteService(DbContextLegal context)
    {
        _context = context;
    }

    #region COMITÉ CRUD

    public async Task<ResultModel> CrearComite(ComiteModel model)
    {
        try
        {
            var comite = new TbDatosComite
            {
                NombreComiteSalud = model.NombreComiteSalud,
                Comunidad = model.Comunidad,
                RegionSaludId = model.RegionSaludId,
                ProvinciaId = model.ProvinciaId,
                DistritoId = model.DistritoId,
                CorregimientoId = model.CorregimientoId
            };
            _context.TbDatosComite.Add(comite);
            await _context.SaveChangesAsync();

            if (model.Miembros?.Any() == true)
                foreach (var miembro in model.Miembros)
                    await AgregarMiembro(comite.DcomiteId, miembro);

            return new ResultModel { Success = true, Message = "Comité creado correctamente", Data = comite.DcomiteId };
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
            var entity = await _context.TbDatosComite.FindAsync(model.ComiteId);
            if (entity == null) return new ResultModel { Success = false, Message = "Comité no encontrado" };

            entity.NombreComiteSalud = model.NombreComiteSalud;
            entity.Comunidad = model.Comunidad;
            entity.RegionSaludId = model.RegionSaludId ?? entity.RegionSaludId;
            entity.ProvinciaId = model.ProvinciaId ?? entity.ProvinciaId;
            entity.DistritoId = model.DistritoId ?? entity.DistritoId;
            entity.CorregimientoId = model.CorregimientoId ?? entity.CorregimientoId;

            _context.TbDatosComite.Update(entity);
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
            var entity = await _context.TbDatosComite.FindAsync(comiteId);
            if (entity == null) return new ResultModel { Success = false, Message = "Comité no encontrado" };

            _context.TbDatosComite.Remove(entity);
            await _context.SaveChangesAsync();

            return new ResultModel { Success = true, Message = "Comité eliminado correctamente" };
        }
        catch (Exception ex)
        {
            return new ResultModel { Success = false, Message = ex.Message };
        }
    }

    public async Task<List<ComiteModel>> ObtenerTodos()
    {
        return await _context.TbDatosComite
            .Select(c => new ComiteModel
            {
                ComiteId = c.DcomiteId,
                NombreComiteSalud = c.NombreComiteSalud,
                Comunidad = c.Comunidad,
                RegionSaludId = c.RegionSaludId,
                ProvinciaId = c.ProvinciaId,
                DistritoId = c.DistritoId,
                CorregimientoId = c.CorregimientoId
            }).ToListAsync();
    }

    public async Task<ComiteModel?> ObtenerComiteCompletoAsync(int comiteId)
    {
        var entity = await _context.TbDatosComite
            .Include(c => c.TbDatosMiembros).ThenInclude(m => m.Cargo)
            .FirstOrDefaultAsync(c => c.DcomiteId == comiteId);

        if (entity == null) return null;

        return new ComiteModel
        {
            ComiteId = entity.DcomiteId,
            NombreComiteSalud = entity.NombreComiteSalud,
            Comunidad = entity.Comunidad,
            RegionSaludId = entity.RegionSaludId,
            ProvinciaId = entity.ProvinciaId,
            DistritoId = entity.DistritoId,
            CorregimientoId = entity.CorregimientoId,
            Miembros = entity.TbDatosMiembros
                .Select(m => new MiembroComiteModel
                {
                    MiembroId = m.DmiembroId,
                    ComiteId = m.DcomiteId ?? 0,
                    NombreMiembro = m.NombreMiembro,
                    ApellidoMiembro = m.ApellidoMiembro,
                    CedulaMiembro = m.CedulaMiembro,
                    CargoId = m.CargoId,
                    TelefonoMiembro = m.TelefonoMiembro,
                    CorreoMiembro = m.CorreoMiembro,
                    NombreCargo = m.Cargo.NombreCargo
                }).ToList()
        };
    }

    // 🔹 Nuevo método: último comité con miembros
    public async Task<ComiteModel?> ObtenerUltimoComiteConMiembrosAsync()
    {
        var entity = await _context.TbDatosComite
            .Include(c => c.TbDatosMiembros)
            .ThenInclude(m => m.Cargo)
            .OrderByDescending(c => c.DcomiteId)
            .FirstOrDefaultAsync();

        if (entity == null) return null;

        return new ComiteModel
        {
            ComiteId = entity.DcomiteId,
            NombreComiteSalud = entity.NombreComiteSalud,
            Comunidad = entity.Comunidad,
            RegionSaludId = entity.RegionSaludId,
            ProvinciaId = entity.ProvinciaId,
            DistritoId = entity.DistritoId,
            CorregimientoId = entity.CorregimientoId,
            Miembros = entity.TbDatosMiembros
                .Select(m => new MiembroComiteModel
                {
                    MiembroId = m.DmiembroId,
                    ComiteId = m.DcomiteId ?? 0,
                    NombreMiembro = m.NombreMiembro,
                    ApellidoMiembro = m.ApellidoMiembro,
                    CedulaMiembro = m.CedulaMiembro,
                    CargoId = m.CargoId,
                    TelefonoMiembro = m.TelefonoMiembro,
                    CorreoMiembro = m.CorreoMiembro,
                    NombreCargo = m.Cargo.NombreCargo
                }).ToList()
        };
    }

    #endregion

    #region MIEMBROS

    public async Task<TbDatosMiembros> AgregarMiembro(int comiteId, MiembroComiteModel miembro)
    {
        var entity = new TbDatosMiembros
        {
            DcomiteId = comiteId,
            NombreMiembro = miembro.NombreMiembro,
            ApellidoMiembro = miembro.ApellidoMiembro,
            CedulaMiembro = miembro.CedulaMiembro,
            CargoId = miembro.CargoId,
            TelefonoMiembro = miembro.TelefonoMiembro,
            CorreoMiembro = miembro.CorreoMiembro
        };
        _context.TbDatosMiembros.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ResultModel> ActualizarMiembro(MiembroComiteModel miembro)
    {
        var entity = await _context.TbDatosMiembros.FindAsync(miembro.MiembroId);
        if (entity == null) return new ResultModel { Success = false, Message = "Miembro no encontrado" };

        entity.NombreMiembro = miembro.NombreMiembro;
        entity.ApellidoMiembro = miembro.ApellidoMiembro;
        entity.CedulaMiembro = miembro.CedulaMiembro;
        entity.CargoId = miembro.CargoId;
        entity.TelefonoMiembro = miembro.TelefonoMiembro;
        entity.CorreoMiembro = miembro.CorreoMiembro;

        _context.TbDatosMiembros.Update(entity);
        await _context.SaveChangesAsync();

        return new ResultModel { Success = true, Message = "Miembro actualizado correctamente" };
    }

    public async Task<ResultModel> EliminarMiembro(int miembroId)
    {
        var entity = await _context.TbDatosMiembros.FindAsync(miembroId);
        if (entity == null) return new ResultModel { Success = false, Message = "Miembro no encontrado" };

        _context.TbDatosMiembros.Remove(entity);
        await _context.SaveChangesAsync();

        return new ResultModel { Success = true, Message = "Miembro eliminado correctamente" };
    }

    public async Task<List<MiembroComiteModel>> ObtenerMiembros(int comiteId)
    {
        return await _context.TbDatosMiembros
            .Where(m => m.DcomiteId == comiteId)
            .Select(m => new MiembroComiteModel
            {
                MiembroId = m.DmiembroId,
                ComiteId = m.DcomiteId ?? 0,
                NombreMiembro = m.NombreMiembro,
                ApellidoMiembro = m.ApellidoMiembro,
                CedulaMiembro = m.CedulaMiembro,
                CargoId = m.CargoId,
                TelefonoMiembro = m.TelefonoMiembro,
                CorreoMiembro = m.CorreoMiembro
            }).ToListAsync();
    }

    public async Task<List<CargoModel>> ObtenerCargos()
    {
        return await _context.TbCargosMiembrosComite
            .Where(c => c.IsActivo)
            .Select(c => new CargoModel { CargoId = c.CargoId, NombreCargo = c.NombreCargo })
            .ToListAsync();
    }

    #endregion

    #region ARCHIVOS

    public async Task<ResultModel> AgregarArchivo(int comiteId, CArchivoModel archivo)
    {
        var entity = new TbComiteArchivos
        {
            DetRegComiteId = comiteId,
            Categoria = "General",
            NombreOriginal = archivo.NombreArchivo,
            NombreArchivoGuardado = archivo.RutaArchivo,
            Url = archivo.RutaArchivo,
            FechaSubida = DateTime.Now,
            Version = 1,
            IsActivo = true
        };
        _context.TbComiteArchivos.Add(entity);
        await _context.SaveChangesAsync();

        return new ResultModel { Success = true, Message = "Archivo agregado correctamente" };
    }

    public async Task<ResultModel> EliminarArchivo(int archivoId)
    {
        var entity = await _context.TbComiteArchivos.FindAsync(archivoId);
        if (entity == null) return new ResultModel { Success = false, Message = "Archivo no encontrado" };

        _context.TbComiteArchivos.Remove(entity);
        await _context.SaveChangesAsync();

        return new ResultModel { Success = true, Message = "Archivo eliminado correctamente" };
    }

    public async Task<List<CArchivoModel>> ObtenerArchivos(int comiteId)
    {
        return await _context.TbComiteArchivos
            .Where(a => a.DetRegComiteId == comiteId && a.IsActivo)
            .Select(a => new CArchivoModel
            {
                ComiteArchivoId = a.ComiteArchivoId,
                ComiteId = a.DetRegComiteId,
                NombreArchivo = a.NombreOriginal,
                RutaArchivo = a.Url,
                SubidoEn = a.FechaSubida
            }).ToListAsync();
    }
    public async Task<ResultModel> GuardarResolucionAsync(int comiteId, IBrowserFile archivo)
    {
        try
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resoluciones");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(archivo.Name)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.OpenReadStream(10 * 1024 * 1024).CopyToAsync(stream); // máx 10MB
            }

            var archivoModel = new CArchivoModel
            {
                ComiteId = comiteId,
                NombreArchivo = archivo.Name,
                RutaArchivo = $"/uploads/resoluciones/{uniqueFileName}",
                SubidoEn = DateTime.Now
            };

            return await AgregarArchivo(comiteId, archivoModel);
        }
        catch (Exception ex)
        {
            return new ResultModel { Success = false, Message = $"Error al guardar resolución: {ex.Message}" };
        }
    }


    #endregion

    #region HISTORIAL

    public async Task<List<DetalleRegComiteModel>> ObtenerDetalleHistorial(int comiteId)
    {
        return await _context.TbDetalleRegComiteHistorial
            .Where(h => h.ComiteId == comiteId)
            .Include(h => h.CoEstadoSolicitud)
            .Select(h => new DetalleRegComiteModel
            {
                DetalleRegComiteId = h.RegComiteSolId,
                CreadaEn = h.FechaCambioCo,
                CreadaPor = h.UsuarioRevisorCo,
                NumRegCoCompleta = h.Comite.NumRegCoCompleta,
                TipoTramiteId = h.Comite.TipoTramiteId
            }).ToListAsync();
    }

    public async Task GuardarHistorialMiembros(int comiteId, List<MiembroComiteModel> miembros)
    {
        if (miembros == null || miembros.Count == 0)
            return;

        foreach (var miembro in miembros)
        {
            var historial = new TbDatosMiembrosHistorial
            {
                DcomiteId = comiteId,
                NombreMiembro = miembro.NombreMiembro,
                ApellidoMiembro = miembro.ApellidoMiembro,
                CedulaMiembro = miembro.CedulaMiembro,
                CargoId = miembro.CargoId,
                TelefonoMiembro = miembro.TelefonoMiembro,
                CorreoMiembro = miembro.CorreoMiembro,
                FechaCambio = DateTime.Now
            };

            _context.TbDatosMiembrosHistorial.Add(historial);
        }

        await _context.SaveChangesAsync();
    }

    #endregion
}
