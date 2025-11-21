using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models.LegalModels;

public class CambioDirectivaModel : ComiteModel
{
    public CambioDirectivaModel()
    {
        TipoTramiteEnum = TipoTramite.CambioDirectiva;
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

    // Nueva junta directiva - validación específica
    [MinLength(7, ErrorMessage = "Debe haber exactamente 7 miembros")]
    [MaxLength(7, ErrorMessage = "Debe haber exactamente 7 miembros")]
    public new List<MiembroComiteModel> Miembros 
    { 
        get => base.Miembros; 
        set => base.Miembros = value; 
    }
}