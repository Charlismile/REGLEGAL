using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using REGISTROLEGAL.Data;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components
{
    public partial class Dashboard : ComponentBase
    {
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected IUserData _UserService { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        private ApplicationUser LoggedUser = new ApplicationUser();
        private string UnidadUser { get; set; } = "";

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                Navigation.NavigateTo("/");
                return;
            }

            LoggedUser = await _UserService.GetUser(user.Identity.Name ?? "");
            if (LoggedUser != null)
            {
                UnidadUser = "Asesoria Legal"; 
            }
        }
    }
}