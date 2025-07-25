using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class RegistroService : IRegistroService
{
    private readonly DbContextLegal _context;
    private readonly string _basePath;
    public RegistroService(DbContextLegal context, IWebHostEnvironment env)
    {
        _context = context;
        _basePath = Path.Combine(env.WebRootPath, "uploads", "documentos-legales");
        Directory.CreateDirectory(_basePath);
    }

    #region Numeracion
    public async Task<string> GenerarNumeroAsociacion()
    {
        return await GenerarNumero(1); // Ajusta según tu lógica de EntidadId
    }

    public async Task<string> GenerarNumeroComite()
    {
        return await GenerarNumero(2);
    }

    private async Task<string> GenerarNumero(int entidadId)
    {
        var anio = DateTime.Now.Year;
        var mes = DateTime.Now.Month;

        var secuencia = await _context.TbRegSecuencia
            .FirstOrDefaultAsync(s => s.EntidadId == entidadId && s.Anio == anio && s.Activo);

        if (secuencia == null)
        {
            secuencia = new TbRegSecuencia
            {
                EntidadId = entidadId,
                Anio = anio,
                Numeracion = 1,
                Activo = true
            };
            _context.TbRegSecuencia.Add(secuencia);
        }
        else
        {
            secuencia.Numeracion++;
        }

        await _context.SaveChangesAsync();
        return $"{secuencia.Numeracion}/{anio}/{mes:D2}";
    }
    
    #endregion
    
    #region Comites

    public async Task<List<RegistroDto>> ObtenerComiteAsync()
        {
            var comites = await _context.TbDatosComite.ToListAsync();
            var lista = new List<RegistroDto>();

            foreach (var comite in comites)
            {
                lista.Add(new RegistroDto
                {
                    NombreComiteSalud = comite.NombreComiteSalud,
                    Comunidad = comite.Comunidad,
                    RegionSaludId = comite.RegionSaludId,
                    ProvinciaId = comite.ProvinciaId,
                    DistritoId = comite.DistritoId,
                    CorregimientoId = comite.CorregimientoId
                });
            }

            return lista;
        }

        public async Task<RegistroDto> ObtenerComitePorIdAsync(int id)
        {
            var comite = await _context.TbDatosComite.FindAsync(id);
            if (comite == null) return null;

            return new RegistroDto
            {
                NombreComiteSalud = comite.NombreComiteSalud,
                Comunidad = comite.Comunidad,
                RegionSaludId = comite.RegionSaludId,
                ProvinciaId = comite.ProvinciaId,
                DistritoId = comite.DistritoId,
                CorregimientoId = comite.CorregimientoId
            };
        }

        public async Task<bool> CrearComiteAsync(RegistroDto dto)
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
            return true;
        }

        public async Task<bool> ActualizarComiteAsync(int id, RegistroDto dto)
        {
            var comite = await _context.TbDatosComite.FindAsync(id);
            if (comite == null) return false;

            comite.NombreComiteSalud = dto.NombreComiteSalud;
            comite.Comunidad = dto.Comunidad;
            comite.RegionSaludId = dto.RegionSaludId;
            comite.ProvinciaId = dto.ProvinciaId;
            comite.DistritoId = dto.DistritoId;
            comite.CorregimientoId = dto.CorregimientoId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarComiteAsync(int id)
        {
            var comite = await _context.TbDatosComite.FindAsync(id);
            if (comite == null) return false;

            _context.TbDatosComite.Remove(comite);
            await _context.SaveChangesAsync();
            return true;
        }
        
        // Services/Implementations/RegistroService.cs
        public async Task<List<TbTipoTramite>> ObtenerTipoTramitesAsync()
        {
            return await _context.TbTipoTramite.Where(t => t.IsActivo).ToListAsync();
        }

        public async Task<List<TbCargosMiembrosComite>> ObtenerCargosMiembrosAsync()
        {
            return await _context.TbCargosMiembrosComite.Where(c => c.IsActivo).ToListAsync();
        }

    #endregion

    #region Asociaciones

    public async Task<List<RegistroDto>> ObtenerAsociacionAsync()
        {
            var asociaciones = await _context.TbAsociacion.ToListAsync();
            var lista = new List<RegistroDto>();

            foreach (var a in asociaciones)
            {
                lista.Add(new RegistroDto
                {
                    NombreAsociacion = a.NombreAsociacion,
                    Folio = a.Folio,
                    Actividad = a.Actividad
                });
            }

            return lista;
        }

        public async Task<RegistroDto> ObtenerAsociacionPorIdAsync(int id)
        {
            var a = await _context.TbAsociacion.FindAsync(id);
            if (a == null) return null;

            return new RegistroDto
            {
                NombreAsociacion = a.NombreAsociacion,
                Folio = a.Folio,
                Actividad = a.Actividad
            };
        }

        public async Task<bool> CrearAsociacionAsync(RegistroDto dto)
        {
            var asociacion = new TbAsociacion
            {
                NombreAsociacion = dto.NombreAsociacion,
                Folio = dto.Folio,
                Actividad = dto.Actividad
            };

            _context.TbAsociacion.Add(asociacion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarAsociacionAsync(int id, RegistroDto dto)
        {
            var a = await _context.TbAsociacion.FindAsync(id);
            if (a == null) return false;

            a.NombreAsociacion = dto.NombreAsociacion;
            a.Folio = dto.Folio;
            a.Actividad = dto.Actividad;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsociacionAsync(int id)
        {
            var a = await _context.TbAsociacion.FindAsync(id);
            if (a == null) return false;

            _context.TbAsociacion.Remove(a);
            await _context.SaveChangesAsync();
            return true;
        }

    #endregion

    #region FirmaAbogados
    public Task<List<TbApoderadoFirma>> ObtenerFirmasAsync() => 
        _context.TbApoderadoFirma.OrderBy(f => f.NombreFirma).ToListAsync();

    public Task<TbApoderadoFirma?> ObtenerFirmaPorIdAsync(int firmaId) => 
        _context.TbApoderadoFirma.FindAsync(firmaId).AsTask();
    

    #endregion

    #region Ubicacion

    public Task<List<TbRegionSalud>> ObtenerRegionesAsync() => 
        _context.TbRegionSalud.OrderBy(r => r.NombreRegion).ToListAsync();

    public Task<List<TbProvincia>> ObtenerProvinciasPorRegionAsync(int regionId) => 
        _context.TbProvincia.Where(p => p.RegionSaludId == regionId).OrderBy(p => p.NombreProvincia).ToListAsync();

    public Task<List<TbDistrito>> ObtenerDistritosPorProvinciaAsync(int provinciaId) => 
        _context.TbDistrito.Where(d => d.ProvinciaId == provinciaId).OrderBy(d => d.NombreDistrito).ToListAsync();

    public Task<List<TbCorregimiento>> ObtenerCorregimientosPorDistritoAsync(int distritoId) => 
        _context.TbCorregimiento.Where(c => c.DistritoId == distritoId).OrderBy(c => c.NombreCorregimiento).ToListAsync();

    #endregion

    #region Documentos
    public async Task<List<string>> GuardarArchivosAsync(List<IBrowserFile> archivos, string categoria, int? detAsociacionId = null, int? detComiteId = null)
    {
        var urls = new List<string>();

        foreach (var archivo in archivos)
        {
            var fileName = $"{Guid.NewGuid()}_{archivo.Name}";
            var filePath = Path.Combine(_basePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.OpenReadStream().CopyToAsync(stream);
            }

            var documento = new TbArchivos
            {
                Categoria = categoria,
                NombreOriginal = archivo.Name,
                NombreArchivoGuardado = fileName,
                Url = $"/uploads/documentos-legales/{fileName}",
                FechaSubida = DateTime.Now,
                Version = 1,
                IsActivo = true,
                DetRegAsociacionId = detAsociacionId,
                DetRegComiteId = detComiteId
            };

            _context.TbArchivos.Add(documento);
            urls.Add(documento.Url);
        }

        await _context.SaveChangesAsync();
        return urls;
    }

    public async Task<List<TbArchivos>> ObtenerDocumentosPorAsociacionAsync(int asociacionId)
    {
        return await _context.TbArchivos
            .Where(a => a.DetRegAsociacionId == asociacionId)
            .ToListAsync();
    }

    public async Task<List<TbArchivos>> ObtenerDocumentosPorComiteAsync(int comiteId)
    {
        return await _context.TbArchivos
            .Where(a => a.DetRegComiteId == comiteId)
            .ToListAsync();
    }
    

    #endregion
}