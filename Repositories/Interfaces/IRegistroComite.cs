using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IRegistroComite
{
    Task<ResultModel> CrearComite(ComiteModel model);
    
    Task<TbDatosMiembros> AgregarMiembro(int comiteId, MiembroComiteModel miembro);
    // Interfaz
    Task<ComiteModel?> GetComiteByIdAsync(int comiteId);

    
}