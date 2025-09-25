using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IRegistroAsociacion
{
    // CRUD principal
    Task<ResultModel> CrearAsociacion(AsociacionModel model);
    Task<List<AsociacionModel>> ObtenerTodas();
    Task<AsociacionModel?> ObtenerPorId(int id);
    Task<ResultModel> ActualizarAsociacion(AsociacionModel model);
    Task<ResultModel> EliminarAsociacion(int id);

    // Historial / detalles de registro
    Task<List<DetalleRegAsociacionModel>> ObtenerDetalleHistorial(int asociacionId);
}