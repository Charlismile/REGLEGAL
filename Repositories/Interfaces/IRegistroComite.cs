using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IRegistroComite
{
    Task<ResultModel> CrearComite(ComiteModel model);
}