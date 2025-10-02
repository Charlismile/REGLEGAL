namespace REGISTROLEGAL.Models.LegalModels;

public class DocumentoModel
{
    public int DocumentoId { get; set; }
    
    /// Tipo de entidad: "COMITE" o "ASOCIACION"
    public string EntidadTipo { get; set; } = string.Empty;
    
    /// Id de la entidad relacionada (ComiteId o AsociacionId)
    public int EntidadId { get; set; }
    
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    
    /// Contenido del archivo en bytes
    public byte[]? FileContent { get; set; }
    
    public string ContentType { get; set; } = "application/pdf";
    
    public long FileSize { get; set; }
    
    public DateTime SubidoEn { get; set; } = DateTime.UtcNow;
    
    public bool IsActivo { get; set; } = true;

    /// Categoría opcional, ej: "PERFIL", "SOLICITUD"
    public string Categoria { get; set; } = string.Empty;
}