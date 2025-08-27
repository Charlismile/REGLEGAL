namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IDatabaseProvider
{
    public string GetConnectionString();
    public bool GetEnvironment();
}