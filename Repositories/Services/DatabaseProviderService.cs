

using SISTEMALEGAL.Repositories.Interfaces;

namespace salud_ambiental.Services.ClassesPermisos;

public class DatabaseProviderService : IDatabaseProvider
{
    private readonly IConfiguration _configuration;

    public DatabaseProviderService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetConnectionString()
    {
        bool IsDev = Convert.ToBoolean(_configuration.GetSection("DevelopMode").Value ?? "false");
        return IsDev
            ? _configuration.GetConnectionString("DevConnection")
            : _configuration.GetConnectionString("DefaultConnection");
    }

    public bool GetEnvironment()
    {
        bool IsDev = Convert.ToBoolean(_configuration.GetSection("DevelopMode").Value ?? "false");
        return IsDev;
    }
}