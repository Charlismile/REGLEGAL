using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using REGISTROLEGAL.Components.Componente;

namespace REGISTROLEGAL.Components.Pages.Admin;

public partial class EditarRegistro : ComponentBase
{
    [Parameter] public int Id { get; set; }

    private RegistroAsociacionDTO Model { get; set; } = new();
    private bool Cargando = false;
    private string? Error = null;
    private EditRegistroAsociacionContent? formContent;

    protected override async Task OnParametersSetAsync()
    {
        Cargando = true;
        Error = null;
        try
        {
            Model = await RegistroService.ObtenerPorIdAsync(Id);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            Cargando = false;
        }
    }

    private async Task Guardar()
    {
        try
        {
            await RegistroService.ActualizarAsync(Model, Model.DocumentoSubida);
            NavManager.NavigateTo("/lista-registros", true);
            await JSRuntime.InvokeVoidAsync("toastrSuccess", "Registro actualizado con éxito.");
        }
        catch (Exception ex)
        {
            Error = $"Error al guardar: {ex.Message}";
        }
    }

    private void Cancelar()
    {
        NavManager.NavigateTo("/lista-registros");
    }
}