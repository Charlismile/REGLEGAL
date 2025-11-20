using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IRegistroComite
{
    // === MÉTODOS NUEVOS con modelos específicos ===
    Task<ResultModel> CrearPersoneria(PersoneriaModel model);
    Task<ResultModel> RegistrarCambioDirectiva(CambioDirectivaModel model);
    Task<ResultModel> RegistrarJuntaInterventora(JuntaInterventoraModel model);

    // === Consultas (mantienen ComiteModel para compatibilidad) ===
    Task<List<ComiteModel>> ObtenerComites();
    Task<List<ComiteModel>> ObtenerComitesPorTipo(TipoTramite tipoTramite);
    Task<ComiteModel?> ObtenerComiteCompletoAsync(int comiteId);
    Task<List<HistorialComiteModel>> ObtenerHistorialComite(int comiteId);

    // === Utilidades ===
    Task<ResultModel> GuardarResolucionAsync(int comiteId, IBrowserFile archivo);
}