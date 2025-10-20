namespace REGISTROLEGAL.Models.LegalModels;

public class DetalleRegAsociacionModel
{
    public int DetalleRegAsociacionId { get; set; }
    public DateTime CreadaEn { get; set; }
    public string CreadaPor { get; set; } = "";
    public int NumRegAsecuencia { get; set; }
    public string NumRegAcompleta { get; set; } = "";
    public string Accion { get; set; } = "";
    public string Comentario { get; set; } = "";
}