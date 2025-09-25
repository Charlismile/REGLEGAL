namespace REGISTROLEGAL.Models.LegalModels;

public class DetalleRegAsociacionModel
{
    public int DetalleRegAsociacionId { get; set; }
    public DateTime CreadaEn { get; set; }
    public string CreadaPor { get; set; } = "";
    public int NumRegAsecuencia { get; set; }
    public int NomRegAanio { get; set; }
    public int NumRegAmes { get; set; }
    public string NumRegAcompleta { get; set; } = "";
}