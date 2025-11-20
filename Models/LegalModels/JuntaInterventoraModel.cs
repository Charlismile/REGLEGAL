using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models.LegalModels;

public class JuntaInterventoraModel : ComiteModel
{
    public JuntaInterventoraModel()
    {
        TipoTramiteEnum = TipoTramite.JuntaInterventora;
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

    // Miembros actuales (solo lectura)
    public List<MiembroComiteModel> MiembrosActuales { get; set; } = new();

    // Interventores
    [MinLength(2, ErrorMessage = "Debe haber exactamente 2 interventores")]
    [MaxLength(2, ErrorMessage = "Debe haber exactamente 2 interventores")]
    public List<MiembroComiteModel> Interventores { get; set; } = new();
}