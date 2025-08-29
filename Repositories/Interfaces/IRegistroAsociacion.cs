namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IRegistroAsociacion
{
    Task<ResultModel> CrearAsociacion(AsociacionModel model);
}