using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models.LegalModels;

public class JuntaInterventoraModel : ComiteModel
{
    public JuntaInterventoraModel()
    {
        TipoTramiteEnum = TipoTramite.JuntaInterventora;
    }

    [Required(ErrorMessage = "Debe seleccionar un comité existente")]
    public new int ComiteBaseId 
    { 
        get => base.ComiteBaseId ?? 0; 
        set => base.ComiteBaseId = value; 
    }

    // Datos heredados (propiedades de solo lectura para mostrar información del comité base)
    public string NombreComiteBase { get; set; } = "";
    public string ComunidadBase { get; set; } = "";
    public string RegionBase { get; set; } = "";
    public string ProvinciaBase { get; set; } = "";
    public string DistritoBase { get; set; } = "";
    public string CorregimientoBase { get; set; } = "";

    [Required(ErrorMessage = "La fecha de elección es obligatoria")]
    public new DateTime? FechaEleccion 
    { 
        get => base.FechaEleccion; 
        set => base.FechaEleccion = value; 
    }

    // Miembros actuales del comité base (solo lectura)
    public List<MiembroComiteModel> MiembrosActuales { get; set; } = new();

    // Interventores - usando MiembrosInterventores del base con validación específica
    [MinLength(2, ErrorMessage = "Debe haber exactamente 2 interventores")]
    [MaxLength(2, ErrorMessage = "Debe haber exactamente 2 interventores")]
    public new List<MiembroComiteModel> MiembrosInterventores 
    { 
        get => base.MiembrosInterventores; 
        set => base.MiembrosInterventores = value; 
    }
}