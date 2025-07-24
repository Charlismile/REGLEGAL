// Services/Implementations/ConsultaService.cs

using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class ConsultaService : IConsultaService
{
    private readonly DbContextLegal _context;

    public ConsultaService(DbContextLegal context)
    {
        _context = context;
    }

    public async Task<List<RegistroAsociacionDto>> ObtenerAsociacionesAsync()
    {
        return await _context.TbDetalleRegAsociacion
            .Include(d => d.Asociacion)
            .ThenInclude(a => a.RepresentanteLegal)
            .Select(d => new RegistroAsociacionDto
            {
                AsociacionId = d.AsociacionId,
                NombreAsociacion = d.Asociacion.NombreAsociacion,
                NumRegCompleta = d.NumRegAcompleta,
                CreadaEn = d.CreadaEn,
                Estado = "Pendiente" // Puedes mejorar con lógica de estado
            })
            .OrderByDescending(a => a.CreadaEn)
            .ToListAsync();
    }

    public async Task<List<ComiteListadoDTO>> ObtenerComitesAsync()
    {
        return await _context.TbDetalleRegComite
            .Include(d => d.DatosComite)
            .Select(d => new ComiteListadoDTO
            {
                DComiteId = d.ComiteId,
                NombreComiteSalud = d.DatosComite.NombreComiteSalud,
                NumRegCompleta = d.NumRegCoCompleta,
                CreadaEn = d.CreadaEn,
                Estado = "Pendiente"
            })
            .OrderByDescending(c => c.CreadaEn)
            .ToListAsync();
    }
}