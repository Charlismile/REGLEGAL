using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.Models.LegalModels;

public enum TipoTramite { Personeria = 1, CambioDirectiva = 2, JuntaInterventora = 3 }
public class ComiteModel : IValidatableObject
{
    public int ComiteId { get; set; }
    // TRÁMITE
    [Required(ErrorMessage = "El tipo de trámite es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de trámite válido")]
    public TipoTramite TipoTramiteEnum { get; set; }
    
    // Campos automáticos (los llenará el servicio)
    public int NumeroTramite => TipoTramiteEnum switch
    {
        TipoTramite.Personeria => 1,
        TipoTramite.CambioDirectiva => 2,
        TipoTramite.JuntaInterventora => 3,
        _ => 0
    };
    
    public DateTime CreadaEn { get; set; }
    
    public string CreadaPor { get; set; } = null!;
    
    
    [Required(ErrorMessage = "El nombre del comité es obligatorio")]
    public string NombreComiteSalud { get; set; } = null!;
    
    public string? Comunidad { get; set; }

    public int? RegionSaludId { get; set; }
    public int? ProvinciaId { get; set; }

    public int? DistritoId { get; set; }

    public int? CorregimientoId { get; set; }

    // reghistorial
    public int NumRegCoSecuencia { get; set; }
    public int NomRegCoAnio { get; set; }
    public int NumRegCoMes { get; set; }
    public string NumRegCoCompleta => $"{NumRegCoSecuencia}/{NomRegCoAnio}/{NumRegCoMes:D2}";
    // MIEMBROS
    [MinLength(1, ErrorMessage = "Debe agregar al menos 1 miembro")]
    public List<MiembroComiteModel> Miembros { get; set; } = new();
    
    public List<MiembroComiteModel> MiembrosInterventores { get; set; } = new();
    
    public List<ArchivoModel> Archivos { get; set; } = new();

    public List<IBrowserFile> DocumentosSubir { get; set; } = new();
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!ProvinciaId.HasValue)
            yield return new ValidationResult("La provincia es obligatoria.", new[] { nameof(ProvinciaId) });
        else
        {
            if (!RegionSaludId.HasValue)
                yield return new ValidationResult("La región es obligatoria.", new[] { nameof(RegionSaludId) });
            else
            {
                if (!DistritoId.HasValue)
                    yield return new ValidationResult("El distrito es obligatorio.", new[] { nameof(DistritoId) });
                else
                {
                    if (!CorregimientoId.HasValue)
                        yield return new ValidationResult("El corregimiento es obligatorio.", new[] { nameof(CorregimientoId) });
                }
            }
        }
    }
}