using REGISTROLEGAL.Data;
using REGISTROLEGAL.Models;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IUserData
{
    Task<ApplicationUser> GetUser(string UserName);
    Task<List<ApplicationUser>> GetAllUsers(string Filter);
    
    Task<ResultModel> CreateUser(ApplicationUser UserData, List<string> Roles);
    
    Task<ResultModel> UpdateUser(ApplicationUser UserData, List<string> Roles);

    // ACTIVE DIRECTORY
    Task<ResultModel> LoginAD(string UserName, string Password);
    Task<ResultGenericModel<ActiveDirectoryUserModel>> FindUserByEmail(string Email);
    Task LogoutAsync();
}