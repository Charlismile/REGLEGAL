using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.DTOs;

public class DocumentoDTO
{
    [Required(ErrorMessage = "La categoría es obligatoria")]
    public string Categoria { get; set; } = "";

    [Required(ErrorMessage = "El nombre original es obligatorio")]
    [StringLength(500)]
    public string NombreOriginal { get; set; } = "";

    [Required(ErrorMessage = "La URL es obligatoria")]
    [StringLength(1000)]
    public string Url { get; set; } = "";

    public string NombreArchivoGuardado { get; set; } = "";
    public DateTime FechaSubida { get; set; } = DateTime.Now;
    public int Version { get; set; } = 1;
    public bool IsActivo { get; set; } = true;

    // Para carga de archivos
    public IBrowserFile ArchivoFisico { get; set; }
}