namespace REGISTROLEGAL.Models.LegalModels;

public class FileUploadConfig
{
    public int MaxFileSizeMB { get; set; } = 50;
    public int MaxFilesPerUserPerHour { get; set; } = 10;
    public string[] AllowedMimeTypes { get; set; } = { "application/pdf" };
}