namespace REGISTROLEGAL.Models.LegalModels;

public class HistorialComiteModel
{
    public int HistorialId { get; set; }
    public int ComiteId { get; set; }
    public string? Accion { get; set; } 
    public string? Comentario { get; set; }
    public string? Usuario { get; set; }
    public DateTime Fecha { get; set; }
}