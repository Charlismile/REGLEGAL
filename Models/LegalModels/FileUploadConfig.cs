namespace REGISTROLEGAL.Models.LegalModels;

public class FileUploadConfig
{
    public int MaxFileSizeMB { get; set; } = 5;
    public int MaxFilesPerUserPerHour { get; set; } = 10;
    public string[] AllowedMimeTypes { get; set; } = { "application/pdf" };
}