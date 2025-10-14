using System.ComponentModel.DataAnnotations;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Data;
using REGISTROLEGAL.Models;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Components.Pages.User;

public partial class Create : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Parameter] public string? UserName { get; set; }

    #region Model

    private class RegisterModel
    {
        public string UserName { get; set; } = "";

        public string Id { get; set; } = "";

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "El correo no tiene formato correcto.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "El nombre es requerido.")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "El apellido es requerido.")]
        public string LastName { get; set; } = "";
        public bool IsAdminUser { get; set; } = false;
        public bool IsFromActiveDirectory { get; set; } = true;
    }

    #endregion

    #region Variables

    private RegisterModel FormData { get; set; } = new();
    private EditContext EditContext;
    private ValidationMessageStore MessageStore;
    private List<string> ErrorsFormMessages { get; set; } = new();
    private bool UserFound { get; set; } = false;
    private bool UpdatingUser { get; set; } = false;

    #endregion

    #region Events

    protected override async Task OnInitializedAsync()
    {
        ErrorsFormMessages = new();
        FormData = new();
        EditContext = new EditContext(FormData);
        MessageStore = new ValidationMessageStore(EditContext);

        if (!String.IsNullOrEmpty(UserName))
        {
            ApplicationUser UserData = await _UserService.GetUser(UserName);
            if (UserData != null)
            {
                UserFound = true;
                FormData.UserName = UserName;
                FormData.Id = UserData.Id;
                FormData.Email = UserData.Email ?? "";
                FormData.FirstName = UserData.FirstName ?? "";
                FormData.LastName = UserData.LastName ?? "";
                
                var IdentityUserData = await _UserManager.FindByEmailAsync(UserName);
                if (IdentityUserData != null)
                {
                    var roles = await _UserManager.GetRolesAsync(IdentityUserData);
                    FormData.IsAdminUser = roles.Contains("user_admin");
                }
                UpdatingUser = true;
            }
        }
    }

    private async Task ValidateEmail()
    {
        ErrorsFormMessages.Clear();

        var SearchUser = await _UserManager.FindByEmailAsync(FormData.Email);
        if (SearchUser != null)
        {
            ErrorsFormMessages.Add("El usuario ya esta registrado en el sistema.");
            return;
        }

        ResultGenericModel<ActiveDirectoryUserModel> ActiveDirectoryUserModelData =
            await _UserService.FindUserByEmail(FormData.Email);

        if (ActiveDirectoryUserModelData.Data != null)
        {
            UserFound = true;
            FormData.LastName = ActiveDirectoryUserModelData.Data.LastName;
            FormData.FirstName = ActiveDirectoryUserModelData.Data.FirstName;
            FormData.IsFromActiveDirectory = true;
        }
        else
        {
            UserFound = false;
            FormData.LastName = "";
            FormData.FirstName = "";
            FormData.IsFromActiveDirectory = false;
            ErrorsFormMessages.Add("No se encontro el correo ingresado.");
        }
    }

    private async Task RegisterUser()
    {
        ApplicationUser UserData = new ApplicationUser()
        {
            UserName = FormData.Email,
            Email = FormData.Email,
            FirstName = FormData.FirstName,
            LastName = FormData.LastName,
            EmailConfirmed = true,
            CreatedOn = DateTime.Now,
            IsAproved = true,
            IsFromActiveDirectory = FormData.IsFromActiveDirectory,
        };

        List<string> Roles = new List<string>();

        if (FormData.IsAdminUser)
        {
            Roles.Add("user_admin");
        }

        ResultModel Resultado;
        if (UpdatingUser)
        {
            Resultado = await _UserService.UpdateUser(UserData, Roles);
        }
        else
        {
            Resultado = await _UserService.CreateUser(UserData, Roles);
        }
        if (Resultado.Success)
        {
            Navigation.NavigateTo("/administracion/users");
        }
    }

    #endregion
    private void Volver()
    {
        Navigation.NavigateTo("/admin/listado");
    }
}