using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class CommonServices : ICommon
{
    private readonly IDbContextFactory<DbContextLegal> _context;

    private readonly string _rutaBaseArchivos = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documentos-legales");
    public CommonServices(IDbContextFactory<DbContextLegal> context)
    {
        _context = context;
    }

    #region archivos
    public async Task<(bool ok, string mensaje)> GuardarArchivoAsync(IBrowserFile archivo, string categoria, int? asociacionId = null, int? comiteId = null)
    {
        try
        {
            if ((asociacionId == null && comiteId == null) || (asociacionId != null && comiteId != null))
                return (false, "Debe asociar el archivo a una Asociación o a un Comité, pero no a ambos.");

            if (archivo == null || archivo.Size == 0)
                return (false, "El archivo no puede estar vacío.");

            var extensionesPermitidas = new[] { ".pdf", ".docx", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(archivo.Name).ToLower();
            if (!extensionesPermitidas.Contains(extension))
                return (false, $"Extensión no permitida ({extension}). Solo se permiten: {string.Join(", ", extensionesPermitidas)}");

            if (!Directory.Exists(_rutaBaseArchivos))
                Directory.CreateDirectory(_rutaBaseArchivos);

            var nombreGuardado = $"{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(_rutaBaseArchivos, nombreGuardado);

            await using (var stream = archivo.OpenReadStream())
            await using (var fileStream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            using var localContext = await _context.CreateDbContextAsync();

            int version = 1;
            var archivoExistente = await localContext.TbArchivos
                .Where(a => a.Categoria == categoria &&
                            ((asociacionId != null && a.DetRegAsociacionId == asociacionId) ||
                             (comiteId != null && a.DetRegComiteId == comiteId)))
                .OrderByDescending(a => a.Version)
                .FirstOrDefaultAsync();

            if (archivoExistente != null)
                version = archivoExistente.Version + 1;

            var nuevoArchivo = new TbArchivos
            {
                DetRegAsociacionId = asociacionId,
                DetRegComiteId = comiteId,
                Categoria = categoria,
                NombreOriginal = archivo.Name,
                NombreArchivoGuardado = nombreGuardado,
                Url = $"/uploads/documentos-legales/{nombreGuardado}",
                FechaSubida = DateTime.Now,
                Version = version,
                IsActivo = true
            };

            localContext.TbArchivos.Add(nuevoArchivo);
            await localContext.SaveChangesAsync();

            return (true, "Archivo guardado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al guardar archivo: {ex.Message}");
        }
    }
    #endregion

    #region generales
    // servicios generales
 public async Task<List<ListModel>> GetRegiones()
    {
        List<ListModel> Lista = new List<ListModel>();
        try
        {
            using (var localContext = await _context.CreateDbContextAsync())
            {
                Lista = await localContext.TbRegionSalud
                    .Select(x => new ListModel()
                    {
                        Id = x.RegionSaludId,
                        Name = x.NombreRegion ?? "",
                    }).ToListAsync();
            }
        }
        catch (Exception)
        {
        }
        return Lista;
    }
    
    public async Task<List<ListModel>> GetProvincias(int RegionId)
    {
        List<ListModel> Lista = new List<ListModel>();
        try
        {
            using (var localContext = await _context.CreateDbContextAsync())
            {
                Lista = await localContext.TbProvincia.Where(x => x.ProvinciaId == RegionId)
                    .Select(x => new ListModel()
                    {
                        Id = x.ProvinciaId,
                        Name = x.NombreProvincia ?? "",
                    }).ToListAsync();

                Lista = Lista.OrderBy(x => x.Name).ToList();
            }
        }
        catch (Exception)
        {
        }
        return Lista;
    }
    
    public async Task<List<ListModel>> GetDistritos(int ProvinciaId)
    {
        List<ListModel> Lista = new List<ListModel>();
        try
        {
            using (var localContext = await _context.CreateDbContextAsync())
            {
                Lista = await localContext.TbDistrito.Where(x => x.ProvinciaId == ProvinciaId)
                    .Select(x => new ListModel()
                    {
                        Id = x.DistritoId,
                        Name = x.NombreDistrito ?? "",
                    }).ToListAsync();

                Lista = Lista.OrderBy(x => x.Name).ToList();
            }
        }
        catch (Exception)
        {
        }
        return Lista;
    }

    public async Task<List<ListModel>> GetCorregimientos(int DistritoId)
    {
        List<ListModel> Lista = new List<ListModel>();
        try
        {
            using (var localContext = await _context.CreateDbContextAsync())
            {
                Lista = await localContext.TbCorregimiento.Where(x => x.DistritoId == DistritoId)
                    .Select(x => new ListModel()
                    {
                        Id = x.CorregimientoId,
                        Name = x.NombreCorregimiento ?? "",
                    }).ToListAsync();

                Lista = Lista.OrderBy(x => x.Name).ToList();
            }
        }
        catch (Exception)
        {
        }
        return Lista;
    }


    public async Task<List<ListModel>> GetCargos()
    {
        List<ListModel> Lista = new List<ListModel>();
        try
        {
            using (var localContext = await _context.CreateDbContextAsync())
            {
                Lista = await localContext.TbCargosMiembrosComite // Asumiendo que tienes una tabla TbCargo
                    .Select(x => new ListModel()
                    {
                        Id = x.CargoId,
                        Name = x.NombreCargo ?? ""
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
        }
        catch (Exception ex)
        {
            // Opcional: loggear ex.Message o usar ILogger
            // Nunca ignores excepciones en producción
            throw new Exception("Error al obtener los cargos.", ex);
        }
        return Lista;
    }
    #endregion

    #region Comite
    public async Task<(bool exito, string mensaje)> RegistrarComiteAsync(ComiteModel model, IBrowserFile archivoResolucion)
{
    using var context = await _context.CreateDbContextAsync();
    using var transaction = await context.Database.BeginTransactionAsync();

    try
    {
        var userId = "USR-TEMP"; // TOD0: reemplazar con usuario real
        if (string.IsNullOrEmpty(userId))
            return (false, "Usuario no autenticado.");

        // Validaciones de miembros según trámite
        if (model.TipoTramiteEnum == TipoTramite.Personeria && model.Miembros.Count != 7)
            return (false, "Para Personería, se requieren exactamente 7 miembros.");

        if (model.TipoTramiteEnum == TipoTramite.JuntaInterventora && model.MiembrosInterventores.Count > 2)
            return (false, "La Junta Interventora solo puede tener hasta 2 miembros.");

        // Secuencia
        var anio = DateTime.Now.Year;
        var mes = DateTime.Now.Month;
        var secuenciaEntidadId = 2; // Comité
        var secuencia = await context.TbRegSecuencia
            .FirstOrDefaultAsync(s => s.EntidadId == secuenciaEntidadId && s.Anio == anio && s.Activo);

        if (secuencia == null)
            return (false, "No hay secuencia disponible para este año.");

        var numSecuencia = secuencia.Numeracion;
        secuencia.Numeracion++;
        await context.SaveChangesAsync();

        // DetalleRegComite
        var tipoTramiteId = model.TipoTramiteEnum switch
        {
            TipoTramite.Personeria => 1,
            TipoTramite.CambioDirectiva => 2,
            TipoTramite.JuntaInterventora => 3,
            _ => throw new ArgumentException("Tipo de trámite no válido")
        };

        var detalleReg = new TbDetalleRegComite
        {
            TipoTramiteId = tipoTramiteId,
            CreadaEn = DateTime.Now,
            CreadaPor = userId,
            NumRegCoSecuencia = numSecuencia,
            NomRegCoAnio = anio,
            NumRegCoMes = mes
        };

        context.TbDetalleRegComite.Add(detalleReg);
        await context.SaveChangesAsync();

        // DatosComite
        var datosComite = new TbDatosComite
        {
            NombreComiteSalud = model.NombreComiteSalud,
            Comunidad = model.Comunidad,
            RegionSaludId = model.RegionSaludId ?? 0,
            ProvinciaId = model.ProvinciaId ?? 0,
            DistritoId = model.DistritoId,
            CorregimientoId = model.CorregimientoId,
            DcomiteId = detalleReg.ComiteId // FK
        };

        context.TbDatosComite.Add(datosComite);
        await context.SaveChangesAsync();

        // Miembros
        var miembros = model.TipoTramiteEnum == TipoTramite.JuntaInterventora
            ? model.MiembrosInterventores
            : model.Miembros;

        foreach (var m in miembros)
        {
            context.TbDatosMiembros.Add(new TbDatosMiembros
            {
                NombreMiembro = m.NombreMiembro,
                CedulaMiembro = m.CedulaMiembro,
                CargoId = m.CargoId,
                DcomiteId = datosComite.DcomiteId
            });
        }
        await context.SaveChangesAsync();

        // Archivo
        if (archivoResolucion != null)
        {
            var (archivoOk, archivoMsg) = await GuardarArchivoAsync(
                archivoResolucion,
                "Resolución",
                comiteId: detalleReg.ComiteId
            );

            if (!archivoOk)
            {
                await transaction.RollbackAsync();
                return (false, "Error al guardar el archivo: " + archivoMsg);
            }
        }

        // Historial
        var historial = new TbDetalleRegComiteHistorial
        {
            ComiteId = detalleReg.ComiteId,
            CoEstadoSolicitudId = 1,
            FechaCambioCo = DateTime.Now,
            UsuarioRevisorCo = userId,
            ComentarioCo = "Registro inicial del comité"
        };

        context.TbDetalleRegComiteHistorial.Add(historial);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();

        return (true, $"Comité registrado con éxito. Número de registro: {numSecuencia}/{anio}/{mes}");
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return (false, "Error interno al registrar el comité: " + ex.Message);
    }
}

    #endregion
}
