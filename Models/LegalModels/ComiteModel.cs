using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.Models.LegalModels;

public enum TipoTramite 
{ 
    Personeria = 1, 
    CambioDirectiva = 2, 
    JuntaInterventora = 3 
}

// 🔹 MODELO BASE (para datos comunes)
public class ComiteModel
{
    public int ComiteId { get; set; }
    public string? UsuarioId { get; set; }
    public string CreadaPor { get; set; } = "";
    public int EstadoId { get; set; }
    
    [Required(ErrorMessage = "El número de resolución es obligatorio")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string NumeroResolucion { get; set; } = "";

    [Required(ErrorMessage = "La fecha de resolución es obligatoria")]
    public DateTime FechaResolucion { get; set; } = DateTime.Now;

    public List<IBrowserFile> DocumentosSubir { get; set; } = new();
    public List<CArchivoModel> Archivos { get; set; } = new();
    
    // Campos comunes para todos los trámites
    public TipoTramite TipoTramiteEnum { get; set; }
    
    [Required(ErrorMessage = "El nombre del comité es obligatorio")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
    public string NombreComiteSalud { get; set; } = "";
    
    [Required(ErrorMessage = "El nombre de la comunidad es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string Comunidad { get; set; } = "";
    
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaEleccion { get; set; }
    
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string NumeroNota { get; set; } = "";
    
    public int? RegionSaludId { get; set; }
    public int? ProvinciaId { get; set; }
    public int? DistritoId { get; set; }
    public int? CorregimientoId { get; set; }
    
    public List<MiembroComiteModel> Miembros { get; set; } = new();
    public List<MiembroComiteModel> MiembrosInterventores { get; set; } = new();
    public List<DetalleRegComiteModel> Historial { get; set; } = new();
    public IBrowserFile? CedulaFile { get; set; }
    public string? CedulaPreviewUrl { get; set; }
    public IBrowserFile? PasaporteFile { get; set; }
    public string? PasaportePreviewUrl { get; set; }
    public int? ComiteBaseId { get; set; }
}