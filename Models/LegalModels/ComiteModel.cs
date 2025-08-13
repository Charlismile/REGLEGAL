using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models.LegalModels;

public enum TipoTramite { Personeria = 1, CambioDirectiva = 2, JuntaInterventora = 3 }
public class ComiteModel
{
    // TRÁMITE
    [Required(ErrorMessage = "El tipo de trámite es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de trámite válido")]
    public TipoTramite TipoTramiteEnum { get; set; }
    
    public DateTime CreadaEn { get; set; }

    //Miembros

    [Required(ErrorMessage = "El nombre del miembro es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string NombreMiembro { get; set; } = "";

    [Required(ErrorMessage = "El cargo del miembro es obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string NombreCargoMiembro { get; set; } = "";

    [Required(ErrorMessage = "El número de cédula es obligatorio")]
    [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
    public string CedulaMiembro { get; set; } = "";

    [Required(ErrorMessage = "El número de teléfono es obligatorio")]
    [Phone(ErrorMessage = "Número de teléfono no válido")]
    public string TelefonoMiembro { get; set; } = "";

    [EmailAddress(ErrorMessage = "Correo electrónico no válido")]
    public string? CorreoMiembro { get; set; }

    
    [Required(ErrorMessage = "El nombre del comité es obligatorio")]
    public string NombreComiteSalud { get; set; } = null!;
    
    public string? Comunidad { get; set; }

    [Required(ErrorMessage = "La región de salud es obligatoria")]
    public int? RegionSaludId { get; set; }

    [Required(ErrorMessage = "La provincia es obligatoria")]
    public int? ProvinciaId { get; set; }

    [Required(ErrorMessage = "El distrito es obligatorio")]
    public int? DistritoId { get; set; }

    public int? CorregimientoId { get; set; }

}