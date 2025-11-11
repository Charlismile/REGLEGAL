using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Pages.Comites;

public partial class EditComite : ComponentBase
{
    [Parameter] public int id { get; set; }
    private ComiteModel? cModel;
    private bool cargando = true;
    private bool guardando = false;
    private string mensajeExito = "";
    private string mensajeError = "";
    private List<ListModel> cargos = new();

    [Inject] private IRegistroComite ComiteService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ICommon CommonService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Console.WriteLine($"[DEBUG] Cargando comité con id: {id}");
            
            // Cargar lista de cargos primero
            cargos = await CommonService.GetCargos();
            
            // Cargar datos del comité
            cModel = await ComiteService.ObtenerComiteCompletoAsync(id);

            if (cModel == null)
            {
                Console.WriteLine($"[DEBUG] No se encontró comité con id: {id}");
                mensajeError = "No se encontró el comité especificado.";
            }
            else
            {
                // Obtener usuario autenticado
                await ObtenerUsuarioActual();
                Console.WriteLine($"[DEBUG] Comité cargado: {cModel.NombreComiteSalud}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            mensajeError = $"Error al cargar el comité: {ex.Message}";
        }
        finally
        {
            cargando = false;
        }
    }

    private async Task ObtenerUsuarioActual()
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            
            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                           ?? user.Identity.Name;
                
                cModel!.CreadaPor = user.Identity.Name ?? "Sistema";
                cModel.UsuarioId = userId;
                
                Console.WriteLine($"Usuario autenticado: {userId}, Nombre: {cModel.CreadaPor}");
            }
            else
            {
                Console.WriteLine("Usuario NO autenticado");
                mensajeError = "Usuario no autenticado. Por favor, inicie sesión.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo usuario: {ex.Message}");
            mensajeError = "Error al verificar autenticación.";
        }
    }

    private async Task Guardar()
    {
        if (cModel == null)
            return;

        guardando = true;
        mensajeExito = "";
        mensajeError = "";

        try
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(cModel.CreadaPor))
            {
                await ObtenerUsuarioActual();
                
                if (string.IsNullOrWhiteSpace(cModel.CreadaPor))
                {
                    mensajeError = "Usuario no autenticado. Por favor, inicie sesión.";
                    guardando = false;
                    StateHasChanged();
                    return;
                }
            }

            // Validar miembros
            if (cModel.Miembros == null || !cModel.Miembros.Any())
            {
                mensajeError = "Debe haber al menos un miembro en el comité.";
                guardando = false;
                StateHasChanged();
                return;
            }

            foreach (var miembro in cModel.Miembros)
            {
                if (string.IsNullOrWhiteSpace(miembro.NombreMiembro) ||
                    string.IsNullOrWhiteSpace(miembro.ApellidoMiembro) ||
                    string.IsNullOrWhiteSpace(miembro.CedulaMiembro) ||
                    miembro.CargoId == 0)
                {
                    mensajeError = "Todos los miembros deben tener nombre, apellido, cédula y cargo válidos.";
                    guardando = false;
                    StateHasChanged();
                    return;
                }
            }

            Console.WriteLine($"[DEBUG] Actualizando comité ID: {cModel.ComiteId}");
            var resultado = await ComiteService.ActualizarComite(cModel);

            if (resultado.Success)
            {
                mensajeExito = "✅ Comité actualizado correctamente.";
                Console.WriteLine("✅ Comité actualizado correctamente.");
                
                // Esperar un momento y redirigir
                await Task.Delay(1500);
                Navigation.NavigateTo("/admin/listado");
            }
            else
            {
                mensajeError = $"❌ Error al actualizar: {resultado.Message}";
                Console.WriteLine($"❌ Error al actualizar: {resultado.Message}");
            }
        }
        catch (Exception ex)
        {
            mensajeError = $"❌ Excepción al guardar: {ex.Message}";
            Console.WriteLine($"❌ Excepción al guardar: {ex.Message}");
        }
        finally
        {
            guardando = false;
            StateHasChanged();
        }
    }

    private void Volver()
    {
        Navigation.NavigateTo("/admin/listado");
    }
}