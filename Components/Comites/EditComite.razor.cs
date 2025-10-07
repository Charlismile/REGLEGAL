using Microsoft.AspNetCore.Components;
using REGISTROLEGAL.Models.LegalModels;

namespace REGISTROLEGAL.Components.Comites;

public partial class EditComite : ComponentBase
{
    [Parameter] public int id { get; set; }
    private ComiteModel? cModel;
    private bool cargando = true;

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            Console.WriteLine($"[DEBUG] Cargando comité con id: {id}");
            cModel = await ComiteService.ObtenerComiteCompletoAsync(id);
            if (cModel == null)
            {
                Console.WriteLine($"[DEBUG] No se encontró comité con id: {id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
        }
        finally
        {
            cargando = false;
        }
    }

    private async Task Guardar()
    {
        if (cModel == null)
            return;

        var resultado = await ComiteService.ActualizarComite(cModel);

        if (resultado.Success)
        {
            // Mostrar un mensaje (puedes usar MudBlazor o Bootstrap Toasts si prefieres)
            Console.WriteLine("✅ Comité actualizado correctamente.");
            Navigation.NavigateTo("/comites");
        }
        else
        {
            Console.WriteLine($"❌ Error al actualizar: {resultado.Message}");
            
        }
    }

    private void Volver()
    {
        Navigation.NavigateTo("/comites");
    }
}