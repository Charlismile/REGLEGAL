using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models.LegalModels;

public class PersoneriaModel : ComiteModel
{
    public PersoneriaModel()
    {
        TipoTramiteEnum = TipoTramite.Personeria;
    }
    [Required(ErrorMessage = "El nombre del comité es obligatorio")]
    [StringLength(200)]
    public string NombreComiteSalud { get; set; } = "";

    [Required(ErrorMessage = "El nombre de la comunidad es obligatorio")]
    [StringLength(150)]
    public string Comunidad { get; set; } = "";

    [Required(ErrorMessage = "La fecha de creación es obligatoria")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "El número de nota es obligatorio")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string NumeroNota { get; set; } = "";

    // Ubicación
    [Required(ErrorMessage = "La región es obligatoria")]
    public int? RegionSaludId { get; set; }

    [Required(ErrorMessage = "La provincia es obligatoria")]
    public int? ProvinciaId { get; set; }

    [Required(ErrorMessage = "El distrito es obligatorio")]
    public int? DistritoId { get; set; }

    public int? CorregimientoId { get; set; }

    // Miembros
    [MinLength(7, ErrorMessage = "Debe haber exactamente 7 miembros")]
    [MaxLength(7, ErrorMessage = "Debe haber exactamente 7 miembros")]
    public List<MiembroComiteModel> Miembros { get; set; } = new();
}