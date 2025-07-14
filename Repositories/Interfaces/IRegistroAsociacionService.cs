using SISTEMALEGAL.DTOs;

namespace SISTEMALEGAL.Repositories.Interfaces;

public interface IRegistroAsociacionService
{
    Task<ResultadoOperacion<int>> RegistrarAsociacion(RegistroAsociacionDTO dto, string usuarioId);  
}