using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Pages.Comites;

public partial class EditComite : ComponentBase
{
    [Parameter] public int id { get; set; }
    [Inject] private IRegistroComite ComiteService { get; set; } = default!;
    [Inject] private ICommon CommonService { get; set; } = default!;
    [Inject] private IArchivoLegalService ArchivoService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    private ComiteModel cModel = new();
    private bool cargando = true;
    private bool guardando = false;
    private string mensajeExito = "";
    private string mensajeError = "";

    // Listas para dropdowns
    private List<ListModel> regiones = new();
    private List<ListModel> provincias = new();
    private List<ListModel> distritos = new();
    private List<ListModel> corregimientos = new();
    private List<ListModel> cargos = new();

    // Archivos
    private List<IBrowserFile> nuevosArchivos = new();

    protected override async Task OnInitializedAsync()
    {
        await CargarListasIniciales();
        await CargarComite();
        await ObtenerUsuarioActual();
    }

    private async Task ObtenerUsuarioActual()
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            
            if (user.Identity?.IsAuthenticated == true)
            {
                cModel.UsuarioId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                cModel.CreadaPor = user.Identity.Name ?? "Sistema";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo usuario: {ex.Message}");
        }
    }

    private async Task CargarListasIniciales()
    {
        regiones = await CommonService.GetRegiones();
        cargos = await CommonService.GetCargos();
    }

    private async Task CargarComite()
    {
        cargando = true;
        StateHasChanged();

        try
        {
            var comite = await ComiteService.ObtenerComiteCompletoAsync(id);
            if (comite != null)
            {
                cModel = comite;
                
                // Cargar archivos existentes
                cModel.Archivos = await ArchivoService.ObtenerArchivosComiteAsync(id, "RESOLUCION COMITE");
                
                // Cargar listas de ubicación según los datos del comité
                if (cModel.RegionSaludId.HasValue)
                {
                    provincias = await CommonService.GetProvincias(cModel.RegionSaludId.Value);
                }
                if (cModel.ProvinciaId.HasValue)
                {
                    distritos = await CommonService.GetDistritos(cModel.ProvinciaId.Value);
                }
                if (cModel.DistritoId.HasValue)
                {
                    corregimientos = await CommonService.GetCorregimientos(cModel.DistritoId.Value);
                }
            }
            else
            {
                mensajeError = "No se pudo cargar el comité solicitado.";
            }
        }
        catch (Exception ex)
        {
            mensajeError = $"Error al cargar el comité: {ex.Message}";
        }
        finally
        {
            cargando = false;
            StateHasChanged();
        }
    }

    private async Task OnRegionChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int regionId) && regionId > 0)
        {
            cModel.RegionSaludId = regionId;
            provincias = await CommonService.GetProvincias(regionId);
            distritos = new();
            corregimientos = new();
            cModel.ProvinciaId = null;
            cModel.DistritoId = null;
            cModel.CorregimientoId = null;
            StateHasChanged();
        }
    }

    private async Task OnProvinciaChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int provinciaId) && provinciaId > 0)
        {
            cModel.ProvinciaId = provinciaId;
            distritos = await CommonService.GetDistritos(provinciaId);
            corregimientos = new();
            cModel.DistritoId = null;
            cModel.CorregimientoId = null;
            StateHasChanged();
        }
    }

    private async Task OnDistritoChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int distritoId) && distritoId > 0)
        {
            cModel.DistritoId = distritoId;
            corregimientos = await CommonService.GetCorregimientos(distritoId);
            cModel.CorregimientoId = null;
            StateHasChanged();
        }
    }

    private void AgregarMiembro()
    {
        cModel.Miembros ??= new List<MiembroComiteModel>();
        cModel.Miembros.Add(new MiembroComiteModel());
        StateHasChanged();
    }

    private void EliminarMiembro(MiembroComiteModel miembro)
    {
        cModel.Miembros?.Remove(miembro);
        StateHasChanged();
    }

    private async Task CargarNuevosArchivos(InputFileChangeEventArgs e)
    {
        foreach (var file in e.GetMultipleFiles())
        {
            if (Path.GetExtension(file.Name).ToLower() == ".pdf" && file.Size <= 50 * 1024 * 1024)
            {
                nuevosArchivos.Add(file);
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
            var resultado = await ArchivoService.DesactivarArchivoComiteAsync(archivoId);
            if (resultado)
            {
                // Remover de la lista local
                cModel.Archivos?.RemoveAll(a => a.ComiteArchivoId == archivoId);
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

    private async Task Guardar()
    {
        guardando = true;
        mensajeError = "";
        mensajeExito = "";
        StateHasChanged();

        try
        {
            // Validaciones básicas
            if (cModel.Miembros == null || !cModel.Miembros.Any())
            {
                mensajeError = "Debe agregar al menos un miembro al comité.";
                guardando = false;
                StateHasChanged();
                return;
            }

            // Asegurar que tenemos el usuario
            if (string.IsNullOrEmpty(cModel.UsuarioId))
            {
                await ObtenerUsuarioActual();
            }

            // CORRECCIÓN: Usar directamente ActualizarComite (sin la 'r' extra)
            var resultado = await ComiteService.ActualizarComite(cModel);
            
            if (resultado.Success)
            {
                // Subir nuevos archivos si los hay
                if (nuevosArchivos.Any())
                {
                    foreach (var archivo in nuevosArchivos)
                    {
                        await ArchivoService.GuardarArchivoComiteAsync(cModel.ComiteId, archivo, "RESOLUCION COMITE");
                    }
                }

                mensajeExito = "Comité actualizado exitosamente!";
                StateHasChanged();
                
                await Task.Delay(2000);
                NavigationManager.NavigateTo("/admin/listado");
            }
            else
            {
                mensajeError = resultado.Message ?? "Error al actualizar el comité";
                guardando = false;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            mensajeError = $"Error al guardar: {ex.Message}";
            guardando = false;
            StateHasChanged();
        }
    }

    private void Volver()
    {
        NavigationManager.NavigateTo("/admin/listado");
    }
}