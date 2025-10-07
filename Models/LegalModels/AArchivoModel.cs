namespace REGISTROLEGAL.Models.LegalModels;

public class AArchivoModel
{
    public int AsociacionArchivoId { get; set; }
    public string NombreArchivo { get; set; } = "";
    public string RutaArchivo { get; set; } = "";
    public DateTime SubidoEn { get; set; }
}