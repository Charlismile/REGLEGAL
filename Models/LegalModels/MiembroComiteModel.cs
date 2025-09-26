using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models.LegalModels;

public class MiembroComiteModel
{
    public int MiembroId { get; set; }   // Identificador del miembro
    public int ComiteId { get; set; }    // Relación con comité

    [Required(ErrorMessage = "El nombre del miembro es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string NombreMiembro { get; set; } = "";

    [Required(ErrorMessage = "El apellido del miembro es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string ApellidoMiembro { get; set; } = "";

    [Required(ErrorMessage = "El cargo del miembro es obligatorio")]
    public int CargoId { get; set; }

    // Opcional: para mostrar en UI
    public string? NombreCargo { get; set; }

    [Required(ErrorMessage = "El número de cédula es obligatorio")]
    [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
    public string CedulaMiembro { get; set; } = "";

    [Phone(ErrorMessage = "Número de teléfono no válido")]
    [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
    public string? TelefonoMiembro { get; set; }

    [EmailAddress(ErrorMessage = "Correo electrónico no válido")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string? CorreoMiembro { get; set; }
}
