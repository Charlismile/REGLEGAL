using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.DTOs;

public class RegistroComiteDTO
{
    // DATOS DEL COMITE
    [Required(ErrorMessage = "El nombre del comité es obligatorio")]
    public string NombreComiteSalud { get; set; }

    public string Comunidad { get; set; }
    
    // Miembro

    [Required]
    public string NombreMiembro { get; set; }

    [Required]
    [RegularExpression(@"^\d{3}-\d{4}-\d{5}$", ErrorMessage = "Formato inválido (000-0000-00000)")]
    public string CedulaMiembro { get; set; }

    [Range(1, int.MaxValue)]
    public int CargoId { get; set; }
    
    // Trámite
    public int TramiteId { get; set; } = 0;
    public string NombreTramite { get; set; }
    public DateTime? CreadaEn { get; set; }
    public string CreadaPor { get; set; }

    // UBICACION
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una región válida")]
    public int RegionSaludId { get; set; } = 0;
    public string NombreRegion { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una provincia válida")]
    public int ProvinciaId { get; set; } = 0;
    public string NombreProvicnicia { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un distrito válido")]
    public int DistritoId { get; set; } = 0;
    public string NombreDistrito { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un corregimiento válido")]
    public int CorregimientoId { get; set; } = 0;
    public string CorregimientoNombre { get; set; } = "";

    // MIEMBROS
    public List<MiembroComiteDTO> Miembros { get; set; } = new();
    
    // ARCHIVOS
    public List<ArchivoDTO> Archivos { get; set; } = new();
    public List<IBrowserFile> ArchivosSubida { get; set; } = new();

    // ID y modo edición
    public int Id { get; set; } = 0;
    public bool EditMode { get; set; } = false;
}

public class MiembroComiteDTO
{
    [Required]
    public string Nombre { get; set; }

    [Required]
    [RegularExpression(@"^\d{3}-\d{4}-\d{5}$", ErrorMessage = "Formato inválido (000-0000-00000)")]
    public string Cedula { get; set; }

    [Range(1, int.MaxValue)]
    public int CargoId { get; set; }
}

public class ArchivoDTO
{
    public string Categoria { get; set; }
    public string NombreOriginal { get; set; }
    public string Ruta { get; set; }
    
}