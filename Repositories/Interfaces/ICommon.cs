
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface ICommon
{
    #region Datos Comite
    Task<List<ListModel>> GetRegiones();
    Task<List<ListModel>> GetProvincias();
    Task<List<ListModel>> GetDistritos(int ProvinciaId);
    Task<List<ListModel>> GetCorregimientos(int DistritoId);
    Task<List<ListModel>> GetCargos();
    
    #endregion
}