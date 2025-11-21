using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models.LegalModels;

public class PersoneriaModel : ComiteModel
{
    public PersoneriaModel()
    {
        TipoTramiteEnum = TipoTramite.Personeria;
        
    }
    
    [Required(ErrorMessage = "La fecha de creación es obligatoria")]
    public new DateTime? FechaCreacion 
    { 
        get => base.FechaCreacion; 
        set => base.FechaCreacion = value; 
    }

    [Required(ErrorMessage = "El número de nota es obligatorio")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public new string NumeroNota 
    { 
        get => base.NumeroNota; 
        set => base.NumeroNota = value; 
    }

    // Ubicación - validaciones específicas
    [Required(ErrorMessage = "La región es obligatoria")]
    public new int? RegionSaludId 
    { 
        get => base.RegionSaludId; 
        set => base.RegionSaludId = value; 
    }

    [Required(ErrorMessage = "La provincia es obligatoria")]
    public new int? ProvinciaId 
    { 
        get => base.ProvinciaId; 
        set => base.ProvinciaId = value; 
    }

    [Required(ErrorMessage = "El distrito es obligatorio")]
    public new int? DistritoId 
    { 
        get => base.DistritoId; 
        set => base.DistritoId = value; 
    }

    // Miembros - validación específica para Personería
    [MinLength(7, ErrorMessage = "Debe haber exactamente 7 miembros")]
    [MaxLength(7, ErrorMessage = "Debe haber exactamente 7 miembros")]
    public new List<MiembroComiteModel> Miembros 
    { 
        get => base.Miembros; 
        set => base.Miembros = value; 
    }
}