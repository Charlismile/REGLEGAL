

using REGISTROLEGAL.Models.Forms;
using REGISTROLEGAL.Data;

namespace REGISTROLEGAL.Repositories.Interfaces;

public interface IUserData
{
    Task<FullUserModel> GetUser(string UserName);
    Task<List<FullUserModel>> GetAllUsers(string Filter);
    
    Task<ResultModel> CreateUser(ApplicationUser UserData, List<string> Roles);
    
    Task<ResultModel> UpdateUser(ApplicationUser UserData, List<string> Roles);

    // ACTIVE DIRECTORY
    Task<ResultModel> LoginAD(string UserName, string Password);
    Task<ResultGenericModel<ActiveDirectoryUserModel>> FindUserByEmail(string Email);
}