namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IHistorialRegistro
{
    Task RegistrarHistorialAsociacionAsync(int detRegAsociacionId, int asociacionId, string accion, string comentario, string usuarioId);
    Task RegistrarHistorialComiteAsync(int detRegComiteId, int comiteId, int estadoId, string comentario, string usuarioId);
}