using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class RegistroComiteService : IRegistroComiteService
{
    private readonly DbContextLegal _context;
    private readonly ILogger<RegistroComiteService> _logger;

    public RegistroComiteService(DbContextLegal context, ILogger<RegistroComiteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // --- CATÁLOGOS ---
    public async Task<List<RegistroComiteDTO>> GetRegionesAsync()
    {
        return await _context.TbRegionSalud
            .Select(r => new RegistroComiteDTO
            {
                RegionSaludId = r.RegionSaludId,
                NombreRegion = r.NombreRegion
            })
            .ToListAsync();
    }

    public async Task<List<RegistroComiteDTO>> GetProvinciasAsync(int regionId)
    {
        return await _context.TbProvincia
            .Where(p => p.RegionSaludId == regionId)
            .Select(p => new RegistroComiteDTO
            {
                ProvinciaId = p.ProvinciaId,
                NombreProvincia = p.NombreProvincia
            })
            .ToListAsync();
    }

    public async Task<List<RegistroComiteDTO>> GetDistritosAsync(int provinciaId)
    {
        return await _context.TbDistrito
            .Where(d => d.ProvinciaId == provinciaId)
            .Select(d => new RegistroComiteDTO
            {
                DistritoId = d.DistritoId,
                NombreDistrito = d.NombreDistrito
            })
            .ToListAsync();
    }

    public async Task<List<RegistroComiteDTO>> GetCorregimientosAsync(int distritoId)
    {
        return await _context.TbCorregimiento
            .Where(c => c.DistritoId == distritoId)
            .Select(c => new RegistroComiteDTO
            {
                CorregimientoId = c.CorregimientoId,
                CorregimientoNombre = c.NombreCorregimiento
            })
            .ToListAsync();
    }

    public async Task<List<RegistroComiteDTO>> GetTiposTramiteAsync()
    {
        return await _context.TbTipoTramite
            .Where(t => t.IsActivo)
            .Select(t => new RegistroComiteDTO
            {
                TramiteId = t.TramiteId,
                NombreTramite = t.NombreTramite
            })
            .ToListAsync();
    }

    public async Task<List<MiembroComiteDTO>> GetCargosAsync()
    {
        return await _context.TbCargosMiembrosComite
            .Where(c => c.IsActivo)
            .Select(c => new MiembroComiteDTO
            {
                CargoId = c.CargoId,
                NombreCargo = c.NombreCargo
            })
            .ToListAsync();
    }

    // --- CRUD COMITÉ ---
    public async Task<RegistroComiteDTO> GetComitePorIdAsync(int id)
    {
        var comite = await _context.TbDatosComite
            .Include(c => c.RegionSalud)
            .Include(c => c.Provincia)
            .Include(c => c.Distrito)
            .Include(c => c.Corregimiento)
            .FirstOrDefaultAsync(c => c.DcomiteId == id);

        if (comite == null) return null;

        var miembros = await _context.TbDatosMiembros
            .Include(m => m.Cargo)
            .Where(m => m.DcomiteId == id)
            .ToListAsync();

        var detalle = await _context.TbDetalleRegComite
            .Include(d => d.TipoTramite)
            .FirstOrDefaultAsync(d => d.ComiteId == id);

        var archivos = await _context.TbArchivos
            .Where(a => a.DetRegComiteId == id)
            .ToListAsync();

        return new RegistroComiteDTO
        {
            Id = comite.DcomiteId,
            NombreComiteSalud = comite.NombreComiteSalud,
            Comunidad = comite.Comunidad,
            RegionSaludId = comite.RegionSaludId,
            NombreRegion = comite.RegionSalud?.NombreRegion,
            ProvinciaId = comite.ProvinciaId,
            NombreProvincia = comite.Provincia?.NombreProvincia,
            DistritoId = comite.DistritoId,
            NombreDistrito = comite.Distrito?.NombreDistrito,
            CorregimientoId = comite.CorregimientoId,
            CorregimientoNombre = comite.Corregimiento?.NombreCorregimiento,
            TramiteId = detalle?.TipoTramiteId ?? 0,
            NombreTramite = detalle?.TipoTramite?.NombreTramite,
            CreadaEn = detalle?.CreadaEn,
            CreadaPor = detalle?.CreadaPor,
            Miembros = miembros.Select(m => new MiembroComiteDTO
            {
                Nombre = m.NombreMiembro,
                Cedula = m.CedulaMiembro,
                CargoId = m.CargoId
            }).ToList(),
            Archivos = archivos.Select(a => new ArchivoDTO
            {
                Categoria = a.Categoria,
                Archivo = a.NombreOriginal,
                Url = a.Url
            }).ToList(),
            EditMode = true
        };
    }

    public async Task<List<RegistroComiteDTO>> GetAllAsync()
    {
        var comites = await _context.TbDatosComite
            .Include(c => c.RegionSalud)
            .Include(c => c.Provincia)
            .Include(c => c.Distrito)
            .Include(c => c.Corregimiento)
            .Select(c => new RegistroComiteDTO
            {
                Id = c.DcomiteId,
                NombreComiteSalud = c.NombreComiteSalud,
                Comunidad = c.Comunidad,
                NombreRegion = c.RegionSalud.NombreRegion,
                NombreProvincia = c.Provincia.NombreProvincia,
                NombreDistrito = c.Distrito.NombreDistrito,
                CorregimientoNombre = c.Corregimiento.NombreCorregimiento,
                CreadaEn = c.TbDetalleRegComite.FirstOrDefault().CreadaEn
            })
            .ToListAsync();

        return comites;
    }

    public async Task<List<RegistroComiteDTO>> SearchAsync(string? termino = null, int? regionId = null, int? provinciaId = null)
    {
        var query = _context.TbDatosComite
            .Include(c => c.RegionSalud)
            .Include(c => c.Provincia)
            .Include(c => c.Distrito)
            .Include(c => c.Corregimiento)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(termino))
        {
            query = query.Where(c => c.NombreComiteSalud.Contains(termino) || c.Comunidad.Contains(termino));
        }

        if (regionId.HasValue)
        {
            query = query.Where(c => c.RegionSaludId == regionId.Value);
        }

        if (provinciaId.HasValue)
        {
            query = query.Where(c => c.ProvinciaId == provinciaId.Value);
        }

        return await query
            .Select(c => new RegistroComiteDTO
            {
                Id = c.DcomiteId,
                NombreComiteSalud = c.NombreComiteSalud,
                Comunidad = c.Comunidad,
                NombreRegion = c.RegionSalud.NombreRegion,
                NombreProvincia = c.Provincia.NombreProvincia,
                NombreDistrito = c.Distrito.NombreDistrito,
                CorregimientoNombre = c.Corregimiento.NombreCorregimiento,
                CreadaEn = c.TbDetalleRegComite.FirstOrDefault().CreadaEn
            })
            .ToListAsync();
    }

    public async Task<bool> SaveAsync(RegistroComiteDTO dto)
    {
        if (dto.Id == 0)
            return await CreateAsync(dto);
        else
            return await UpdateAsync(dto);
    }

    public async Task<bool> UpdateAsync(RegistroComiteDTO dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var comite = await _context.TbDatosComite.FindAsync(dto.Id);
            if (comite == null) return false;

            comite.NombreComiteSalud = dto.NombreComiteSalud;
            comite.Comunidad = dto.Comunidad;
            comite.RegionSaludId = dto.RegionSaludId;
            comite.ProvinciaId = dto.ProvinciaId;
            comite.DistritoId = dto.DistritoId;
            comite.CorregimientoId = dto.CorregimientoId;

            var detalle = await _context.TbDetalleRegComite.FindAsync(dto.Id);
            if (detalle != null)
            {
                detalle.TipoTramiteId = dto.TramiteId;
            }

            // Miembros
            var miembrosExistentes = await _context.TbDatosMiembros.Where(m => m.DcomiteId == dto.Id).ToListAsync();
            _context.TbDatosMiembros.RemoveRange(miembrosExistentes);

            foreach (var m in dto.Miembros)
            {
                _context.TbDatosMiembros.Add(new TbDatosMiembros
                {
                    NombreMiembro = m.Nombre,
                    CedulaMiembro = m.Cedula,
                    CargoId = m.CargoId,
                    DcomiteId = dto.Id
                });
            }

            // Archivos
            var archivosExistentes = await _context.TbArchivos.Where(a => a.DetRegComiteId == dto.Id).ToListAsync();
            foreach (var a in archivosExistentes)
            {
                var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", a.Url.TrimStart('/'));
                if (File.Exists(ruta)) File.Delete(ruta);
            }
            _context.TbArchivos.RemoveRange(archivosExistentes);

            foreach (var a in dto.Archivos)
            {
                _context.TbArchivos.Add(new TbArchivos
                {
                    DetRegComiteId = dto.Id,
                    Categoria = a.Categoria,
                    NombreOriginal = a.Archivo,
                    Url = a.Url,
                    IsActivo = true,
                    Version = 1
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, $"Error al actualizar comité: {dto.Id}");
            return false;
        }
    }

    private async Task<bool> CreateAsync(RegistroComiteDTO dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var comite = new TbDatosComite
            {
                NombreComiteSalud = dto.NombreComiteSalud,
                Comunidad = dto.Comunidad,
                RegionSaludId = dto.RegionSaludId,
                ProvinciaId = dto.ProvinciaId,
                DistritoId = dto.DistritoId,
                CorregimientoId = dto.CorregimientoId
            };
            _context.TbDatosComite.Add(comite);
            await _context.SaveChangesAsync();

            var secuencia = await _context.TbRegSecuencia.FirstOrDefaultAsync(s => s.Anio == DateTime.Now.Year && s.Activo);
            if (secuencia == null)
            {
                secuencia = new TbRegSecuencia { Anio = DateTime.Now.Year, Numeracion = 0, Activo = true };
                _context.TbRegSecuencia.Add(secuencia);
                await _context.SaveChangesAsync();
            }

            secuencia.Numeracion++;
            await _context.SaveChangesAsync();

            var detalle = new TbDetalleRegComite
            {
                ComiteId = comite.DcomiteId,
                TipoTramiteId = dto.TramiteId,
                CreadaPor = dto.CreadaPor ?? "Sistema",
                NumRegCoSecuencia = secuencia.Numeracion,
                NomRegCoAnio = DateTime.Now.Year,
                NumRegCoMes = DateTime.Now.Month
            };
            _context.TbDetalleRegComite.Add(detalle);

            foreach (var m in dto.Miembros)
            {
                _context.TbDatosMiembros.Add(new TbDatosMiembros
                {
                    NombreMiembro = m.Nombre,
                    CedulaMiembro = m.Cedula,
                    CargoId = m.CargoId,
                    DcomiteId = comite.DcomiteId
                });
            }

            foreach (var a in dto.Archivos)
            {
                _context.TbArchivos.Add(new TbArchivos
                {
                    DetRegComiteId = comite.DcomiteId,
                    Categoria = a.Categoria,
                    NombreOriginal = a.Archivo,
                    Url = a.Url,
                    IsActivo = true,
                    Version = 1
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al crear comité");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var comite = await _context.TbDatosComite.FindAsync(id);
            if (comite == null) return false;

            var detalle = await _context.TbDetalleRegComite.FindAsync(id);
            var miembros = await _context.TbDatosMiembros.Where(m => m.DcomiteId == id).ToListAsync();
            var archivos = await _context.TbArchivos.Where(a => a.DetRegComiteId == id).ToListAsync();

            foreach (var a in archivos)
            {
                var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", a.Url.TrimStart('/'));
                if (File.Exists(ruta)) File.Delete(ruta);
            }

            _context.TbArchivos.RemoveRange(archivos);
            _context.TbDatosMiembros.RemoveRange(miembros);
            if (detalle != null) _context.TbDetalleRegComite.Remove(detalle);
            _context.TbDatosComite.Remove(comite);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, $"Error al eliminar comité: {id}");
            return false;
        }
    }

    public async Task<bool> ExistsByIdAsync(int id)
    {
        return await _context.TbDatosComite.AnyAsync(c => c.DcomiteId == id);
    }

    public async Task<bool> IsCedulaUniqueAsync(string cedula, int? comiteId = null)
    {
        var query = _context.TbDatosMiembros.Where(m => m.CedulaMiembro == cedula);
        if (comiteId.HasValue)
        {
            query = query.Where(m => m.DcomiteId != comiteId);
        }
        return !await query.AnyAsync();
    }
}