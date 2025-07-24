// Services/IRegistroService.cs

using REGISTROLEGAL.DTOs;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IRegistroService
{
    Task<ResultadoRegistro> RegistrarAsociacionAsync(RegistroAsociacionDto dto);
    Task<ResultadoRegistro> RegistrarComiteAsync(RegistroComiteDto dto);
}

// Resultado claro para el componente Blazor
public class ResultadoRegistro
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; }
    public int IdGenerado { get; set; }
}