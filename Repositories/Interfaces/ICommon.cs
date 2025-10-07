
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface ICommon
{
    #region General
    Task<List<ListModel>> GetRegiones();
    Task<List<ListModel>> GetProvincias(int RegionId);
    Task<List<ListModel>> GetDistritos(int ProvinciaId);
    Task<List<ListModel>> GetCorregimientos(int DistritoId);
    Task<List<ListModel>> GetCargos();
    #endregion
    
    #region Archivos
    // Task<(bool ok, string mensaje)> GuardarArchivoComiteAsync(IBrowserFile archivo, string categoria, int comiteId);
    // Task<(bool ok, string mensaje)> GuardarArchivoAsociacionAsync(IBrowserFile archivo, string categoria, int asociacionId);
    #endregion
}