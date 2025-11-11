using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Pages.Asociaciones;

public partial class EditAsociacion : ComponentBase
{
    [Parameter] public int id { get; set; }
    private AsociacionModel? aModel;
    private bool cargando = true;
    private bool guardando = false;
    private string mensajeExito = "";
    private string mensajeError = "";

    [Inject] private IRegistroAsociacion AsociacionService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Console.WriteLine($"[DEBUG] Cargando asociación con id: {id}");
            aModel = await AsociacionService.ObtenerPorId(id);

            if (aModel == null)
            {
                Console.WriteLine($"[DEBUG] No se encontró asociación con id: {id}");
                mensajeError = "No se encontró la asociación especificada.";
            }
            else
            {
                // Establecer valores por defecto
                if (string.IsNullOrEmpty(aModel.CargoRepLegal))
                {
                    aModel.CargoRepLegal = "Presidente";
                }

                // Obtener usuario autenticado
                await ObtenerUsuarioActual();
                Console.WriteLine($"[DEBUG] Asociación cargada: {aModel.NombreAsociacion}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            mensajeError = $"Error al cargar la asociación: {ex.Message}";
        }
        finally
        {
            cargando = false;
            StateHasChanged();
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
                
                aModel!.CreadaPor = user.Identity.Name ?? "Sistema";
                aModel.UsuarioId = userId;
                
                Console.WriteLine($"Usuario autenticado: {userId}, Nombre: {aModel.CreadaPor}");
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
        if (aModel == null)
            return;

        guardando = true;
        mensajeExito = "";
        mensajeError = "";

        try
        {
            // Validar usuario autenticado
            if (string.IsNullOrWhiteSpace(aModel.CreadaPor))
            {
                await ObtenerUsuarioActual();
                
                if (string.IsNullOrWhiteSpace(aModel.CreadaPor))
                {
                    mensajeError = "Usuario no autenticado. Por favor, inicie sesión.";
                    guardando = false;
                    StateHasChanged();
                    return;
                }
            }

            Console.WriteLine($"[DEBUG] Actualizando asociación ID: {aModel.AsociacionId}");
            var resultado = await AsociacionService.ActualizarAsociacion(aModel);

            if (resultado.Success)
            {
                mensajeExito = "✅ Asociación actualizada correctamente.";
                Console.WriteLine("✅ Asociación actualizada correctamente.");
                
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