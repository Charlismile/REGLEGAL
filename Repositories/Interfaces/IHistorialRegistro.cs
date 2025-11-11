using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IHistorialRegistro
{
    Task RegistrarHistorialComiteAsync(int detRegComiteId, int comiteId, int estadoId, string comentario, string usuarioId);
    Task RegistrarHistorialAsociacionAsync(int detRegAsociacionId, int asociacionId, string accion, string comentario, string usuarioId);
}