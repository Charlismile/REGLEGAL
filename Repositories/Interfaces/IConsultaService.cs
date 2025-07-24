using REGISTROLEGAL.DTOs;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IConsultaService
{
    Task<List<RegistroAsociacionDto>> ObtenerAsociacionesAsync();
    Task<List<ComiteListadoDTO>> ObtenerComitesAsync();
}