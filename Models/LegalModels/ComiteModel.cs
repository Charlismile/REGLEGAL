using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.Models.LegalModels;

public enum TipoTramite 
{ 
    Personeria = 1, 
    CambioDirectiva = 2, 
    JuntaInterventora = 3 
}

public class ComiteModel
{
    public int ComiteId { get; set; }

    // TRÁMITE
    [Required(ErrorMessage = "El tipo de trámite es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de trámite válido")]
    public TipoTramite TipoTramiteEnum { get; set; }

    [Required]
    public string CreadaPor { get; set; } = null!;

    [Required(ErrorMessage = "El nombre del comité es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres")]
    public string NombreComiteSalud { get; set; } = null!;

    [Required(ErrorMessage = "El nombre de la comunidad es obligatorio")]
    [StringLength(150, ErrorMessage = "El nombre de la comunidad no puede superar los 150 caracteres")]
    public string? Comunidad { get; set; }

    // UBICACIÓN
    public int? RegionSaludId { get; set; }
    public int? ProvinciaId { get; set; }
    public int? DistritoId { get; set; }
    public int? CorregimientoId { get; set; }

    // MIEMBROS
    [MinLength(1, ErrorMessage = "Debe agregar al menos 1 miembro")]
    public List<MiembroComiteModel> Miembros { get; set; } = new();

    public List<MiembroComiteModel> MiembrosInterventores { get; set; } = new();

    // ARCHIVOS DEL COMITÉ
    public List<CArchivoModel> Archivos { get; set; } = new();

    // Para carga temporal de documentos en formulario (no mapeado en DB directamente)
    public List<IBrowserFile> DocumentosSubir { get; set; } = new();

    // HISTORIAL / DETALLES
    public List<DetalleRegComiteModel> Historial { get; set; } = new();
}