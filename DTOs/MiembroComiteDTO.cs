using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.DTOs;

public class MiembroComiteDTO
{
    [Required(ErrorMessage = "El nombre del miembro es obligatorio")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
    [StringLength(200)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "La cédula es obligatoria")]
    [RegularExpression(@"^\d{1,2}-\d{1,8}-\d{1,8}$", 
        ErrorMessage = "Formato inválido. Ejemplo: 1-123456-7")]
    [StringLength(20)]
    public string Cedula { get; set; } = "";

    [Required(ErrorMessage = "El cargo es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cargo válido")]
    public int CargoId { get; set; }

    public string NombreCargo { get; set; } = "";
}