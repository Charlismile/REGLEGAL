using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class CommonServices : ICommon
{
    private readonly IDbContextFactory<DbContextLegal> _Context;
    private readonly IConfiguration _Configuration;

    public CommonServices(IDbContextFactory<DbContextLegal> Context, IConfiguration Configuration)
    {
        _Context = Context;
        _Configuration = Configuration;
    }

    public async Task<string> GetFakePassword()
    {
        string Password = _Configuration.GetSection("FakePass").Value ?? "";
        return await Task.FromResult(Password);
    }

    public async Task<List<RegistroDto>> ObtenerRegionesAsync()
    {
        var lista = new List<RegistroDto>();
        try
        {
            using (var localContext = await _Context.CreateDbContextAsync())
            {
                lista = await localContext.TbRegionSalud
                    .Select(x => new RegistroDto()
                    {
                        RId = x.RegionSaludId,
                        NombreRegion = x.NombreRegion
                    }).ToListAsync();
            }
        }
        catch (Exception)
        {
            // manejar errores o log
        }

        return lista;
    }

    
    
}