
using Microsoft.AspNetCore.Components.Forms;
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
    
    #region Archivos
    /// <summary>
    /// Guarda un archivo en el servidor y registra la información en la base de datos
    /// </summary>
    /// <param name="archivo">Archivo a subir</param>
    /// <param name="categoria">Categoría del archivo</param>
    /// <param name="asociacionId">ID de la asociación asociada (opcional)</param>
    /// <param name="comiteId">ID del comité asociado (opcional)</param>
    /// <returns>Tupla indicando éxito y mensaje</returns>
    Task<(bool ok, string mensaje)> GuardarArchivoAsync(
        IBrowserFile archivo, 
        string categoria, 
        int? asociacionId = null, 
        int? comiteId = null
    );
    #endregion
}