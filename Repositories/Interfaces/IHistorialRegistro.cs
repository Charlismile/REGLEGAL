using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IHistorialRegistro
{
    Task RegistrarHistorialComiteAsync(int detRegComiteId, int comiteId, int estadoId, string comentario, string usuarioId);
    Task RegistrarHistorialAsociacionAsync(int detRegAsociacionId, int asociacionId, string accion, string comentario, string usuarioId);
    
    // 🔹 NUEVOS: Métodos para registrar historial de miembros
    Task RegistrarHistorialMiembrosAsync(List<TbMiembrosComite> miembrosAntiguos, int comiteId, string usuarioId, string accion = "Actualización");
    Task RegistrarHistorialMiembrosCambioDirectivaAsync(int comiteBaseId, List<MiembroComiteModel> nuevosMiembros, string usuarioId);
    Task RegistrarHistorialMiembrosJuntaInterventoraAsync(int comiteBaseId, List<MiembroComiteModel> interventores, string usuarioId);
}