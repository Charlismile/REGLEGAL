using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models.LegalModels;

public class MiembroComiteModel
{
    public int MiembroId { get; set; }
    
    [Required(ErrorMessage = "El nombre del miembro es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string NombreMiembro { get; set; } = "";
    
    [Required(ErrorMessage = "El apellido del miembro es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string ApellidoMiembro { get; set; } = "";
    
    [Required(ErrorMessage = "La cédula del miembro es obligatoria")]
    [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
    public string CedulaMiembro { get; set; } = "";
    
    [Required(ErrorMessage = "El cargo del miembro es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cargo")]
    public int CargoId { get; set; }
    
    public int ComiteId { get; set; }
}