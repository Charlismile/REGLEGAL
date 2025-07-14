namespace REGISTROLEGAL.Models.Forms;

public class ResultModel
{
    public bool Success { get; set; }
    public string Message { get; set; } = String.Empty;
    public List<string> Errores { get; set; } = new List<string>();
}