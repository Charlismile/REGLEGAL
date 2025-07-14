using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class RegistroAsociacionDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la asociación es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre de la asociación solo puede contener letras y espacios.")]
    public string NombreAsociacion { get; set; }

    [Required(ErrorMessage = "El folio es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El folio debe ser un número positivo.")]
    public int Folio { get; set; }

    [Required(ErrorMessage = "La actividad es obligatoria.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "La actividad solo puede contener letras y espacios.")]
    public string Actividad { get; set; }

    [Required(ErrorMessage = "El nombre del representante legal es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre del representante legal solo puede contener letras y espacios.")]
    public string NombreRepLegal { get; set; }

    [Required(ErrorMessage = "La cédula del representante legal es obligatoria.")]
    [RegularExpression(@"^(\d{1,2}-\d{1,8}-\d{1,8}|PE-\d{1,8}-\d{1,8}|E-\d{1,8}-\d{1,8}|N-\d{1,8}-\d{1,8}|\d{1,8}AV-\d{1,8}-\d{1,8}|\d{1,8}PI-\d{1,8}-\d{1,8})$", 
        ErrorMessage = "La cédula no tiene un formato válido. Ejemplos válidos: 1-123456-7, PE-123456-7, E-123456-7, N-123456-7, 1AV-123456-7, 1PI-123456-7")]
    public string CedulaRepLegal { get; set; }

    [Required(ErrorMessage = "El cargo del representante legal es obligatorio.")]
    public string CargoRepLegal { get; set; }

    [Required(ErrorMessage = "El teléfono del representante legal es obligatorio.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "El teléfono del representante legal solo puede contener números.")]
    public string TelefonoRepLegal { get; set; }

    [Required(ErrorMessage = "La dirección del representante legal es obligatoria.")]
    public string DireccionRepLegal { get; set; }

    [Required(ErrorMessage = "El nombre del apoderado abogado es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre del apoderado abogado solo puede contener letras y espacios.")]
    public string NombreApoAbogado { get; set; }

    [Required(ErrorMessage = "La cédula del apoderado abogado es obligatoria.")]
    [RegularExpression(@"^(\d{1}-\d{6,7}-\d{1}|PE-\d{6}-\d{1}|E-\d{6}-\d{1}|N-\d{6}-\d{1}|\d{1}AV-\d{6}-\d{1}|\d{1}PI-\d{6}-\d{1})$", 
        ErrorMessage = "La cédula no tiene un formato válido. Ejemplos válidos: 1-123456-7, PE-123456-7, E-123456-7, N-123456-7, 1AV-123456-7, 1PI-123456-7")]
    public string CedulaApoAbogado { get; set; }

    [Required(ErrorMessage = "El teléfono del apoderado abogado es obligatorio.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "El teléfono del apoderado abogado solo puede contener números.")]
    public string TelefonoApoAbogado { get; set; }

    [Required(ErrorMessage = "El correo electrónico del apoderado abogado es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    public string CorreoApoAbogado { get; set; }

    [Required(ErrorMessage = "La dirección del apoderado abogado es obligatoria.")]
    public string DireccionApoAbogado { get; set; }
}