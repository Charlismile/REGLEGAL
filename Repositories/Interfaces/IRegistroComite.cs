using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IRegistroComite
{
    // CRUD principal
    Task<ResultModel> CrearComite(ComiteModel model);
    Task<List<ComiteModel>> ObtenerTodos();
    Task<ComiteModel?> GetComiteByIdAsync(int comiteId);
    Task<ResultModel> ActualizarComite(ComiteModel model);
    Task<ResultModel> EliminarComite(int comiteId);

    // Gestión de miembros
    Task<TbDatosMiembros> AgregarMiembro(int comiteId, MiembroComiteModel miembro);
    Task<ResultModel> ActualizarMiembro(MiembroComiteModel miembro);
    Task<ResultModel> EliminarMiembro(int miembroId);
    Task<List<MiembroComiteModel>> ObtenerMiembros(int comiteId);

    // 🔹 Gestión de cargos (simplificada: un cargo por miembro)
    Task<List<CargoModel>> ObtenerCargos();

    // Gestión de archivos del comité
    Task<ResultModel> AgregarArchivo(int comiteId, CArchivoModel archivo);
    Task<ResultModel> EliminarArchivo(int archivoId);
    Task<List<CArchivoModel>> ObtenerArchivos(int comiteId);

    // Historial / detalles
    Task<List<DetalleRegComiteModel>> ObtenerDetalleHistorial(int comiteId);
}