using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IArchivoLegalService
{
    Task<byte[]> ValidateAndReadPdfAsync(IBrowserFile file);

    #region Comites

    Task<CArchivoModel> GuardarArchivoComiteAsync(int comiteId, IBrowserFile archivo, string categoria);
    Task<List<CArchivoModel>> ObtenerArchivosComiteAsync(int comiteId, string categoria);
    Task<bool> DesactivarArchivoComiteAsync(int archivoId);

        #endregion

    #region Asociaciones

    Task<AArchivoModel> GuardarArchivoAsociacionAsync(int asociacionId, IBrowserFile archivo, string categoria);
    Task<List<AArchivoModel>> ObtenerArchivosAsociacionAsync(int asociacionId, string categoria);
    Task<bool> DesactivarArchivoAsociacionAsync(int archivoId);

    #endregion
}