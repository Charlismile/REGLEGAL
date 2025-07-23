using REGISTROLEGAL.DTOs;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IRegistroComiteService
{
    // 🔹 UBICACIÓN GEOGRÁFICA (usar DTOs, no entidades)
    Task<List<RegistroComiteDTO>> GetRegionesAsync();
    Task<List<RegistroComiteDTO>> GetProvinciasAsync(int regionId);
    Task<List<RegistroComiteDTO>> GetDistritosAsync(int provinciaId);
    Task<List<RegistroComiteDTO>> GetCorregimientosAsync(int distritoId);

    // 🔹 CATÁLOGOS
    Task<List<RegistroComiteDTO>> GetTiposTramiteAsync();
    Task<List<RegistroComiteDTO>> GetCargosAsync();

    // 🔹 GESTIÓN DE COMITÉS
    Task<RegistroComiteDTO> GetComitePorIdAsync(int id);
    Task<List<RegistroComiteDTO>> GetAllAsync(); // Más claro que "GetTodosLosComitesAsync"
    Task<List<RegistroComiteDTO>> SearchAsync(string? termino = null, int? regionId = null, int? provinciaId = null);

    // 🔹 CRUD
    Task<bool> SaveAsync(RegistroComiteDTO dto); // Maneja tanto crear como actualizar
    Task<bool> UpdateAsync(RegistroComiteDTO dto);
    Task<bool> DeleteAsync(int id);

    // 🔹 VALIDACIONES (opcional, pero útil)
    Task<bool> ExistsByIdAsync(int id);
    Task<bool> IsCedulaUniqueAsync(string cedula, int? comiteId = null);
}