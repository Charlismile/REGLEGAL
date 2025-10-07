using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.Models.LegalModels;

public class CArchivoModel
{
    public int ComiteArchivoId { get; set; }

    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public DateTime SubidoEn { get; set; } = DateTime.Now;
    
    public IBrowserFile? Archivo { get; set; }
}