using REGISTROLEGAL.DTOs;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface ICommon
{
    #region Datos Requisicion
    Task<string> GetFakePassword();
    Task<List<RegistroDto>> ObtenerRegionesAsync();
    
    #endregion
}