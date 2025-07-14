namespace REGISTROLEGAL.Models.Forms;

public class ResultGenericModel<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = String.Empty;
    public List<string> Errores { get; set; } = new List<string>();
    public T Data { get; set; }
}