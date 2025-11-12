using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
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
    [Inject] private IArchivoLegalService ArchivoService { get; set; } = default!;

    // Archivos
    private List<IBrowserFile> nuevosArchivos = new();

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

                // Cargar archivos existentes
                await CargarArchivosExistentes();

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

    private async Task CargarArchivosExistentes()
    {
        try
        {
            if (aModel != null)
            {
                var archivos = await ArchivoService.ObtenerArchivosAsociacionAsync(aModel.AsociacionId, "DocumentosAsociacion");
                aModel.Archivos = archivos ?? new List<AArchivoModel>();
                Console.WriteLine($"[DEBUG] Cargados {aModel.Archivos.Count} archivos existentes");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error al cargar archivos: {ex.Message}");
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

    private async Task CargarNuevosArchivos(InputFileChangeEventArgs e)
    {
        foreach (var file in e.GetMultipleFiles())
        {
            if (Path.GetExtension(file.Name).ToLower() == ".pdf" && file.Size <= 50 * 1024 * 1024)
            {
                nuevosArchivos.Add(file);
            }
            else
            {
                if (Path.GetExtension(file.Name).ToLower() != ".pdf")
                {
                    mensajeError = $"El archivo {file.Name} no es un PDF válido.";
                }
                else if (file.Size > 50 * 1024 * 1024)
                {
                    mensajeError = $"El archivo {file.Name} excede el tamaño máximo de 50MB.";
                }
            }
        }
        StateHasChanged();
    }

    private void RemoverNuevoArchivo(IBrowserFile archivo)
    {
        nuevosArchivos.Remove(archivo);
        StateHasChanged();
    }

    private async Task EliminarArchivo(int archivoId)
    {
        try
        {
            var resultado = await ArchivoService.DesactivarArchivoAsociacionAsync(archivoId);
            if (resultado)
            {
                // Remover de la lista local
                aModel?.Archivos?.RemoveAll(a => a.AsociacionArchivoId == archivoId);
                mensajeExito = "Archivo eliminado correctamente.";
                StateHasChanged();
            }
            else
            {
                mensajeError = "Error al eliminar el archivo";
            }
        }
        catch (Exception ex)
        {
            mensajeError = $"Error al eliminar archivo: {ex.Message}";
        }
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
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
                // Subir nuevos archivos si los hay
                if (nuevosArchivos.Any())
                {
                    foreach (var archivo in nuevosArchivos)
                    {
                        await ArchivoService.GuardarArchivoAsociacionAsync(
                            aModel.AsociacionId, 
                            archivo, 
                            "DocumentosAsociacion"
                        );
                    }
                }

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