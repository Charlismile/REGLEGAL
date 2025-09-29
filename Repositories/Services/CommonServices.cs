using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class CommonServices : ICommon
{
    private readonly IDbContextFactory<DbContextLegal> _context;

    private readonly string _rutaBaseArchivos =
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documentos-legales");

    public CommonServices(IDbContextFactory<DbContextLegal> context)
    {
        _context = context;
    }


    // servicios generales

    #region Ubicación jerárquica

    #region Ubicación Geográfica

    public async Task<List<ListModel>> GetRegiones()
    {
        List<ListModel> lista = new List<ListModel>();
        try
        {
            using (var localContext = await _context.CreateDbContextAsync())
            {
                lista = await localContext.TbRegionSalud
                    .Select(x => new ListModel
                    {
                        Id = x.RegionSaludId,
                        Name = x.NombreRegion ?? ""
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
        }
        catch (Exception)
        {
            // manejar errores si deseas
        }

        return lista;
    }

    public async Task<List<ListModel>> GetProvincias(int regionId)
    {
        List<ListModel> lista = new List<ListModel>();
        try
        {
            using (var localContext = await _context.CreateDbContextAsync())
            {
                lista = await localContext.TbProvincia
                    .Where(x => x.RegionSaludId == regionId) // filtramos por RegionSaludId
                    .Select(x => new ListModel
                    {
                        Id = x.ProvinciaId,
                        Name = x.NombreProvincia ?? ""
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
        }
        catch (Exception)
        {
        }

        return lista;
    }

    public async Task<List<ListModel>> GetDistritos(int provinciaId)
    {
        List<ListModel> lista = new List<ListModel>();
        try
        {
            using (var localContext = await _context.CreateDbContextAsync())
            {
                lista = await localContext.TbDistrito
                    .Where(x => x.ProvinciaId == provinciaId) // filtramos por ProvinciaId
                    .Select(x => new ListModel
                    {
                        Id = x.DistritoId,
                        Name = x.NombreDistrito ?? ""
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
        }
        catch (Exception)
        {
        }

        return lista;
    }

    public async Task<List<ListModel>> GetCorregimientos(int distritoId)
    {
        List<ListModel> lista = new List<ListModel>();
        try
        {
            using (var localContext = await _context.CreateDbContextAsync())
            {
                lista = await localContext.TbCorregimiento
                    .Where(x => x.DistritoId == distritoId) // filtramos por DistritoId
                    .Select(x => new ListModel
                    {
                        Id = x.CorregimientoId,
                        Name = x.NombreCorregimiento ?? ""
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
        }
        catch (Exception)
        {
        }

        return lista;
    }

    #endregion


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
            throw new Exception("Error al obtener los cargos.", ex);
        }

        return Lista;
    }

    public async Task<(bool ok, string mensaje)> GuardarArchivoAsync(
        IBrowserFile archivo,
        string categoria,
        int asociacionId
    )
    {
        try
        {
            var rutaCategoria = Path.Combine(_rutaBaseArchivos, categoria, asociacionId.ToString());
            if (!Directory.Exists(rutaCategoria))
                Directory.CreateDirectory(rutaCategoria);

            var filePath = Path.Combine(rutaCategoria, archivo.Name);

            await using var stream = archivo.OpenReadStream(10 * 1024 * 1024); // 10 MB
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await stream.CopyToAsync(fileStream);

            return (true, "Archivo guardado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al guardar archivo {archivo.Name}: {ex.Message}");
        }
    }

    #endregion
}