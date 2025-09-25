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

    #region Secuencia

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

    #endregion

    #region CRUD Asociación

    public async Task<ResultModel> CrearAsociacion(AsociacionModel model)
    {
        await using var context = await _context.CreateDbContextAsync();
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            var sec = await ObtenerOSincronizarSecuencia(context);
            var ahora = DateTime.UtcNow;
            var numeracion = sec.Anio < ahora.Year ? 1 : sec.Numeracion;
            var anio = sec.Anio < ahora.Year ? ahora.Year : sec.Anio;
            var numCompleto = $"ASC-{anio}-{ahora.Month:00}-{numeracion:0000000000}";

            // Actualizar secuencia
            if (sec.Anio == anio) sec.Numeracion++;
            else
            {
                sec.Anio = anio;
                sec.Numeracion = 2;
            }

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

            context.TbAsociacion.Add(nuevaAsociacion);
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
            return new ResultModel { Success = false, Message = $"Error al registrar la asociación: {ex.Message}" };
        }
    }

    public async Task<List<AsociacionModel>> ObtenerTodas()
    {
        await using var context = await _context.CreateDbContextAsync();
        return await context.TbAsociacion
            .Include(a => a.RepresentanteLegal)
            .Include(a => a.ApoderadoLegal)
            .Select(a => new AsociacionModel
            {
                AsociacionId = a.AsociacionId,
                NombreAsociacion = a.NombreAsociacion,
                Actividad = a.Actividad,
                Folio = a.Folio,
                NombreRepLegal = a.RepresentanteLegal.NombreRepLegal,
                ApellidoRepLegal = a.RepresentanteLegal.ApellidoRepLegal
            }).ToListAsync();
    }

    public async Task<AsociacionModel?> ObtenerPorId(int id)
    {
        await using var context = await _context.CreateDbContextAsync();
        var a = await context.TbAsociacion
            .Include(a => a.RepresentanteLegal)
            .Include(a => a.ApoderadoLegal)
            .ThenInclude(f => f.ApoderadoFirma)
            .Include(a => a.TbDetalleRegAsociacion)
            .FirstOrDefaultAsync(a => a.AsociacionId == id);

        if (a == null) return null;

        return new AsociacionModel
        {
            AsociacionId = a.AsociacionId,
            NombreAsociacion = a.NombreAsociacion,
            Actividad = a.Actividad,
            Folio = a.Folio,
            NombreRepLegal = a.RepresentanteLegal.NombreRepLegal,
            ApellidoRepLegal = a.RepresentanteLegal.ApellidoRepLegal,
            CedulaRepLegal = a.RepresentanteLegal.CedulaRepLegal,
            CargoRepLegal = a.RepresentanteLegal.CargoRepLegal,
            TelefonoRepLegal = a.RepresentanteLegal.TelefonoRepLegal,
            DireccionRepLegal = a.RepresentanteLegal.DireccionRepLegal,
            NombreApoAbogado = a.ApoderadoLegal.NombreApoAbogado,
            ApellidoApoAbogado = a.ApoderadoLegal.ApellidoApoAbogado,
            CedulaApoAbogado = a.ApoderadoLegal.CedulaApoAbogado,
            TelefonoApoAbogado = a.ApoderadoLegal.TelefonoApoAbogado,
            CorreoApoAbogado = a.ApoderadoLegal.CorreoApoAbogado,
            DireccionApoAbogado = a.ApoderadoLegal.DireccionApoAbogado,
            PerteneceAFirma = a.ApoderadoLegal.ApoderadoFirma != null,
            NombreFirma = a.ApoderadoLegal.ApoderadoFirma?.NombreFirma ?? "",
            CorreoFirma = a.ApoderadoLegal.ApoderadoFirma?.CorreoFirma ?? "",
            TelefonoFirma = a.ApoderadoLegal.ApoderadoFirma?.TelefonoFirma ?? "",
            DireccionFirma = a.ApoderadoLegal.ApoderadoFirma?.DireccionFirma ?? "",
            NumRegAsecuencia = a.TbDetalleRegAsociacion.FirstOrDefault()?.NumRegAsecuencia ?? 0,
            NomRegAanio = a.TbDetalleRegAsociacion.FirstOrDefault()?.NomRegAanio ?? 0,
            NumRegAmes = a.TbDetalleRegAsociacion.FirstOrDefault()?.NumRegAmes ?? 0,
            NumRegAcompleta = a.TbDetalleRegAsociacion.FirstOrDefault()?.NumRegAcompleta
        };
    }

    public async Task<ResultModel> ActualizarAsociacion(AsociacionModel model)
    {
        await using var context = await _context.CreateDbContextAsync();
        var a = await context.TbAsociacion
            .Include(x => x.RepresentanteLegal)
            .Include(x => x.ApoderadoLegal)
            .ThenInclude(f => f.ApoderadoFirma)
            .FirstOrDefaultAsync(x => x.AsociacionId == model.AsociacionId);

        if (a == null)
            return new ResultModel { Success = false, Message = "Asociación no encontrada" };

        a.NombreAsociacion = model.NombreAsociacion;
        a.Actividad = model.Actividad;
        a.Folio = model.Folio;

        // Representante Legal
        a.RepresentanteLegal.NombreRepLegal = model.NombreRepLegal;
        a.RepresentanteLegal.ApellidoRepLegal = model.ApellidoRepLegal;
        a.RepresentanteLegal.CedulaRepLegal = model.CedulaRepLegal;
        a.RepresentanteLegal.CargoRepLegal = model.CargoRepLegal;
        a.RepresentanteLegal.TelefonoRepLegal = model.TelefonoRepLegal;
        a.RepresentanteLegal.DireccionRepLegal = model.DireccionRepLegal;

        // Apoderado Legal
        a.ApoderadoLegal.NombreApoAbogado = model.NombreApoAbogado;
        a.ApoderadoLegal.ApellidoApoAbogado = model.ApellidoApoAbogado;
        a.ApoderadoLegal.CedulaApoAbogado = model.CedulaApoAbogado;
        a.ApoderadoLegal.TelefonoApoAbogado = model.TelefonoApoAbogado;
        a.ApoderadoLegal.CorreoApoAbogado = model.CorreoApoAbogado;
        a.ApoderadoLegal.DireccionApoAbogado = model.DireccionApoAbogado;

        if (model.PerteneceAFirma)
        {
            if (a.ApoderadoLegal.ApoderadoFirma == null)
            {
                a.ApoderadoLegal.ApoderadoFirma = new TbApoderadoFirma();
            }

            a.ApoderadoLegal.ApoderadoFirma.NombreFirma = model.NombreFirma;
            a.ApoderadoLegal.ApoderadoFirma.CorreoFirma = model.CorreoFirma;
            a.ApoderadoLegal.ApoderadoFirma.TelefonoFirma = model.TelefonoFirma;
            a.ApoderadoLegal.ApoderadoFirma.DireccionFirma = model.DireccionFirma;
        }
        else
        {
            a.ApoderadoLegal.ApoderadoFirma = null;
        }

        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Asociación actualizada correctamente" };
    }

    public async Task<ResultModel> EliminarAsociacion(int id)
    {
        await using var context = await _context.CreateDbContextAsync();
        var a = await context.TbAsociacion.FirstOrDefaultAsync(x => x.AsociacionId == id);
        if (a == null) return new ResultModel { Success = false, Message = "Asociación no encontrada" };

        context.TbAsociacion.Remove(a);
        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Asociación eliminada correctamente" };
    }

    #endregion

    #region Apoderado y Firma

    public async Task<ResultModel> AgregarApoderado(int asociacionId, AsociacionModel apoderado)
    {
        await using var context = await _context.CreateDbContextAsync();
        var a = await context.TbAsociacion.Include(x => x.ApoderadoLegal)
            .FirstOrDefaultAsync(x => x.AsociacionId == asociacionId);
        if (a == null) return new ResultModel { Success = false, Message = "Asociación no encontrada" };

        a.ApoderadoLegal = new TbApoderadoLegal
        {
            NombreApoAbogado = apoderado.NombreApoAbogado,
            ApellidoApoAbogado = apoderado.ApellidoApoAbogado,
            CedulaApoAbogado = apoderado.CedulaApoAbogado,
            TelefonoApoAbogado = apoderado.TelefonoApoAbogado,
            CorreoApoAbogado = apoderado.CorreoApoAbogado,
            DireccionApoAbogado = apoderado.DireccionApoAbogado
        };

        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Apoderado agregado correctamente" };
    }

    public async Task<ResultModel> ActualizarApoderado(AsociacionModel apoderado)
    {
        await using var context = await _context.CreateDbContextAsync();
        var a = await context.TbApoderadoLegal.FirstOrDefaultAsync(x => x.ApoAbogadoId == apoderado.ApoAbogadoId);
        if (a == null) return new ResultModel { Success = false, Message = "Apoderado no encontrado" };

        a.NombreApoAbogado = apoderado.NombreApoAbogado;
        a.ApellidoApoAbogado = apoderado.ApellidoApoAbogado;
        a.CedulaApoAbogado = apoderado.CedulaApoAbogado;
        a.TelefonoApoAbogado = apoderado.TelefonoApoAbogado;
        a.CorreoApoAbogado = apoderado.CorreoApoAbogado;
        a.DireccionApoAbogado = apoderado.DireccionApoAbogado;

        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Apoderado actualizado correctamente" };
    }

    public async Task<ResultModel> EliminarApoderado(int apoderadoId)
    {
        await using var context = await _context.CreateDbContextAsync();
        var a = await context.TbApoderadoLegal.FirstOrDefaultAsync(x => x.ApoAbogadoId == apoderadoId);
        if (a == null) return new ResultModel { Success = false, Message = "Apoderado no encontrado" };

        context.TbApoderadoLegal.Remove(a);
        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Apoderado eliminado correctamente" };
    }

    #endregion

    #region Archivos

    public async Task<ResultModel> AgregarArchivo(int asociacionId, AArchivoModel archivo)
    {
        await using var context = await _context.CreateDbContextAsync();
        var a = await context.TbAsociacion.FirstOrDefaultAsync(x => x.AsociacionId == asociacionId);
        if (a == null) return new ResultModel { Success = false, Message = "Asociación no encontrada" };

        context.TbAsociacionArchivos.Add(new TbAsociacionArchivos
        {
            AsociacionArchivoId = asociacionId,
            NombreArchivoGuardado = archivo.NombreArchivo,
            Url = archivo.RutaArchivo,
            FechaSubida = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Archivo agregado correctamente" };
    }

    public async Task<ResultModel> EliminarArchivo(int archivoId)
    {
        await using var context = await _context.CreateDbContextAsync();
        var a = await context.TbAsociacionArchivos.FirstOrDefaultAsync(x => x.AsociacionArchivoId == archivoId);
        if (a == null) return new ResultModel { Success = false, Message = "Archivo no encontrado" };

        context.TbAsociacionArchivos.Remove(a);
        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Archivo eliminado correctamente" };
    }

    public async Task<List<AArchivoModel>> ObtenerArchivos(int asociacionId)
    {
        await using var context = await _context.CreateDbContextAsync();
        return await context.TbAsociacionArchivos
            .Where(x => x.AsociacionArchivoId == asociacionId)
            .Select(x => new AArchivoModel
            {
                AsociacionArchivoId = x.AsociacionArchivoId,
                NombreArchivo = x.NombreArchivoGuardado,
                RutaArchivo = x.Url,
                SubidoEn = x.FechaSubida
            })
            .ToListAsync();
    }

    #endregion

    #region Historial

    public async Task<List<DetalleRegAsociacionModel>> ObtenerDetalleHistorial(int asociacionId)
    {
        await using var context = await _context.CreateDbContextAsync();
        return await context.TbDetalleRegAsociacion
            .Where(x => x.AsociacionId == asociacionId)
            .Select(x => new DetalleRegAsociacionModel
            {
                DetalleRegAsociacionId = x.DetRegAsociacionId,
                CreadaEn = x.CreadaEn,
                CreadaPor = x.CreadaPor,
                NumRegAsecuencia = x.NumRegAsecuencia,
                NomRegAanio = x.NomRegAanio,
                NumRegAmes = x.NumRegAmes,
                NumRegAcompleta = x.NumRegAcompleta
            })
            .ToListAsync();
    }

    #endregion

    #region Firma

    public async Task<ResultModel> AgregarFirma(int apoderadoId, AsociacionModel firma)
    {
        await using var context = await _context.CreateDbContextAsync();
        var a = await context.TbApoderadoLegal.Include(x => x.ApoderadoFirma)
            .FirstOrDefaultAsync(x => x.ApoAbogadoId == apoderadoId);

        if (a == null) return new ResultModel { Success = false, Message = "Apoderado no encontrado" };

        if (a.ApoderadoFirma == null) a.ApoderadoFirma = new TbApoderadoFirma();

        a.ApoderadoFirma.NombreFirma = firma.NombreFirma;
        a.ApoderadoFirma.CorreoFirma = firma.CorreoFirma;
        a.ApoderadoFirma.TelefonoFirma = firma.TelefonoFirma;
        a.ApoderadoFirma.DireccionFirma = firma.DireccionFirma;

        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Firma agregada correctamente" };
    }

    public async Task<ResultModel> ActualizarFirma(AsociacionModel firma)
    {
        await using var context = await _context.CreateDbContextAsync();
        var a = await context.TbApoderadoFirma.FirstOrDefaultAsync(x => x.FirmaId == firma.ApoderadoFirmaId);
        if (a == null) return new ResultModel { Success = false, Message = "Firma no encontrada" };

        a.NombreFirma = firma.NombreFirma;
        a.CorreoFirma = firma.CorreoFirma;
        a.TelefonoFirma = firma.TelefonoFirma;
        a.DireccionFirma = firma.DireccionFirma;

        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Firma actualizada correctamente" };
    }

    public async Task<ResultModel> EliminarFirma(int firmaId)
    {
        await using var context = await _context.CreateDbContextAsync();
        var a = await context.TbApoderadoFirma.FirstOrDefaultAsync(x => x.FirmaId == firmaId);
        if (a == null) return new ResultModel { Success = false, Message = "Firma no encontrada" };
        context.TbApoderadoFirma.Remove(a);
        await context.SaveChangesAsync();
        return new ResultModel { Success = true, Message = "Firma eliminada correctamente" };
    }
    #endregion
}