
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.Services
{
    public interface IRegistroAsociacionService
    {
        /// <summary>
        /// Obtiene un registro de asociación por su ID.
        /// </summary>
        /// <param name="id">ID del registro</param>
        /// <returns>DTO con los datos del registro</returns>
        Task<RegistroAsociacionDTO> ObtenerPorIdAsync(int id);

        /// <summary>
        /// Actualiza un registro de asociación existente, incluyendo nuevos archivos.
        /// </summary>
        /// <param name="model">DTO con los datos actualizados</param>
        /// <param name="nuevosArchivos">Lista de nuevos archivos seleccionados para subir</param>
        /// <returns>Task</returns>
        Task ActualizarAsync(RegistroAsociacionDTO model, List<IBrowserFile> nuevosArchivos);
    }
}