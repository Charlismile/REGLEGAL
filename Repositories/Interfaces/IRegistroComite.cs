using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;

public interface IRegistroComite
{
    // === CRUD principal ===
    Task<ResultModel> CrearComite(ComiteModel model);
    Task<ResultModel> ActualizarComite(ComiteModel model);
    Task<ResultModel> EliminarComite(int comiteId);

    // === Consultas ===
    Task<List<ComiteModel>> ObtenerComites();
    Task<List<ComiteModel>> ObtenerComitesPorTipo(TipoTramite tipoTramite);
    Task<ComiteModel?> ObtenerComiteCompletoAsync(int comiteId);
    Task<List<HistorialComiteModel>> ObtenerHistorialComite(int comiteId);

    // === Utilidades opcionales ===
    Task<ResultModel> GuardarResolucionAsync(int comiteId, IBrowserFile archivo);
    
}