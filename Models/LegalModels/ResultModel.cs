namespace REGISTROLEGAL.Models.LegalModels
{
    public class ResultModel
    {
        public bool Success { get; set; } = false;
        
        public int Id { get; set; }
        public string Message { get; set; } = String.Empty;
        public List<string> Errores { get; set; } = new List<string>();
        public int AsociacionId { get; set; }
    }
}