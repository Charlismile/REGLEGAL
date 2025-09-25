using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models.LegalModels;

public class CargoModel
{
    public int CargoId { get; set; }   // PK del cargo en la tabla de BD

    [Required(ErrorMessage = "El nombre del cargo es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string NombreCargo { get; set; } = "";
    
}