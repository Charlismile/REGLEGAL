namespace SISTEMALEGAL.DTOs;

public class ResultadoOperacion<T>
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = "";
    public T Data { get; set; } = default!;
    public List<string> Errores { get; set; } = new();
}