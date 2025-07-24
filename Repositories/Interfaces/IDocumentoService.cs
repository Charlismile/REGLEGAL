using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.Entities.BdSisLegal;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IDocumentoService
{
    Task<string> GuardarArchivoAsync(IBrowserFile archivo, string categoria, int? detAsociacionId = null, int? detComiteId = null);
    Task<List<TbArchivos>> ObtenerDocumentosPorAsociacionAsync(int asociacionId);
    Task<List<TbArchivos>> ObtenerDocumentosPorComiteAsync(int comiteId);
}