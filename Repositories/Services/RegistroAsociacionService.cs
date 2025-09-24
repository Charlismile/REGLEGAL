using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class RegistroAsociacionService : IRegistroAsociacion
{
    private readonly IDbContextFactory<DbContextLegal> _context;

    public RegistroAsociacionService(IDbContextFactory<DbContextLegal> context)
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

    public async Task<ResultModel> CrearAsociacion(AsociacionModel model)
    {
        await using var context = await _context.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            // Obtener o sincronizar la secuencia
            var sec = await ObtenerOSincronizarSecuencia(context);
            var ahora = DateTime.UtcNow;

            var numeracion = sec.Anio < ahora.Year ? 1 : sec.Numeracion;
            var anio = sec.Anio < ahora.Year ? ahora.Year : sec.Anio;

            var numCompleto = $"ASC-{anio}-{ahora.Month:00}-{numeracion:0000000000}";

            // Actualizar secuencia
            if (sec.Anio == anio)
                sec.Numeracion++;
            else
            {
                sec.Anio = anio;
                sec.Numeracion = 2;
            }

            // Crear Asociación junto con sus relaciones
            var nuevaAsociacion = new TbAsociacion
            {
                NombreAsociacion = model.NombreAsociacion,
                Actividad = model.Actividad,
                Folio = model.Folio,
                RepresentanteLegal = new TbRepresentanteLegal
                {
                    NombreRepLegal = model.NombreRepLegal,
                    ApellidoRepLegal = model.ApellidoRepLegal,
                    CedulaRepLegal = model.CedulaRepLegal,
                    CargoRepLegal = model.CargoRepLegal,
                    TelefonoRepLegal = model.TelefonoRepLegal,
                    DireccionRepLegal = model.DireccionRepLegal
                },
                ApoderadoLegal = new TbApoderadoLegal
                {
                    NombreApoAbogado = model.NombreApoAbogado,
                    ApellidoApoAbogado = model.ApellidoApoAbogado,
                    CedulaApoAbogado = model.CedulaApoAbogado,
                    TelefonoApoAbogado = model.TelefonoApoAbogado,
                    CorreoApoAbogado = model.CorreoApoAbogado,
                    DireccionApoAbogado = model.DireccionApoAbogado,
                    ApoderadoFirma = model.PerteneceAFirma
                        ? new TbApoderadoFirma
                        {
                            NombreFirma = model.NombreFirma,
                            CorreoFirma = model.CorreoFirma,
                            TelefonoFirma = model.TelefonoFirma,
                            DireccionFirma = model.DireccionFirma
                        }
                        : null
                },
                TbDetalleRegAsociacion = new List<TbDetalleRegAsociacion>
                {
                    new TbDetalleRegAsociacion
                    {
                        CreadaEn = ahora,
                        CreadaPor = model.CreadaPor,
                        NumRegAsecuencia = numeracion,
                        NomRegAanio = anio,
                        NumRegAmes = (byte)ahora.Month,
                        NumRegAcompleta = numCompleto
                    }
                }
            };

            // Agregar la asociación completa a la base de datos
            context.TbAsociacion.Add(nuevaAsociacion);

            // Guardar todos los cambios en una sola operación
            await context.SaveChangesAsync();
            await tx.CommitAsync();

            return new ResultModel
            {
                Success = true,
                Message = "Asociación registrada exitosamente.",
                AsociacionId = nuevaAsociacion.AsociacionId
            };

        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new ResultModel
            {
                Success = false,
                Message = $"Error al registrar la asociación: {ex.Message}"
            };
        }
    }
}