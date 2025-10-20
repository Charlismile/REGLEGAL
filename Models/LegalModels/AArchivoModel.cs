namespace REGISTROLEGAL.Models.LegalModels;

public class AArchivoModel
{
    public int AsociacionArchivoId { get; set; }
    public string NombreArchivo { get; set; } = "";
    public string NombreArchivoGuardado { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string RutaArchivo { get; set; } = "";
    public DateTime SubidoEn { get; set; } = DateTime.Now;
    public int Version { get; set; }
    public bool IsActivo { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}