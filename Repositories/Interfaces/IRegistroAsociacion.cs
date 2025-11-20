using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IRegistroAsociacion
{
    Task<ResultModel> CrearAsociacion(AsociacionModel model);
    Task<ResultModel> ActualizarAsociacion(AsociacionModel model);

    Task<ResultModel> ActualizarMiembrosAsociacionAsync(int asociacionId, CambioMiembrosModel miembros, string usuarioId);
    
    Task<ResultModel> EliminarAsociacion(int id);
    Task<AsociacionModel?> ObtenerPorId(int id);
    Task<List<AsociacionModel>> ObtenerTodas();
    
    Task<ResultModel> AgregarArchivo(int asociacionId, AArchivoModel archivo);
    Task<ResultModel> EliminarArchivo(int archivoId);
    Task<List<AArchivoModel>> ObtenerArchivos(int asociacionId);
    
    Task<List<DetalleRegAsociacionModel>> ObtenerDetalleHistorial(int asociacionId);
}