using REGISTROLEGAL.Models.Entities.BdSisLegal;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IFirmaService
{
    Task<List<TbApoderadoFirma>> ObtenerFirmasAsync();
    Task<TbApoderadoFirma?> ObtenerFirmaPorIdAsync(int firmaId);
}