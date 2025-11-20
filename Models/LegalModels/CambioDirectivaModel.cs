using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models.LegalModels;

public class CambioDirectivaModel : ComiteModel
{
    public CambioDirectivaModel()
    {
        TipoTramiteEnum = TipoTramite.CambioDirectiva;
    }
    [Required(ErrorMessage = "Debe seleccionar un comité existente")]
    public int ComiteBaseId { get; set; }
    
    // Datos heredados (solo lectura)
    public string NombreComiteSalud { get; set; } = "";
    public string Comunidad { get; set; } = "";
    public int? RegionSaludId { get; set; }
    public int? ProvinciaId { get; set; }
    public int? DistritoId { get; set; }
    public int? CorregimientoId { get; set; }

    [Required(ErrorMessage = "La fecha de elección es obligatoria")]
    public DateTime FechaEleccion { get; set; } = DateTime.Now;

    // Nueva junta directiva
    [MinLength(7, ErrorMessage = "Debe haber exactamente 7 miembros")]
    [MaxLength(7, ErrorMessage = "Debe haber exactamente 7 miembros")]
    public List<MiembroComiteModel> Miembros { get; set; } = new();
}