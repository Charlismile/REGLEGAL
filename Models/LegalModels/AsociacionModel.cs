using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.Models.LegalModels;

public class AsociacionModel : IValidatableObject
{
    // ===============================
    // DATOS DE LA ASOCIACIÓN (TbAsociacion)
    // ===============================
    public int AsociacionId { get; set; } = 0;
    
    public int? RegistroId { get; set; }
    public string? UsuarioId  { get; set; }

    [Required(ErrorMessage = "El nombre de la asociación es obligatorio")]
    [StringLength(150, ErrorMessage = "El nombre no debe exceder los 150 caracteres")]
    public string NombreAsociacion { get; set; } = "";

    [Required(ErrorMessage = "El número de folio es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe ser un número positivo")]
    public int Folio { get; set; }

    [StringLength(300, ErrorMessage = "La actividad no debe exceder los 300 caracteres")]
    public string? Actividad { get; set; }

    // ===============================
    // REPRESENTANTE LEGAL (TbRepresentanteLegal)
    // ===============================
    public int RepLegalId { get; set; } = 0;

    [Required(ErrorMessage = "La cédula del representante es obligatoria")]
    [RegularExpression(@"^(\d{1,2}-\d{1,8}-\d{1,8}|PE-\d{1,8}-\d{1,8}|E-\d{1,8}-\d{1,8}|N-\d{1,8}-\d{1,8}|\d{1,8}AV-\d{1,8}-\d{1,8}|\d{1,8}PI-\d{1,8}-\d{1,8})$", 
        ErrorMessage = "La cédula debe tener un formato válido. Ejemplos: 1-123456-7, PE-123456-7, E-123456-7, N-123456-7, 1AV-123456-7, 1PI-123456-7")]
    [StringLength(20)]
    public string CedulaRepLegal { get; set; } = "";

    [Required(ErrorMessage = "El nombre del representante es obligatorio")]
    [StringLength(100)]
    public string NombreRepLegal { get; set; } = "";
    
    [Required(ErrorMessage = "El apellido del representante es obligatorio")]
    [StringLength(100)]
    public string ApellidoRepLegal { get; set; } = "";

    [Required(ErrorMessage = "El cargo del representante es obligatorio")]
    [StringLength(100)]
    public string CargoRepLegal { get; set; } = "";

    [Required(ErrorMessage = "El teléfono del representante es obligatorio")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Solo puede contener números")]
    [StringLength(20)]
    public string TelefonoRepLegal { get; set; } = "";

    [Required(ErrorMessage = "La dirección del representante es obligatoria")]
    [StringLength(500)]
    public string DireccionRepLegal { get; set; } = "";

    // ===============================
    // APODERADO LEGAL (TbApoderadoLegal)
    // ===============================
    public int ApoAbogadoId { get; set; } = 0;

    [Required(ErrorMessage = "El nombre del apoderado es obligatorio")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "Solo puede contener letras y espacios")]
    [StringLength(200)]
    public string NombreApoAbogado { get; set; } = "";
    
    [Required(ErrorMessage = "El apellido del apoderado es obligatorio")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "Solo puede contener letras y espacios")]
    [StringLength(200)]
    public string ApellidoApoAbogado { get; set; } = "";

    [Required(ErrorMessage = "La cédula del apoderado es obligatoria")]
    [RegularExpression(@"^(\d{1,2}-\d{1,8}-\d{1,8}|PE-\d{1,8}-\d{1,8}|E-\d{1,8}-\d{1,8}|N-\d{1,8}-\d{1,8}|\d{1,8}AV-\d{1,8}-\d{1,8}|\d{1,8}PI-\d{1,8}-\d{1,8})$", 
        ErrorMessage = "La cédula debe tener un formato válido. Ejemplos: 1-123456-7, PE-123456-7, E-123456-7, N-123456-7, 1AV-123456-7, 1PI-123456-7")]
    [StringLength(20)]
    public string CedulaApoAbogado { get; set; } = "";

    [Required(ErrorMessage = "El teléfono del apoderado es obligatorio")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Solo puede contener números")]
    [StringLength(20)]
    public string TelefonoApoAbogado { get; set; } = "";

    [Required(ErrorMessage = "El correo del apoderado es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    [StringLength(200)]
    public string CorreoApoAbogado { get; set; } = "";

    [Required(ErrorMessage = "La dirección del apoderado es obligatoria")]
    [StringLength(500)]
    public string DireccionApoAbogado { get; set; } = "";

    // ===============================
    // FIRMA LEGAL (TbApoderadoFirma)
    // ===============================
    
    [Required(ErrorMessage = "El número de resolución es obligatorio")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string NumeroResolucion { get; set; } = "";
    
    // FIRMA LEGAL (TbApoderadoFirma)
    public bool PerteneceAFirma { get; set; }

    public string NombreFirma { get; set; } = "";

    public string TelefonoFirma { get; set; } = "";

    public string CorreoFirma { get; set; } = "";

    public string DireccionFirma { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PerteneceAFirma)
        {
            if (string.IsNullOrWhiteSpace(NombreFirma))
                yield return new ValidationResult("Nombre de la firma obligatorio", new[] { nameof(NombreFirma) });

            if (string.IsNullOrWhiteSpace(CorreoFirma))
                yield return new ValidationResult("Correo de la firma obligatorio", new[] { nameof(CorreoFirma) });

            if (string.IsNullOrWhiteSpace(TelefonoFirma))
                yield return new ValidationResult("Teléfono de la firma obligatorio", new[] { nameof(TelefonoFirma) });

            if (string.IsNullOrWhiteSpace(DireccionFirma))
                yield return new ValidationResult("Dirección de la firma obligatoria", new[] { nameof(DireccionFirma) });
        }
    }

    // ===============================
    // TRÁMITE / ESTADO / AUDITORÍA
    // ===============================
    public bool EditMode { get; set; } = true;
    public DateTime? CreadaEn { get; set; }
    public string CreadaPor { get; set; } = "";
    public DateTime? FechaResolucion { get; set; }

    // ===============================
    // ARCHIVOS / DOCUMENTOS
    // ===============================
    public List<AArchivoModel> Archivos { get; set; } = new();
    public List<IBrowserFile> DocumentosSubir { get; set; } = new();
}


