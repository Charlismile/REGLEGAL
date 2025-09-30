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

    [Required(ErrorMessage = "El tipo de trámite es obligatorio")]
    public TipoTramite TipoTramiteEnum { get; set; }
    public string CreadaPor { get; set; } = null!;

    [Required(ErrorMessage = "El nombre del comité es obligatorio")]
    [StringLength(200)]
    public string NombreComiteSalud { get; set; } = null!;

    [Required(ErrorMessage = "El nombre de la comunidad es obligatorio")]
    [StringLength(150)]
    public string? Comunidad { get; set; }

    // 📌 Fechas y resolución
    [Required(ErrorMessage = "La fecha de creación es obligatoria")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "La fecha de elección es obligatoria")]
    public DateTime FechaEleccion { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "El número de resolución es obligatorio")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string NumeroResolucion { get; set; } = "";

    [Required(ErrorMessage = "La fecha de resolución es obligatoria")]
    public DateTime FechaResolucion { get; set; } = DateTime.Now;

    // 📌 Ubicación
    public int? RegionSaludId { get; set; }
    public int? ProvinciaId { get; set; }
    public int? DistritoId { get; set; }
    public int? CorregimientoId { get; set; }

    // 📌 Miembros
    [MinLength(1, ErrorMessage = "Debe agregar al menos 1 miembro")]
    public List<MiembroComiteModel> Miembros { get; set; } = new();

    public List<MiembroComiteModel> MiembrosInterventores { get; set; } = new();

    // 📌 Archivos
    public List<CArchivoModel> Archivos { get; set; } = new();
    public List<IBrowserFile> DocumentosSubir { get; set; } = new();

    // 📌 Historial
    public List<DetalleRegComiteModel> Historial { get; set; } = new();
}
