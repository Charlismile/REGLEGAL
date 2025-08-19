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
    // servicios de subidad de archivos
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
    public async Task<List<ListModel>> GetProvincias()
    {
        List<ListModel> Lista = new List<ListModel>();
        try
        {
            using (var localContext = await _context.CreateDbContextAsync())
            {
                Lista = await localContext.TbProvincia
                    .Select(x => new ListModel()
                    {
                        Id = x.ProvinciaId,
                        Name = x.NombreProvincia ?? "",
                    }).ToListAsync();
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
}