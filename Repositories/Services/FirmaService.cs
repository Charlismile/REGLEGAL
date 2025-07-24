// Services/Implementations/FirmaService.cs

using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class FirmaService : IFirmaService
{
    private readonly DbContextLegal _context;

    public FirmaService(DbContextLegal context)
    {
        _context = context;
    }

    public Task<List<TbApoderadoFirma>> ObtenerFirmasAsync() => 
        _context.TbApoderadoFirma.OrderBy(f => f.NombreFirma).ToListAsync();

    public Task<TbApoderadoFirma?> ObtenerFirmaPorIdAsync(int firmaId) => 
        _context.TbApoderadoFirma.FindAsync(firmaId).AsTask();
}