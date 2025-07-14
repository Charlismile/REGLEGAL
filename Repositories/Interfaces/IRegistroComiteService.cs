using REGISTROLEGAL.Models.Entities.BdSisLegal;
using SISTEMALEGAL.DTOs;

namespace SISTEMALEGAL.Repositories.Interfaces;

public interface IRegistroComiteService
{
    Task<List<TbRegionSalud>> ObtenerRegiones();
    Task<List<TbProvincia>> ObtenerProvincias(int regionId);
    Task<List<TbDistrito>> ObtenerDistritos(int provinciaId);
    Task<List<TbCorregimiento>> ObtenerCorregimientos(int distritoId);
    Task<ResultadoOperacion<int>> RegistrarComite(RegistroComiteDTO dto, string usuarioId);
}