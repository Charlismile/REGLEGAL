using REGISTROLEGAL.Models.Entities.BdSisLegal;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IUbicacionService
{
    Task<List<TbRegionSalud>> ObtenerRegionesAsync();
    Task<List<TbProvincia>> ObtenerProvinciasPorRegionAsync(int regionId);
    Task<List<TbDistrito>> ObtenerDistritosPorProvinciaAsync(int provinciaId);
    Task<List<TbCorregimiento>> ObtenerCorregimientosPorDistritoAsync(int distritoId);
}