using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.DTOs;

public class RegistroDto
{
    #region Comites
    // DATOS DEL COMITÉ (TbDatosComite)
    public int ComiteId { get; set; }
    
    [Required(ErrorMessage = "El nombre del comité es obligatorio")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
    public string NombreComiteSalud { get; set; } = "";

    [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
    public string Comunidad { get; set; } = "";

    // FECHA DE CREACIÓN DEL COMITÉ
    public DateTime? FechaCreacionComite { get; set; }
    

    // TRÁMITE
    [Required(ErrorMessage = "El tipo de trámite es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de trámite válido")]
    public int TipoTramiteId { get; set; }

    // MIEMBROS DEL COMITÉ
    public int DMiembroId { get; set; }
    
    [MinLength(1, ErrorMessage = "Debe registrar al menos un miembro del comité")]
    public List<RegistroDto> Miembros { get; set; } = new();

    // DOCUMENTOS
    public List<RegistroDto> Documentos { get; set; } = new();

    // ID Y ESTADO
    public int DComiteId { get; set; } = 0;

    //Miembros
    public int CargoId { get; set; }

    [Required(ErrorMessage = "El nombre del miembro es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El cargo del miembro es obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string NombreCargo { get; set; } = "";

    [Required(ErrorMessage = "El número de cédula es obligatorio")]
    [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
    public string Cedula { get; set; } = "";

    [Required(ErrorMessage = "El número de teléfono es obligatorio")]
    [Phone(ErrorMessage = "Número de teléfono no válido")]
    public string Telefono { get; set; } = "";

    [EmailAddress(ErrorMessage = "Correo electrónico no válido")]
    public string? Correo { get; set; }
    

    #endregion
    
    #region Asociaciones
    // ===============================
    // DATOS DE LA ASOCIACIÓN (TbAsociacion)
    // ===============================

    public int AsociacionId { get; set; } = 0;

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
    [RegularExpression(@"^\d{1,2}-\d{1,8}-\d{1,8}$", ErrorMessage = "Formato inválido: 1-123456-7")]
    [StringLength(20)]
    public string CedulaRepLegal { get; set; } = "";

    [Required(ErrorMessage = "El nombre del representante es obligatorio")]
    [StringLength(100)]
    public string NombreRepLegal { get; set; } = "";

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

    [Required(ErrorMessage = "La cédula del apoderado es obligatoria")]
    [RegularExpression(@"^\d{1,2}-\d{1,8}-\d{1,8}$", ErrorMessage = "Formato inválido: 1-123456-7")]
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

    public bool PerteneceAFirma { get; set; } = false;

    public int? ApoderadoFirmaId { get; set; }

    [StringLength(200)]
    public string NombreFirma { get; set; } = "";

    [RegularExpression(@"^\d+$", ErrorMessage = "Solo puede contener números")]
    [StringLength(20)]
    public string TelefonoFirma { get; set; } = "";

    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    [StringLength(200)]
    public string CorreoFirma { get; set; } = "";

    [StringLength(500)]
    public string DireccionFirma { get; set; } = "";

    // ===============================
    // DETALLE REGISTRO (TbDetalleRegAsociacion)
    // ===============================

    public int NumRegAsecuencia { get; set; }

    public int NomRegAanio { get; set; }

    public int NumRegAmes { get; set; }

    public string? NumRegAcompleta { get; set; }
    

    // ===============================
    // TRÁMITE / ESTADO / AUDITORÍA
    // ===============================

    [Required(ErrorMessage = "El tipo de trámite es obligatorio")]
    [Range(1, int.MaxValue)]
    // public int TipoTramiteId { get; set; }

    public bool EditMode { get; set; } = false;

    public DateTime? CreadaEn { get; set; }

    public string CreadaPor { get; set; } = "";
    

    #endregion
    
    #region Documentos
    [Required(ErrorMessage = "La categoría es obligatoria")]
    public string Categoria { get; set; } = "";

    [Required(ErrorMessage = "El nombre original es obligatorio")]
    [StringLength(500)]
    public string NombreOriginal { get; set; } = "";

    [Required(ErrorMessage = "La URL es obligatoria")]
    [StringLength(1000)]
    public string Url { get; set; } = "";

    public string NombreArchivoGuardado { get; set; } = "";
    public DateTime FechaSubida { get; set; } = DateTime.Now;
    public int Version { get; set; } = 1;
    public bool IsActivo { get; set; } = true;

    // Para carga de archivos
    public IBrowserFile ArchivoFisico { get; set; }

    public List<IBrowserFile> DocumentosSubir { get; set; } = new();

    #endregion

    #region Ubicaciones

    // UBICACIÓN GEOGRÁFICA
    [Required(ErrorMessage = "La región de salud es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una región válida")]
    public int RId { get; set; }
    public string? NombreRegion { get; set; }

    [Required(ErrorMessage = "La provincia es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una provincia válida")]
    public int PId { get; set; }
    public string? NombreProvincia { get; set; }

    [Required(ErrorMessage = "El distrito es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un distrito válido")]
    public int DId { get; set; }
    public string? NombreDistrito { get; set; }

    [Required(ErrorMessage = "El corregimiento es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un corregimiento válido")]
    public int CId { get; set; }
    public string? NombreCorregimiento { get; set; }

    #endregion
}