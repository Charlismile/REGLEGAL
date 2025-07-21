using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using SISTEMALEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class RegistroComiteService : IRegistroComiteService
{
    private readonly DbContextLegal _context;

    public RegistroComiteService(DbContextLegal context)
    {
        _context = context;
    }

    public async Task<List<TbRegionSalud>> GetRegionesAsync() =>
        await _context.TbRegionSalud.ToListAsync();

    public async Task<List<TbProvincia>> GetProvinciasAsync(int regionId) =>
        await _context.TbProvincia
            .Where(p => p.RegionSaludId == regionId)
            .ToListAsync();

    public async Task<List<TbDistrito>> GetDistritosAsync(int provinciaId) =>
        await _context.TbDistrito
            .Where(d => d.ProvinciaId == provinciaId)
            .ToListAsync();

    public async Task<List<TbCorregimiento>> GetCorregimientosAsync(int distritoId) =>
        await _context.TbCorregimiento
            .Where(c => c.DistritoId == distritoId)
            .ToListAsync();

    public async Task<List<TbTipoTramite>> GetTramitesAsync() =>
        await _context.TbTipoTramite
            .Where(t => t.IsActivo)
            .ToListAsync();

    public async Task<List<TbCargosMiembrosComite>> GetCargosAsync() =>
        await _context.TbCargosMiembrosComite
            .Where(c => c.IsActivo)
            .ToListAsync();

    public async Task<RegistroComiteDTO> GetComitePorIdAsync(int id)
    {
        var comite = await _context.TbDatosComite
            .Include(c => c.Corregimiento)
                .ThenInclude(cor => cor.Distrito)
                    .ThenInclude(dist => dist.Provincia)
                        .ThenInclude(prov => prov.RegionSalud)
            .Include(c => c.Miembro)
            .Include(c => c.Distrito)
            .Include(c => c.Provincia)
            .Include(c => c.RegionSalud)
            .FirstOrDefaultAsync(c => c.DcomiteId == id);

        if (comite == null) return null;

        var detalle = await _context.TbDetalleRegComite
            .Include(d => d.TipoTramite)
            .FirstOrDefaultAsync(d => d.ComiteId == id);

        var archivos = await _context.TbArchivos
            .Where(a => a.DetRegComiteId == id)
            .Select(a => new ArchivoDTO
            {
                NombreOriginal = a.NombreOriginal,
                Ruta = a.Url,
                Categoria = a.Categoria
            }).ToListAsync();

        return new RegistroComiteDTO
        {
            Id = comite.DcomiteId,
            NombreComiteSalud = comite.NombreComiteSalud,
            Comunidad = comite.Comunidad,

            RegionSaludId = comite.RegionSaludId,
            NombreRegion = comite.RegionSalud?.NombreRegion,

            ProvinciaId = comite.ProvinciaId,
            NombreProvicnicia = comite.Provincia?.NombreProvincia,

            DistritoId = comite.DistritoId,
            NombreDistrito = comite.Distrito?.NombreDistrito,

            CorregimientoId = comite.CorregimientoId,
            CorregimientoNombre = comite.Corregimiento?.NombreCorregimiento,

            NombreMiembro = comite.Miembro?.NombreMiembro,
            CedulaMiembro = comite.Miembro?.CedulaMiembro,
            CargoId = comite.Miembro?.CargoId ?? 0,

            TramiteId = detalle?.TipoTramiteId ?? 0,
            NombreTramite = detalle?.TipoTramite?.NombreTramite,

            CreadaEn = detalle?.CreadaEn,
            CreadaPor = detalle?.CreadaPor,

            Archivos = archivos,

            EditMode = true
        };
    }

    public async Task<bool> GuardarComiteAsync(RegistroComiteDTO model)
    {
        try
        {
            int comiteId;

            if (model.EditMode)
            {
                await ActualizarComite(model);
                comiteId = model.Id;
            }
            else
            {
                comiteId = await CrearComite(model);
            }

            // ✅ Guardar archivos después de tener el comiteId
            if (model.Archivos?.Count > 0)
            {
                await GuardarArchivos(comiteId, model.Archivos);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // Aquí puedes loguear ex.Message o usar ILogger
            return false;
        }
    }

    private async Task<int> CrearComite(RegistroComiteDTO model)
    {
        var miembro = new TbDatosMiembros
        {
            NombreMiembro = model.NombreMiembro,
            CedulaMiembro = model.CedulaMiembro,
            CargoId = model.CargoId
        };

        _context.TbDatosMiembros.Add(miembro);
        await _context.SaveChangesAsync();

        var comite = new TbDatosComite
        {
            NombreComiteSalud = model.NombreComiteSalud,
            Comunidad = model.Comunidad,
            RegionSaludId = model.RegionSaludId,
            ProvinciaId = model.ProvinciaId,
            DistritoId = model.DistritoId,
            CorregimientoId = model.CorregimientoId,
            MiembroId = miembro.DmiembroId
        };

        _context.TbDatosComite.Add(comite);
        await _context.SaveChangesAsync();

        var detalle = new TbDetalleRegComite
        {
            ComiteId = comite.DcomiteId,
            TipoTramiteId = model.TramiteId,
            CreadaEn = model.CreadaEn ?? DateTime.Now,
            CreadaPor = model.CreadaPor
        };

        _context.TbDetalleRegComite.Add(detalle);
        await _context.SaveChangesAsync();

        return comite.DcomiteId; // Retornamos el ID para usarlo con los archivos
    }

    private async Task ActualizarComite(RegistroComiteDTO model)
    {
        var comite = await _context.TbDatosComite
            .Include(c => c.Miembro)
            .FirstOrDefaultAsync(c => c.DcomiteId == model.Id);

        if (comite == null) return;

        // Actualizar datos del comité
        comite.NombreComiteSalud = model.NombreComiteSalud;
        comite.Comunidad = model.Comunidad;
        comite.RegionSaludId = model.RegionSaludId;
        comite.ProvinciaId = model.ProvinciaId;
        comite.DistritoId = model.DistritoId;
        comite.CorregimientoId = model.CorregimientoId;

        // Actualizar miembro
        if (comite.Miembro != null)
        {
            comite.Miembro.NombreMiembro = model.NombreMiembro;
            comite.Miembro.CedulaMiembro = model.CedulaMiembro;
            comite.Miembro.CargoId = model.CargoId;
        }

        _context.TbDatosComite.Update(comite);
        await _context.SaveChangesAsync();

        // Actualizar o crear detalle del trámite
        var detalle = await _context.TbDetalleRegComite
            .FirstOrDefaultAsync(d => d.ComiteId == model.Id);

        if (detalle != null)
        {
            detalle.TipoTramiteId = model.TramiteId;
            detalle.CreadaEn = model.CreadaEn;
            detalle.CreadaPor = model.CreadaPor;
            _context.TbDetalleRegComite.Update(detalle);
        }
        else
        {
            _context.TbDetalleRegComite.Add(new TbDetalleRegComite
            {
                ComiteId = model.Id,
                TipoTramiteId = model.TramiteId,
                CreadaEn = model.CreadaEn ?? DateTime.Now,
                CreadaPor = model.CreadaPor
            });
        }
    }

    private async Task GuardarArchivos(int comiteId, List<ArchivoDTO> archivos)
    {
        foreach (var archivo in archivos)
        {
            var entidad = new TbArchivos
            {
                DetRegComiteId = comiteId,
                Categoria = archivo.Categoria,
                NombreOriginal = archivo.NombreOriginal,
                NombreArchivoGuardado = Path.GetFileName(archivo.Ruta),
                Url = archivo.Ruta,
                FechaSubida = DateTime.Now,
                Version = 1,
                IsActivo = true
            };

            _context.TbArchivos.Add(entidad);
        }
    }
}