using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;
using REGISTROLEGAL.Repositories.Services;

namespace REGISTROLEGAL.Components.Formularios;

public partial class RegistroComite : ComponentBase
{
    [Inject] private ICommon _Commonservice { get; set; }

    [Inject] private DbContextLegal _context { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    // Modelo principal
    private ComiteModel CModel { get; set; } = new();

    // Mensajes de feedback
    private string MensajeExito { get; set; } = string.Empty;
    private string MensajeError { get; set; } = string.Empty;

    // Listas para selects
    private List<ListModel> comiteRegioneslist { get; set; } = new();
    private List<ListModel> comiteProvinciaList { get; set; } = new();
    private List<ListModel> comiteDistritolist { get; set; } = new();
    private List<ListModel> comiteCorregimientolist { get; set; } = new();
    
    private List<ListModel> Cargos { get; set; } = new();
   
    

    protected override async Task OnInitializedAsync()
    {
        comiteProvinciaList = await _Commonservice.GetProvincias();
        comiteRegioneslist = await _Commonservice.GetRegiones();
        Cargos = (await _Commonservice.GetCargos()).ToList();
    }

    private async Task comiteProvinciaChanged(int id)
    {
        CModel.ProvinciaId = id;
        comiteDistritolist = await _Commonservice.GetDistritos(id);
        comiteCorregimientolist.Clear();
        CModel.DistritoId = null;
        CModel.CorregimientoId = null;
    }

    private async Task comiteDistritoChanged(int id)
    {
        CModel.DistritoId = id;
        comiteCorregimientolist = await _Commonservice.GetCorregimientos(id);
        CModel.CorregimientoId = null;
    }
    

    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        // Aquí procesas el archivo subido
        var file = e.File;
        if (file != null)
        {
            using var stream = file.OpenReadStream(1024 * 1024); // 1MB max
            // Guardar o procesar el archivo
        }
    }
    // Envío de formulario
    private Task HandleValidSubmit()
    {
        MensajeExito = "Registro exitoso";
        return Task.CompletedTask;
    }

    private void Cancelar()
    {
        Navigation.NavigateTo("/");
    }
}
