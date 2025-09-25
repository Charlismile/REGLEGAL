using BlazorBootstrap;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class RegistroComiteService : IRegistroComite
{
    private readonly IDbContextFactory<DbContextLegal> _context;

    public RegistroComiteService(IDbContextFactory<DbContextLegal> context)
    {
        _context = context;
    }
    private async Task<TbRegSecuencia> ObtenerOSincronizarSecuencia(DbContextLegal context)
    {
        var sec = await context.TbRegSecuencia.FirstOrDefaultAsync(s => s.Activo);
        if (sec != null) return sec;

        var ahora = DateTime.UtcNow;
        var nuevaSecuencia = new TbRegSecuencia
        {
            Anio = ahora.Year,
            Numeracion = 1,
            Activo = true,
            EntidadId = 0
        };
        context.TbRegSecuencia.Add(nuevaSecuencia);
        await context.SaveChangesAsync();
        return nuevaSecuencia;
    }
    
    public async Task<ResultModel> CrearComite(ComiteModel model)
    {
        await using var context = await _context.CreateDbContextAsync();
        var sec = await ObtenerOSincronizarSecuencia(context);
        var ahora = DateTime.UtcNow;
        var numeracion = sec.Anio < ahora.Year ? 1 : sec.Numeracion;
        var anio = sec.Anio < ahora.Year ? ahora.Year : sec.Anio;
        var numCompleto = $"SOL-{anio}-{ahora.Month:00}-{numeracion.ToString().PadLeft(10, '0')}";

        await using var tx = await context.Database.BeginTransactionAsync();
        try
        {
            if (sec.Anio == anio) sec.Numeracion++;
            else
            {
                sec.Anio = anio;
                sec.Numeracion = 2;
            }
            await context.SaveChangesAsync();

            var nuevoComite = new TbDatosComite
            {
                NombreComiteSalud = model.NombreComiteSalud,
                Comunidad = model.Comunidad,
                RegionSaludId = model.RegionSaludId ?? 0,
                ProvinciaId = model.ProvinciaId ?? 0,
                DistritoId = model.DistritoId ?? 0,
                CorregimientoId = model.CorregimientoId ?? 0
            };
            context.TbDatosComite.Add(nuevoComite);
            await context.SaveChangesAsync();

            var sol = new TbDetalleRegComite()
            {
                CreadaEn = ahora,
                CreadaPor = model.CreadaPor,
                ComiteId = nuevoComite.DcomiteId,
                NomRegCoAnio = anio,
                NumRegCoMes = (byte)ahora.Month,
                NumRegCoCompleta = numCompleto,
                TipoTramiteId = (int)model.TipoTramiteEnum
            };
            context.TbDetalleRegComite.Add(sol);
            await context.SaveChangesAsync();

            await tx.CommitAsync();

            return new ResultModel { Success = true, Message = "Comité creado exitosamente" };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new ResultModel { Success = false, Message = $"Error al crear comité: {ex.Message}" };
        }
    }

    public async Task<ResultModel> ActualizarComite(ComiteModel model)
    {
        await using var context = await _context.CreateDbContextAsync();
        var comite = await context.TbDatosComite.FindAsync(model.ComiteId);
        if (comite == null) return new ResultModel { Success = false, Message = "Comité no encontrado" };

        comite.NombreComiteSalud = model.NombreComiteSalud;
        comite.Comunidad = model.Comunidad;
        comite.RegionSaludId = model.RegionSaludId ?? 0;
        comite.ProvinciaId = model.ProvinciaId ?? 0;
        comite.DistritoId = model.DistritoId ?? 0;
        comite.CorregimientoId = model.CorregimientoId ?? 0;

        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Comité actualizado" };
    }

    public async Task<ResultModel> EliminarComite(int comiteId)
    {
        await using var context = await _context.CreateDbContextAsync();
        var comite = await context.TbDatosComite.FindAsync(comiteId);
        if (comite == null) return new ResultModel { Success = false, Message = "Comité no encontrado" };

        context.TbDatosComite.Remove(comite);
        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Comité eliminado" };
    }

    public async Task<List<ComiteModel>> ObtenerTodos()
    {
        await using var context = await _context.CreateDbContextAsync();
        var lista = await context.TbDatosComite
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
        return lista;
    }

    public async Task<ComiteModel?> GetComiteByIdAsync(int comiteId)
    {
        await using var context = await _context.CreateDbContextAsync();
        var comite = await context.TbDatosComite
            .Include(c => c.TbDatosMiembros)
            .FirstOrDefaultAsync(c => c.DcomiteId == comiteId);

        if (comite == null) return null;

        return new ComiteModel
        {
            ComiteId = comite.DcomiteId,
            NombreComiteSalud = comite.NombreComiteSalud,
            Comunidad = comite.Comunidad,
            RegionSaludId = comite.RegionSaludId,
            ProvinciaId = comite.ProvinciaId,
            DistritoId = comite.DistritoId,
            CorregimientoId = comite.CorregimientoId,
            Miembros = comite.TbDatosMiembros.Select(m => new MiembroComiteModel
            {
                MiembroId = m.DmiembroId,
                NombreMiembro = m.NombreMiembro,
                ApellidoMiembro = m.ApellidoMiembro,
                CedulaMiembro = m.CedulaMiembro,
                CargoId = m.CargoId,
                CorreoMiembro = m.CorreoMiembro,
                TelefonoMiembro = m.TelefonoMiembro
            }).ToList()
        };
    }
    
    public async Task<TbDatosMiembros> AgregarMiembro(int comiteId, MiembroComiteModel miembro)
    {
        await using var context = await _context.CreateDbContextAsync();
        var nuevo = new TbDatosMiembros
        {
            NombreMiembro = miembro.NombreMiembro,
            ApellidoMiembro = miembro.ApellidoMiembro,
            CargoId = miembro.CargoId,
            CedulaMiembro = miembro.CedulaMiembro,
            TelefonoMiembro = miembro.TelefonoMiembro,
            CorreoMiembro = miembro.CorreoMiembro,
            DcomiteId = comiteId
        };
        context.TbDatosMiembros.Add(nuevo);
        await context.SaveChangesAsync();
        return nuevo;
    }

    public async Task<ResultModel> ActualizarMiembro(MiembroComiteModel miembro)
    {
        await using var context = await _context.CreateDbContextAsync();
        var m = await context.TbDatosMiembros.FindAsync(miembro.MiembroId);
        if (m == null) return new ResultModel { Success = false, Message = "Miembro no encontrado" };

        m.NombreMiembro = miembro.NombreMiembro;
        m.ApellidoMiembro = miembro.ApellidoMiembro;
        m.CargoId = miembro.CargoId;
        m.CedulaMiembro = miembro.CedulaMiembro;
        m.CorreoMiembro = miembro.CorreoMiembro;
        m.TelefonoMiembro = miembro.TelefonoMiembro;

        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Miembro actualizado" };
    }

    public async Task<ResultModel> EliminarMiembro(int miembroId)
    {
        await using var context = await _context.CreateDbContextAsync();
        var miembro = await context.TbDatosMiembros.FindAsync(miembroId);
        if (miembro == null) return new ResultModel { Success = false, Message = "Miembro no encontrado" };

        context.TbDatosMiembros.Remove(miembro);
        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Miembro eliminado" };
    }

    public async Task<List<MiembroComiteModel>> ObtenerMiembros(int comiteId)
    {
        await using var context = await _context.CreateDbContextAsync();
        return await context.TbDatosMiembros
            .Where(m => m.DcomiteId == comiteId)
            .Select(m => new MiembroComiteModel
            {
                MiembroId = m.DmiembroId,
                NombreMiembro = m.NombreMiembro,
                ApellidoMiembro = m.ApellidoMiembro,
                CargoId = m.CargoId,
                CedulaMiembro = m.CedulaMiembro,
                TelefonoMiembro = m.TelefonoMiembro,
                CorreoMiembro = m.CorreoMiembro
            }).ToListAsync();
    }
    
    public async Task<ResultModel> AgregarArchivo(int comiteId, CArchivoModel archivo)
    {
        await using var context = await _context.CreateDbContextAsync();
        var nuevo = new TbComiteArchivos
        {
            ComiteArchivoId = comiteId, // FK correcto
            NombreOriginal = archivo.NombreArchivo,
            Url = archivo.RutaArchivo,
            FechaSubida = DateTime.UtcNow
        };
        context.TbComiteArchivos.Add(nuevo);
        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Archivo agregado" };
    }

    public async Task<ResultModel> EliminarArchivo(int archivoId)
    {
        await using var context = await _context.CreateDbContextAsync();
        var archivo = await context.TbComiteArchivos.FindAsync(archivoId);
        if (archivo == null) return new ResultModel { Success = false, Message = "Archivo no encontrado" };

        context.TbComiteArchivos.Remove(archivo);
        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Archivo eliminado" };
    }

    public async Task<List<CArchivoModel>> ObtenerArchivos(int comiteId)
    {
        await using var context = await _context.CreateDbContextAsync();
        return await context.TbComiteArchivos
            .Where(a => a.ComiteArchivoId == comiteId)
            .Select(a => new CArchivoModel
            {
                ComiteArchivoId = a.ComiteArchivoId,
                NombreArchivo = a.NombreOriginal,
                RutaArchivo = a.Url,
                SubidoEn = a.FechaSubida
            }).ToListAsync();
    }
    public async Task<List<CargoModel>> ObtenerCargos()
    {
        await using var context = await _context.CreateDbContextAsync();

        var cargos = await context.TbCargosMiembrosComite
            .Where(c => c.IsActivo)
            .Select(c => new CargoModel
            {
                CargoId = c.CargoId,
                NombreCargo = c.NombreCargo
            })
            .ToListAsync();

        return cargos;
    }

    public async Task<List<DetalleRegComiteModel>> ObtenerDetalleHistorial(int comiteId)
    {
        await using var context = await _context.CreateDbContextAsync();
        return await context.TbDetalleRegComite
            .Where(d => d.ComiteId == comiteId)
            .Select(d => new DetalleRegComiteModel
            {
                DetalleRegComiteId = d.DetalleRegComiteId,
                CreadaEn = d.CreadaEn,
                CreadaPor = d.CreadaPor,
                NumRegCoCompleta = d.NumRegCoCompleta,
                TipoTramiteId= d.TipoTramiteId
            }).ToListAsync();
    }
}
