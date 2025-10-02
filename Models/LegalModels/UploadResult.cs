namespace REGISTROLEGAL.Models.LegalModels;

public class UploadResult
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? StoredFileName { get; set; }
    public string? ContentType { get; set; }
}