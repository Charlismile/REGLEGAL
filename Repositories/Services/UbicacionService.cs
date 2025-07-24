// Services/Implementations/UbicacionService.cs

using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class UbicacionService : IUbicacionService
{
    private readonly DbContextLegal _context;

    public UbicacionService(DbContextLegal context)
    {
        _context = context;
    }

    public Task<List<TbRegionSalud>> ObtenerRegionesAsync() => 
        _context.TbRegionSalud.OrderBy(r => r.NombreRegion).ToListAsync();

    public Task<List<TbProvincia>> ObtenerProvinciasPorRegionAsync(int regionId) => 
        _context.TbProvincia.Where(p => p.RegionSaludId == regionId).OrderBy(p => p.NombreProvincia).ToListAsync();

    public Task<List<TbDistrito>> ObtenerDistritosPorProvinciaAsync(int provinciaId) => 
        _context.TbDistrito.Where(d => d.ProvinciaId == provinciaId).OrderBy(d => d.NombreDistrito).ToListAsync();

    public Task<List<TbCorregimiento>> ObtenerCorregimientosPorDistritoAsync(int distritoId) => 
        _context.TbCorregimiento.Where(c => c.DistritoId == distritoId).OrderBy(c => c.NombreCorregimiento).ToListAsync();
}