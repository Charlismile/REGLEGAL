using Microsoft.AspNetCore.Components;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Pages.Asociaciones;

public partial class EditAsociacion : ComponentBase
{
    [Parameter] public int id { get; set; }
    private AsociacionModel? aModel;
    private bool cargando = true;

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            Console.WriteLine($"[DEBUG] Cargando asociación con id: {id}");
            aModel = await AsociacionService.ObtenerPorId(id);
            if (aModel == null)
            {
                Console.WriteLine($"[DEBUG] No se encontró asociación con id: {id}");
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
        if (aModel == null)
            return;

        var resultado = await AsociacionService.ActualizarAsociacion(aModel);

        if (resultado.Success)
        {
            Console.WriteLine("✅ Asociación actualizada correctamente.");
            Navigation.NavigateTo("/listado");
        }
        else
        {
            Console.WriteLine($"❌ Error al actualizar: {resultado.Message}");
        }
    }

    private void Volver()
    {
        Navigation.NavigateTo("/listado");
    }
}