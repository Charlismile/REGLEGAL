using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace REGISTROLEGAL.Components.Componente;

public partial class EditRegistroAsociacionContent : ComponentBase
{
    [Parameter] public RegistroAsociacionDTO Model { get; set; } = new();
    [Parameter] public EventCallback Cancelar { get; set; }

    private async Task CargarNuevosArchivos(InputFileChangeEventArgs e)
    {
        var archivos = e.GetMultipleFiles(10);
        foreach (var archivo in archivos)
        {
            if (!Model.DocumentoSubida.Any(a => a.Name == archivo.Name))
            {
                Model.DocumentoSubida.Add(archivo);
            }
        }
    }

    private void RemoverNuevoArchivo(IBrowserFile archivo)
    {
        Model.DocumentoSubida.Remove(archivo);
    }

    private async Task Guardar()
    {
        await InvokeAsync(StateHasChanged);
    }
}
