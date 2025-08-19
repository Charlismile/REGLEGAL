using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.Models.LegalModels;

public enum TipoTramite { Personeria = 1, CambioDirectiva = 2, JuntaInterventora = 3 }
public class ComiteModel
{
    public int ComiteId { get; set; }
    // TRÁMITE
    [Required(ErrorMessage = "El tipo de trámite es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de trámite válido")]
    public TipoTramite TipoTramiteEnum { get; set; }
    
    public int NumeroTramite { get; set; } 
    
    public DateTime CreadaEn { get; set; }
    
    public string CreadaPor { get; set; } = null!;
    
    
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

    // reghistorial
    public int NumRegCoSecuencia { get; set; }
    public int NomRegCoAnio { get; set; }
    public int NumRegCoMes { get; set; }
    public string? NumRegCoCompleta { get; set; }
    // MIEMBROS
    [MinLength(1, ErrorMessage = "Debe agregar al menos 1 miembro")]
    public List<MiembroComiteModel> Miembros { get; set; } = new();
    
    public List<MiembroComiteModel> MiembrosInterventores { get; set; } = new();
    
    public List<ArchivoModel> Archivos { get; set; } = new();

    public List<IBrowserFile> DocumentosSubir { get; set; } = new();
}