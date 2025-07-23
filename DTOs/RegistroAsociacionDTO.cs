using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.DTOs;

public class RegistroAsociacionDTO
{
    // DATOS DE LA ASOCIACIÓN
    [Required(ErrorMessage = "El nombre de la asociación es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres.")]
    public string NombreAsociacion { get; set; } = "";

    [Required(ErrorMessage = "El folio es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El folio debe ser un número válido mayor que 0.")]
    public int Folio { get; set; }

    [Required(ErrorMessage = "La actividad es obligatoria.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "La actividad solo puede contener letras y espacios.")]
    [StringLength(1000, ErrorMessage = "La actividad no puede exceder 1000 caracteres.")]
    public string Actividad { get; set; } = "";

    // REPRESENTANTE LEGAL
    [Required(ErrorMessage = "El nombre del representante legal es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre del representante solo puede contener letras y espacios.")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres.")]
    public string NombreRepLegal { get; set; } = "";

    [Required(ErrorMessage = "La cédula del representante legal es obligatoria.")]
    [RegularExpression(@"^(\\d{1,2}-\\d{1,8}-\\d{1,8}|PE-\\d{1,8}-\\d{1,8}|E-\\d{1,8}-\\d{1,8}|N-\\d{1,8}-\\d{1,8}|\\d{1,8}AV-\\d{1,8}-\\d{1,8}|\\d{1,8}PI-\\d{1,8}-\\d{1,8})$",
        ErrorMessage = "Formato de cédula inválido. Ejemplos: 1-123456-7, PE-123456-7, E-123456-7, N-123456-7, 1AV-123456-7, 1PI-123456-7")]
    [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres.")]
    public string CedulaRepLegal { get; set; } = "";

    [Required(ErrorMessage = "El cargo del representante legal es obligatorio.")]
    [StringLength(100, ErrorMessage = "El cargo no puede exceder 100 caracteres.")]
    public string CargoRepLegal { get; set; } = "";

    [Required(ErrorMessage = "El teléfono del representante legal es obligatorio.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "El teléfono solo puede contener números.")]
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres.")]
    public string TelefonoRepLegal { get; set; } = "";

    [Required(ErrorMessage = "La dirección del representante legal es obligatoria.")]
    [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres.")]
    public string DireccionRepLegal { get; set; } = "";

    // APODERADO LEGAL
    [Required(ErrorMessage = "El nombre del apoderado legal es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre del apoderado solo puede contener letras y espacios.")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres.")]
    public string NombreApoAbogado { get; set; } = "";

    [Required(ErrorMessage = "La cédula del apoderado legal es obligatoria.")]
    [RegularExpression(@"^(\\d{1}-\\d{6,7}-\\d{1}|PE-\\d{6}-\\d{1}|E-\\d{6}-\\d{1}|N-\\d{6}-\\d{1}|\\d{1}AV-\\d{6}-\\d{1}|\\d{1}PI-\\d{6}-\\d{1})$",
        ErrorMessage = "Formato de cédula inválido. Ejemplos: 1-123456-7, PE-123456-7, E-123456-7, N-123456-7, 1AV-123456-7, 1PI-123456-7")]
    [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres.")]
    public string CedulaApoAbogado { get; set; } = "";

    [Required(ErrorMessage = "El teléfono del apoderado legal es obligatorio.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "El teléfono solo puede contener números.")]
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres.")]
    public string TelefonoApoAbogado { get; set; } = "";

    [Required(ErrorMessage = "El correo del apoderado legal es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(200, ErrorMessage = "El correo no puede exceder 200 caracteres.")]
    public string CorreoApoAbogado { get; set; } = "";

    [Required(ErrorMessage = "La dirección del apoderado legal es obligatoria.")]
    [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres.")]
    public string DireccionApoAbogado { get; set; } = "";

    // FIRMA DE ABOGADOS
    [Required(ErrorMessage = "El nombre de la firma es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre de la firma solo puede contener letras y espacios.")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres.")]
    public string NombreFirma { get; set; } = "";

    [Required(ErrorMessage = "El teléfono de la firma es obligatorio.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "El teléfono solo puede contener números.")]
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres.")]
    public string TelefonoFirma { get; set; } = "";

    [Required(ErrorMessage = "El correo de la firma es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(200, ErrorMessage = "El correo no puede exceder 200 caracteres.")]
    public string CorreoFirma { get; set; } = "";

    [Required(ErrorMessage = "La dirección de la firma es obligatoria.")]
    [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres.")]
    public string DireccionFirma { get; set; } = "";

    // TRÁMITE
    [Required(ErrorMessage = "El tipo de trámite es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de trámite válido.")]
    public int TipoTramiteId { get; set; }

    public string NombreTramite { get; set; } = "";

    // ID Y ESTADO
    public int AsociacionId { get; set; } = 0;
    public bool EditMode { get; set; } = false;

    // CAMPOS DE AUDITORÍA (solo lectura, backend)
    public DateTime? CreadaEn { get; set; }
    public string CreadaPor { get; set; } = "";

    // DOCUMENTOS
    public List<DocumentoDTO> Documentos { get; set; } = new();
    public List<IBrowserFile> DocumentoSubida { get; set; } = new();
}

public class DocumentoDTO
{
    [Required(ErrorMessage = "La categoría es obligatoria")]
    public string Categoria { get; set; } = "";

    [Required(ErrorMessage = "El nombre original es obligatorio")]
    [StringLength(500)]
    public string Documento { get; set; } = "";

    [Required(ErrorMessage = "La URL es obligatoria")]
    [StringLength(1000)]
    public string Ruta { get; set; } = ""; // Cambiado de 'URL' a 'Url' para coincidir con TbArchivos
}