namespace REGISTROLEGAL.Repositories.Interfaces;

public interface ILocalStorage
{
    Task AddItem(string Key, string Value);
    Task RemoveItem(string Key);
    Task<string> GetItem(string Key); 
}