// Services/Implementations/NumeracionService.cs

using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class RegistroNumeracionService : IRegistroNumeracionService
{
    private readonly DbContextLegal _context;

    public RegistroNumeracionService(DbContextLegal context)
    {
        _context = context;
    }

    public async Task<string> GenerarNumeroAsociacion()
    {
        return await GenerarNumero(1); // Ajusta según tu lógica de EntidadId
    }

    public async Task<string> GenerarNumeroComite()
    {
        return await GenerarNumero(2);
    }

    private async Task<string> GenerarNumero(int entidadId)
    {
        var anio = DateTime.Now.Year;
        var mes = DateTime.Now.Month;

        var secuencia = await _context.TbRegSecuencia
            .FirstOrDefaultAsync(s => s.EntidadId == entidadId && s.Anio == anio && s.Activo);

        if (secuencia == null)
        {
            secuencia = new TbRegSecuencia
            {
                EntidadId = entidadId,
                Anio = anio,
                Numeracion = 1,
                Activo = true
            };
            _context.TbRegSecuencia.Add(secuencia);
        }
        else
        {
            secuencia.Numeracion++;
        }

        await _context.SaveChangesAsync();
        return $"{secuencia.Numeracion}/{anio}/{mes:D2}";
    }
}