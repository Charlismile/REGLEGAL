using System.ComponentModel.DataAnnotations;

public class RegistroComiteDTO
{
    // DATOS DEL COMITE
    [Required(ErrorMessage = "El nombre del comité es obligatorio")]
    public string NombreComiteSalud { get; set; }

    public string Comunidad { get; set; }
    
    [Required] public string NombreMiembro { get; set; }

    [Required]
    [RegularExpression(@"^\d{3}-\d{4}-\d{5}$", ErrorMessage = "Formato inválido (000-0000-00000)")]
    public string CedulaMiembro { get; set; }

    [Range(1, int.MaxValue)] public int CargoId { get; set; }


    public int Id { get; set; } = 0;
    
    public int TramiteId  { get; set; } = 0;
    
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
    
    // NEW OR EDIT
    public bool EditMode { get; set; } = false;
}