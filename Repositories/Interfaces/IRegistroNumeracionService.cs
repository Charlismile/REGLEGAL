namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IRegistroNumeracionService
{
    Task<string> GenerarNumeroAsociacion();
    Task<string> GenerarNumeroComite();
}