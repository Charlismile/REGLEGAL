namespace REGISTROLEGAL.Data;

public class FullUserModel : ApplicationUser
{
    public string Region { get; set; } = String.Empty;
    public string Direccion { get; set; } = String.Empty;
    public string SubDireccion { get; set; } = String.Empty;
    public string Departamento { get; set; } = String.Empty;
    public string Seccion { get; set; } = String.Empty;
}