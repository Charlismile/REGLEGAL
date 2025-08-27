
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
    
    #region Comite
    Task<(bool exito, string mensaje)> RegistrarComiteAsync(ComiteModel model, IBrowserFile archivoResolucion);
    #endregion

    #region Asociacion

    

        #endregion
    Task<(bool ok, string mensaje)> GuardarArchivoAsync(IBrowserFile archivo, string categoria, int? asociacionId = null, int? comiteId = null);
    
}