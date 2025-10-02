using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IArchivoLegalService
{
    Task<byte[]> ValidateAndReadPdfAsync(IBrowserFile file);
    Task<DocumentoModel> GuardarDocumentoAsync(string entidadTipo, int entidadId, byte[] bytes, string nombreArchivo, string categoria);
}