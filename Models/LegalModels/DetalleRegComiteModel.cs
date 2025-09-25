namespace REGISTROLEGAL.Models.LegalModels;

public class DetalleRegComiteModel
{
    public int DetalleRegComiteId { get; set; }

    // Fecha y usuario que creó el registro
    public DateTime? CreadaEn { get; set; }
    public string CreadaPor { get; set; } = string.Empty;

    // Numeración del registro del comité
    public int NumRegCoSecuencia { get; set; }   // Secuencia interna
    public int NomRegCoAnio { get; set; }        // Año del registro
    public int NumRegCoMes { get; set; }         // Mes del registro
    public string? NumRegCoCompleta { get; set; } // Formato completo: SOL-YYYY-MM-XXXXXXXXXX

    // Tipo de trámite (1=Personería, 2=CambioDirectiva, 3=JuntaInterventora)
    public int TipoTramiteId { get; set; }
}