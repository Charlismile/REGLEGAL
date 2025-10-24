namespace REGISTROLEGAL.Models.LegalModels;

public class HistorialAsociacionModel
{
    public int HistorialId { get; set; }
    public int DetRegAsociacionId { get; set; }
    public int AsociacionId { get; set; }
    public string Accion { get; set; } = "";
    public string Comentario { get; set; } = "";
    public string UsuarioId { get; set; } = "";
    public DateTime FechaModificacion { get; set; }
}