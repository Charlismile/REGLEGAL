using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.DTOs;

public class RegistroComiteDto
{
    // DATOS DEL COMITÉ (TbDatosComite)
    [Required(ErrorMessage = "El nombre del comité es obligatorio")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
    public string NombreComiteSalud { get; set; } = "";

    [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
    public string Comunidad { get; set; } = "";

    // FECHA DE CREACIÓN DEL COMITÉ
    public DateTime? FechaCreacionComite { get; set; }

    // UBICACIÓN GEOGRÁFICA
    [Required(ErrorMessage = "La región de salud es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una región válida")]
    public int RegionSaludId { get; set; }

    [Required(ErrorMessage = "La provincia es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una provincia válida")]
    public int ProvinciaId { get; set; }

    [Required(ErrorMessage = "El distrito es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un distrito válido")]
    public int DistritoId { get; set; }

    [Required(ErrorMessage = "El corregimiento es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un corregimiento válido")]
    public int CorregimientoId { get; set; }

    // TRÁMITE
    [Required(ErrorMessage = "El tipo de trámite es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de trámite válido")]
    public int TipoTramiteId { get; set; }

    // MIEMBROS DEL COMITÉ
    [MinLength(1, ErrorMessage = "Debe registrar al menos un miembro del comité")]
    public List<MiembroComiteDTO> Miembros { get; set; } = new();

    // DOCUMENTOS
    public List<DocumentoDTO> Documentos { get; set; } = new();
    public List<IBrowserFile> DocumentosSubir { get; set; } = new();

    // ID Y ESTADO
    public int DComiteId { get; set; } = 0;
    public bool EditMode { get; set; } = false;

    // AUDITORÍA
    public DateTime? CreadaEn { get; set; }
    public string CreadaPor { get; set; } = "";
}

public class MiembroComiteDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del miembro es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El cargo del miembro es obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string Cargo { get; set; } = "";

    [Required(ErrorMessage = "El número de cédula es obligatorio")]
    [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
    public string Cedula { get; set; } = "";

    [Required(ErrorMessage = "El número de teléfono es obligatorio")]
    [Phone(ErrorMessage = "Número de teléfono no válido")]
    public string Telefono { get; set; } = "";

    [EmailAddress(ErrorMessage = "Correo electrónico no válido")]
    public string? Correo { get; set; }
}

public class DocumentoDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El tipo de documento es obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string TipoDocumento { get; set; } = "";

    [Required(ErrorMessage = "El nombre del archivo es obligatorio")]
    [StringLength(255, ErrorMessage = "Máximo 255 caracteres")]
    public string NombreArchivo { get; set; } = "";

    public string Ruta { get; set; } = "";
    public DateTime FechaSubida { get; set; } = DateTime.Now;
}

public class ComiteListadoDTO
{
    public int DComiteId { get; set; }
    public string NombreComiteSalud { get; set; } = "";
    public string NumRegCompleta { get; set; } = "";
    public DateTime CreadaEn { get; set; }
    public string Estado { get; set; } = "";
}
