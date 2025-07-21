using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using SISTEMALEGAL.DTOs;

namespace SISTEMALEGAL.Repositories.Interfaces;

public interface IRegistroComiteService
{
    Task<List<TbRegionSalud>> GetRegionesAsync();
    Task<List<TbProvincia>> GetProvinciasAsync(int regionId);
    Task<List<TbDistrito>> GetDistritosAsync(int provinciaId);
    Task<List<TbCorregimiento>> GetCorregimientosAsync(int distritoId);
    Task<List<TbTipoTramite>> GetTramitesAsync();
    Task<List<TbCargosMiembrosComite>> GetCargosAsync();

    Task<RegistroComiteDTO> GetComitePorIdAsync(int id);
    Task<bool> GuardarComiteAsync(RegistroComiteDTO model);
}