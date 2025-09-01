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

        if (sec != null)
            return sec;

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
    
    public async Task<TbDatosMiembros> AgregarMiembro(int comiteId, MiembroComiteModel MModel)
    {
        await using var context = await _context.CreateDbContextAsync();
        var nuevoMiembro = new TbDatosMiembros
        {
            NombreMiembro = MModel.NombreMiembro,
            ApellidoMiembro = MModel.ApellidoMiembro,
            CargoId = MModel.CargoId,
            CedulaMiembro = MModel.CedulaMiembro,
            TelefonoMiembro = MModel.TelefonoMiembro,
            CorreoMiembro = MModel.CorreoMiembro,
            DcomiteId = comiteId
        };

        context.TbDatosMiembros.Add(nuevoMiembro);
        await context.SaveChangesAsync();

        return nuevoMiembro;
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
            // actualizar secuencia
            if (sec.Anio == anio) sec.Numeracion++;
            else
            {
                sec.Anio = anio;
                sec.Numeracion = 2;
            }

            await context.SaveChangesAsync();

            // crear comite
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
            
            // crear detalleregcomite
            var sol = new TbDetalleRegComite()
            {
                CreadaEn = ahora,
                CreadaPor = model.CreadaPor,
                ComiteId = nuevoComite.DcomiteId,
                NomRegCoAnio = anio,
                NumRegCoMes = (byte)ahora.Month,
                NumRegCoCompleta = numCompleto,
                TipoTramiteId = model.TipoTramiteEnum == TipoTramite.Personeria ? 1 :
                    model.TipoTramiteEnum == TipoTramite.CambioDirectiva ? 2 : 3
            };
            context.TbDetalleRegComite.Add(sol);
            await context.SaveChangesAsync();

            await tx.CommitAsync();

            return new ResultModel
            {
                Success = true,
                Message = "Comité creado exitosamente"
            };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new ResultModel
            {
                Success = false,
                Message = $"Error al crear solicitud: {ex.Message}"
            };
        }
    }
    
    // Implementación
    public async Task<ComiteModel?> GetComiteByIdAsync(int comiteId)
    {
        await using var context = await _context.CreateDbContextAsync();
    
        var comite = await context.TbDatosComite
            .Include(c => c.TbDatosMiembros) // Asegúrate de la relación
            .FirstOrDefaultAsync(c => c.DcomiteId == comiteId);

        if (comite == null) return null;

        return new ComiteModel
        {
            NombreComiteSalud = comite.NombreComiteSalud,
            Comunidad = comite.Comunidad,
            RegionSaludId = comite.RegionSaludId,
            ProvinciaId = comite.ProvinciaId,
            DistritoId = comite.DistritoId,
            CorregimientoId = comite.CorregimientoId,
            Miembros = comite.TbDatosMiembros.Select(m => new MiembroComiteModel()
            {
                MiembroId = m.DmiembroId,
                NombreMiembro = m.NombreMiembro,
                ApellidoMiembro = m.ApellidoMiembro,
                CedulaMiembro = m.CedulaMiembro,
                CargoId = m.CargoId
            }).ToList()
        };
    }

}