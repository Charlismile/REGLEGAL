using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Models.Forms;
using REGISTROLEGAL.Data;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services;

public class UserDataService : IUserData
{
    private readonly IConfiguration _Configuration;
    private readonly UserManager<ApplicationUser> _UserManager;
    private readonly DbContextLegal _context;
    private readonly string FakePassword = "";
    private readonly ActiveDirectoryApiModel ActiveDirectoryModel;
    private readonly HttpClient _HttpClient;

    public UserDataService(
        UserManager<ApplicationUser> userManager,
        DbContextLegal context,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _UserManager = userManager;
        _context = context;
        _Configuration = configuration;
        FakePassword = _Configuration["FakePass"] ?? "";
        ActiveDirectoryModel = new ActiveDirectoryApiModel()
        {
            BaseUrl = _Configuration["API_INFO:URL"] ?? "",
            Token = _Configuration["API_INFO:Token"] ?? "",
        };
        _HttpClient = httpClient;
        _HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ActiveDirectoryModel.Token);
        _HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }


    public async Task<FullUserModel> GetUser(string UserName)
    {
        FullUserModel UserData = new FullUserModel();
        var user = await _UserManager.FindByEmailAsync(UserName);

        if (user != null)
        {
            UserData = new FullUserModel()
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email,
                UserName = user.UserName,
                CreadoEn = user.CreadoEn,
                LastLoginDate = user.LastLoginDate,
                IsAproved = user.IsAproved,
            };
        }

        return UserData;
    }

    public async Task<List<FullUserModel>> GetAllUsers(string Filter)
    {
        List<FullUserModel> Users = new List<FullUserModel>();
        var user = await _UserManager.Users.ToListAsync();
        foreach (var item in user)
        {
            FullUserModel UserData = new FullUserModel()
            {
                Id = item.Id,
                Nombre = item.Nombre,
                Apellido = item.Apellido,
                Email = item.Email,
                UserName = item.UserName,
                CreadoEn = item.CreadoEn,
                LastLoginDate = item.LastLoginDate,
                IsAproved = item.IsAproved,
            };
            Users.Add(UserData);
        }

        if (Users.Count > 0 && !String.IsNullOrEmpty(Filter))
        {
            Users = Users.Where(x =>
                (x.Nombre ?? "").ToLower().Contains(Filter.ToLower()) ||
                (x.Apellido ?? "").ToLower().Contains(Filter.ToLower()) ||
                (x.Email ?? "").ToLower().Contains(Filter.ToLower())).ToList();
        }

        return Users;
    }

    public async Task<ResultModel> CreateUser(ApplicationUser UserData, List<string> Roles)
    {
        ResultModel ResultModel;
        try
        {
            var checkUser = await _UserManager.FindByEmailAsync(UserData.Email);
            if (checkUser != null)
            {
                return ResultModel = new ResultModel()
                {
                    Success = false,
                    Message = "El usuario ya existe",
                };
            }
            
            var user = await _UserManager.CreateAsync(UserData, FakePassword);
            if (!user.Succeeded)
            {
                ResultModel = new ResultModel()
                {
                    Success = false,
                    Message = "Error creando el usuario.",
                };
            }

            if (!user.Succeeded)
            {
                return ResultModel = new ResultModel()
                {
                    Success = false,
                    Message = "Error creando el usuario.",
                };
            }

            foreach (var Rol in Roles)
            {
                var userToRol = await _UserManager.AddToRoleAsync(UserData, Rol);
                if (!userToRol.Succeeded)
                {
                    return ResultModel = new ResultModel()
                    {
                        Success = false,
                        Message = $"Error agregando el usuario al rol: {Rol}.",
                    };
                }
            }

            return ResultModel = new ResultModel()
            {
                Success = true,
                Message = $"El usuario fue creado correctamente.",
            };
        }
        catch (Exception ex)
        {
            return ResultModel = new ResultModel()
            {
                Success = false,
                Message = $"Error: {ex.Message}.",
            };
        }
    }

    public async Task<ResultModel> UpdateUser(ApplicationUser UserData, List<string> Roles)
    {
        ResultModel ResultModel;
        try
        {
            var IdentityUserData = await _UserManager.FindByEmailAsync(UserData.Email);
            IdentityUserData.Nombre = UserData.Nombre;
            IdentityUserData.Apellido = UserData.Apellido;
            
            IdentityUserData.IsAproved = UserData.IsAproved;
            await _UserManager.UpdateAsync(IdentityUserData);
            
            var roles = await _UserManager.GetRolesAsync(IdentityUserData);
            //Then remove all the assigned roles for this user
            var result = await _UserManager.RemoveFromRolesAsync(IdentityUserData, roles);
            
            if (!result.Succeeded)
            {
                return ResultModel = new ResultModel()
                {
                    Success = false,
                    Message = $"Cannot remove user existing roles",
                };
            }
            
            foreach (var Rol in Roles)
            {
                var userToRol = await _UserManager.AddToRoleAsync(IdentityUserData, Rol);
                if (!userToRol.Succeeded)
                {
                    return ResultModel = new ResultModel()
                    {
                        Success = false,
                        Message = $"Error agregando el usuario al rol: {Rol}.",
                    };
                }
            }

            return ResultModel = new ResultModel()
            {
                Success = true,
                Message = $"El usuario fue actualizado correctamente.",
            };
        }
        catch (Exception ex)
        {
            return ResultModel = new ResultModel()
            {
                Success = false,
                Message = $"Error: {ex.Message}.",
            };
        }
    }

    public async Task<ResultModel> LoginAD(string UserName, string Password)
    {
        try
        {
            string EncodedUserName = WebUtility.UrlEncode(UserName);
            string EncodedPassword = WebUtility.UrlEncode(Password);
            string LoginUrl = $"{ActiveDirectoryModel.BaseUrl}login/{EncodedUserName}/{EncodedPassword}";
            string Result = await _HttpClient.GetFromJsonAsync<string>(LoginUrl) ?? "";
            return new ResultModel()
            {
                Success = true,
                Message = Result,
            };
        }
        catch (HttpRequestException ex)
        {
            return new ResultModel()
            {
                Success = false,
                Message = ex.StatusCode == System.Net.HttpStatusCode.Unauthorized ? "Error: Acceso denegado." : "",
            };
        }
    }

    public async Task<ResultGenericModel<ActiveDirectoryUserModel>> FindUserByEmail(string Email)
    {
        ResultGenericModel<ActiveDirectoryUserModel> ResultUserModel =
            new ResultGenericModel<ActiveDirectoryUserModel>();
        try
        {
            string EncodedEmail = WebUtility.UrlEncode(Email);
            string FindUrl = $"{ActiveDirectoryModel.BaseUrl}findbyemail/{EncodedEmail}";
            var data = await _HttpClient.GetFromJsonAsync<ActiveDirectoryUserModel>(FindUrl);
            if (data == null)
            {
                ResultUserModel.Success = false;
                ResultUserModel.Message = "No se encontro el usuario.";
                ResultUserModel.Data = null;
            }
            else
            {
                ResultUserModel.Success = true;
                ResultUserModel.Message = "No se encontro el usuario.";
                ResultUserModel.Data = data;
            }
        }
        catch (HttpRequestException ex)
        {
            ResultUserModel.Success = false;
            ResultUserModel.Message =
                ex.StatusCode == System.Net.HttpStatusCode.Unauthorized ? "Error: Acceso denegado." : ex.Message;
            ResultUserModel.Data = null;
        }

        return ResultUserModel;
    }
}