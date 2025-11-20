using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models.LegalModels;

public class CambioMiembrosModel
{
    // Campos para auditoría
    public string CreadaPor { get; set; } = "";
    public string UsuarioId { get; set; } = "";

    // Campos para Representante Legal
    [Required(ErrorMessage = "El nombre del representante es obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string? NombreRepLegal { get; set; }
    
    [Required(ErrorMessage = "El apellido del representante es obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string? ApellidoRepLegal { get; set; }
    
    [Required(ErrorMessage = "La cédula del representante es obligatoria")]
    [RegularExpression(@"^(\d{1,2}-\d{1,8}-\d{1,8}|PE-\d{1,8}-\d{1,8}|E-\d{1,8}-\d{1,8}|N-\d{1,8}-\d{1,8}|\d{1,8}AV-\d{1,8}-\d{1,8}|\d{1,8}PI-\d{1,8}-\d{1,8})$", 
        ErrorMessage = "La cédula debe tener un formato válido")]
    [StringLength(20)]
    public string? CedulaRepLegal { get; set; }

    [Required(ErrorMessage = "El teléfono del representante es obligatorio")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Solo puede contener números")]
    [StringLength(20)]
    public string? TelefonoRepLegal { get; set; }

    [Required(ErrorMessage = "La dirección del representante es obligatoria")]
    [StringLength(500)]
    public string? DireccionRepLegal { get; set; }

    // Campos para Apoderado Legal
    [Required(ErrorMessage = "El nombre del apoderado es obligatorio")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "Solo puede contener letras y espacios")]
    [StringLength(200)]
    public string? NombreApoAbogado { get; set; }

    [Required(ErrorMessage = "El apellido del apoderado es obligatorio")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "Solo puede contener letras y espacios")]
    [StringLength(200)]
    public string? ApellidoApoAbogado { get; set; }

    [Required(ErrorMessage = "La cédula del apoderado es obligatoria")]
    [RegularExpression(@"^(\d{1,2}-\d{1,8}-\d{1,8}|PE-\d{1,8}-\d{1,8}|E-\d{1,8}-\d{1,8}|N-\d{1,8}-\d{1,8}|\d{1,8}AV-\d{1,8}-\d{1,8}|\d{1,8}PI-\d{1,8}-\d{1,8})$", 
        ErrorMessage = "La cédula debe tener un formato válido")]
    [StringLength(20)]
    public string? CedulaApoAbogado { get; set; }

    [Required(ErrorMessage = "El teléfono del apoderado es obligatorio")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Solo puede contener números")]
    [StringLength(20)]
    public string? TelefonoApoAbogado { get; set; }

    [Required(ErrorMessage = "El correo del apoderado es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    [StringLength(200)]
    public string? CorreoApoAbogado { get; set; }

    [Required(ErrorMessage = "La dirección del apoderado es obligatoria")]
    [StringLength(500)]
    public string? DireccionApoAbogado { get; set; }

    // Campos para Firma
    public bool PerteneceAFirma { get; set; }

    [StringLength(200)]
    public string? NombreFirma { get; set; }

    [RegularExpression(@"^\d+$", ErrorMessage = "Solo puede contener números")]
    [StringLength(20)]
    public string? TelefonoFirma { get; set; }

    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    [StringLength(200)]
    public string? CorreoFirma { get; set; }

    [StringLength(500)]
    public string? DireccionFirma { get; set; }

    // Propiedades para mostrar (no se guardan)
    public string? CargoRepLegal { get; set; } = "Presidente";
    public int? Folio { get; set; }
    public string? NombreAsociacion { get; set; }
}