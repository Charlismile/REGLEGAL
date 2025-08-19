using System.ComponentModel.DataAnnotations;

namespace REGISTROLEGAL.Models.LegalModels;

public class ArchivoModel
{
    [Required(ErrorMessage = "Debe adjuntar un archivo")]
    public IFormFile Archivo { get; set; }

    [Required(ErrorMessage = "El tipo de documento es obligatorio")]
    [StringLength(100, ErrorMessage = "El tipo de documento no debe exceder los 100 caracteres")]
    public string TipoDocumento { get; set; }

    // Validación adicional de tamaño (máximo 5 MB)
    [Range(1, 5 * 1024 * 1024, ErrorMessage = "El archivo no puede superar los 5 MB")]
    public long? MaxFileSize => Archivo?.Length;

    // Opcional: restringir extensiones
    public bool ExtensionValida =>
        Archivo != null && 
        new[] { ".pdf", ".jpg", ".png" }
            .Contains(Path.GetExtension(Archivo.FileName).ToLower());
}