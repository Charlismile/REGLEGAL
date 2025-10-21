using Microsoft.AspNetCore.Components;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Pages.Comites;

public partial class EditComite : ComponentBase
{
    [Parameter] public int id { get; set; }
    private ComiteModel? cModel;
    private bool cargando = true;

    [Inject] private IRegistroComite ComiteService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

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

        try
        {
            var resultado = await ComiteService.ActualizarComite(cModel);

            if (resultado.Success)
            {
                Console.WriteLine("✅ Comité actualizado correctamente.");
                Navigation.NavigateTo("/admin/listado");
            }
            else
            {
                Console.WriteLine($"❌ Error al actualizar: {resultado.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción al guardar: {ex.Message}");
        }
    }

    private void Volver()
    {
        Navigation.NavigateTo("/admin/listado");
    }
}