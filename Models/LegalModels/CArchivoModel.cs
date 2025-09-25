namespace REGISTROLEGAL.Models.LegalModels;

public class CArchivoModel
{
    public int ComiteArchivoId { get; set; }
    public int ComiteId { get; set; }

    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public DateTime SubidoEn { get; set; }
}