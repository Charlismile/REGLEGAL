using System.ComponentModel.DataAnnotations;

namespace SISTEMALEGAL.DTOs;

public class AsociacionDTO
{
    [Required(ErrorMessage = "El nombre de la asociación es obligatorio")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
    public string NombreAsociacion { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un representante legal")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un representante válido")]
    public int RepresentanteLegalId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un apoderado legal")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un apoderado válido")]
    public int ApoderadoLegalId { get; set; }

    [Required(ErrorMessage = "El folio es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Folio inválido")]
    public int Folio { get; set; }

    [StringLength(1000, ErrorMessage = "Máximo 1000 caracteres")]
    public string Actividad { get; set; }
}