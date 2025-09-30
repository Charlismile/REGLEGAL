using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IRegistroComite
{
    // CRUD principal
    Task<ResultModel> CrearComite(ComiteModel model);
    Task<ResultModel> ActualizarComite(ComiteModel model);
    Task<ResultModel> EliminarComite(int comiteId);
    Task<List<ComiteModel>> ObtenerComites();
    Task<ComiteModel?> ObtenerComiteCompletoAsync(int comiteId);
    Task<ComiteModel?> ObtenerUltimoComiteConMiembrosAsync();

    // Gestión de miembros
    Task<TbDatosMiembros> AgregarMiembro(int comiteId, MiembroComiteModel miembro);
    Task<ResultModel> ActualizarMiembro(MiembroComiteModel miembro);
    Task<ResultModel> EliminarMiembro(int miembroId);
    Task<List<MiembroComiteModel>> ObtenerMiembros(int comiteId);
    Task<List<CargoModel>> ObtenerCargos();

    // Gestión de archivos del comité
    Task<ResultModel> AgregarArchivo(int comiteId, CArchivoModel archivo);
    Task<ResultModel> EliminarArchivo(int archivoId);
    Task<List<CArchivoModel>> ObtenerArchivos(int comiteId);
    Task<ResultModel> GuardarResolucionAsync(int comiteId, IBrowserFile archivo);

    // Historial / detalles
    Task<List<DetalleRegComiteModel>> ObtenerDetalleHistorial(int comiteId);
    Task GuardarHistorialMiembros(int comiteId, List<MiembroComiteModel> miembros);
}