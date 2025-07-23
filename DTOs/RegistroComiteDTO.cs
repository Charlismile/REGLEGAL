using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.DTOs;

public class RegistroComiteDTO
{
    // DATOS DEL COMITE
    [Required(ErrorMessage = "El nombre del comité es obligatorio")]
    public string NombreComiteSalud { get; set; }

    public string Comunidad { get; set; }

    // TRÁMITE
    [Required(ErrorMessage = "El tipo de trámite es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de trámite válido")]
    public int TramiteId { get; set; }

    public string NombreTramite { get; set; }

    // UBICACIÓN GEOGRÁFICA
    [Required(ErrorMessage = "La región de salud es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una región válida")]
    public int RegionSaludId { get; set; }

    public string NombreRegion { get; set; } = "";

    [Required(ErrorMessage = "La provincia es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una provincia válida")]
    public int ProvinciaId { get; set; }

    public string NombreProvincia { get; set; } = ""; // Corregido: NombreProvicnicia → NombreProvincia

    [Required(ErrorMessage = "El distrito es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un distrito válido")]
    public int DistritoId { get; set; }

    public string NombreDistrito { get; set; } = "";

    [Required(ErrorMessage = "El corregimiento es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un corregimiento válido")]
    public int CorregimientoId { get; set; }

    public string CorregimientoNombre { get; set; } = "";

    // MIEMBROS DEL COMITÉ
    // ✅ Eliminados: NombreMiembro, CedulaMiembro, CargoId (redundantes)
    [MinLength(1, ErrorMessage = "Debe agregar al menos un miembro al comité")]
    public List<MiembroComiteDTO> Miembros { get; set; } = new();

    // ARCHIVOS
    public List<ArchivoDTO> Archivos { get; set; } = new();
    public List<IBrowserFile> ArchivoSubida { get; set; } = new();

    // ID Y ESTADO
    public int Id { get; set; } = 0; // DComiteId
    public bool EditMode { get; set; } = false;

    // Campos de auditoría (solo lectura, llenados por el backend)
    public DateTime? CreadaEn { get; set; }
    public string CreadaPor { get; set; }
}

public class MiembroComiteDTO
{
    [Required(ErrorMessage = "El nombre del miembro es obligatorio")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "La cédula es obligatoria")]
    [RegularExpression(@"^\d{1,2}-\d{1,8}-\d{1,8}$", 
        ErrorMessage = "Formato inválido. Ejemplo: 1-123456-7")]
    public string Cedula { get; set; }

    [Required(ErrorMessage = "El cargo es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cargo válido")]
    public int CargoId { get; set; }
    
    public string NombreCargo { get; set; } = "";

    // Opcional: agregar si necesitas marcar un miembro principal
    // public bool EsPrincipal { get; set; }
}

public class ArchivoDTO
{
    [Required(ErrorMessage = "La categoría es obligatoria")]
    public string Categoria { get; set; }

    [Required(ErrorMessage = "El nombre original es obligatorio")]
    public string Archivo { get; set; }

    [Required(ErrorMessage = "La URL es obligatoria")]
    public string Url { get; set; } // Cambiado de 'Ruta' a 'Url' para coincidir con TbArchivos
}