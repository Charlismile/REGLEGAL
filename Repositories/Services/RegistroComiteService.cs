
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using SISTEMALEGAL.DTOs;
using SISTEMALEGAL.Repositories.Interfaces;

public class RegistroComiteService : IRegistroComiteService
{
    private readonly DbContextLegal _context;

    public RegistroComiteService(DbContextLegal context) => _context = context;

    public async Task<List<TbRegionSalud>> ObtenerRegiones() =>
        await _context.TbRegionSalud.ToListAsync();

    public async Task<List<TbProvincia>> ObtenerProvincias(int regionId) =>
        await _context.TbProvincia.Where(p => p.RegionSaludId == regionId).ToListAsync();

    public async Task<List<TbDistrito>> ObtenerDistritos(int provinciaId) =>
        await _context.TbDistrito.Where(d => d.ProvinciaId == provinciaId).ToListAsync();

    public async Task<List<TbCorregimiento>> ObtenerCorregimientos(int distritoId) =>
        await _context.TbCorregimiento.Where(c => c.DistritoId == distritoId).ToListAsync();

    public async Task<ResultadoOperacion<int>> RegistrarComite(RegistroComiteDTO dto, string usuarioId)
    {
        var resultado = new ResultadoOperacion<int>();

        // Validaciones manuales aquí...

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Lógica de guardado...
            await transaction.CommitAsync();
            resultado.Exito = true;
            resultado.Data = 1; // ID del comité creado
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            resultado.Exito = false;
            resultado.Errores.Add(ex.Message);
        }

        return resultado;
    }
}