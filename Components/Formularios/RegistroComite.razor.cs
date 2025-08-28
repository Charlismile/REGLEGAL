using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;
using REGISTROLEGAL.Repositories.Services;

namespace REGISTROLEGAL.Components.Formularios;

public partial class RegistroComite : ComponentBase
{
    [Inject] private ICommon _Commonservice { get; set; }
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private DbContextLegal _context { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IRegistroComite RegistroComiteService { get; set; } = default!;

    private ComiteModel CModel { get; set; } = new();
    private TbDetalleRegComiteHistorial CModelHistorial { get; set; } = new();
    private EditContext editContext;
    private ValidationMessageStore messageStore;
    private string UserName { get; set; } = "";
    private List<ListModel> comiteRegioneslist { get; set; } = new();
    private List<ListModel> comiteProvinciaList { get; set; } = new();
    private List<ListModel> comiteDistritoList { get; set; } = new();
    private List<ListModel> comiteCorregimientoList { get; set; } = new();
    private List<ListModel> Cargos { get; set; } = new();

    private IBrowserFile _archivoResolucion;

    protected override void OnInitialized()
    {
        editContext = new EditContext(CModel);
        messageStore = new ValidationMessageStore(editContext);
        editContext.OnValidationRequested += OnValidationRequested;
    }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        UserName = authState.User.Identity?.Name ?? "";
        
        Cargos = (await _Commonservice.GetCargos()).ToList();
        comiteRegioneslist = await _Commonservice.GetRegiones();
    }

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        messageStore.Clear();

        // Validaciones de provincias / regiones / distritos / corregimientos
        if (!CModel.ProvinciaId.HasValue || CModel.ProvinciaId == 0)
            messageStore.Add(() => CModel.ProvinciaId, "La provincia es obligatoria.");

        if (!CModel.RegionSaludId.HasValue || CModel.RegionSaludId == 0)
            messageStore.Add(() => CModel.RegionSaludId, "La región es obligatoria.");

        if (!CModel.DistritoId.HasValue || CModel.DistritoId == 0)
            messageStore.Add(() => CModel.DistritoId, "El distrito es obligatorio.");

        if (!CModel.CorregimientoId.HasValue || CModel.CorregimientoId == 0)
            messageStore.Add(() => CModel.CorregimientoId, "El corregimiento es obligatorio.");
    }


    private async Task RegionChanged(int id)
    {
        CModel.RegionSaludId = id;
        comiteProvinciaList = await _Commonservice.GetProvincias(id);
        comiteDistritoList.Clear();
        CModel.ProvinciaId = null;
        comiteCorregimientoList.Clear();
        CModel.DistritoId = null;
        CModel.CorregimientoId = null; 
    }
    private async Task ProvinciaChanged(int id)
    {
        CModel.ProvinciaId = id;
        comiteDistritoList = await _Commonservice.GetDistritos(id);
        comiteCorregimientoList.Clear();
        CModel.DistritoId = null;
        CModel.CorregimientoId = null;
    }
    
    private async Task DistritoChanged(int id)
    {
        CModel.DistritoId = id;
        comiteCorregimientoList = await _Commonservice.GetCorregimientos(id);
        CModel.CorregimientoId = null;
    }

    private Task OnFileSelected(InputFileChangeEventArgs e)
    {
        _archivoResolucion = e.File;
        return Task.CompletedTask;
    }

    private async Task HandleValidSubmit()
    {
        CModel.CreadaPor = UserName;
        ResultModel result = await RegistroComiteService.CrearComite(CModel);
        
        if (!result.Success)
        {
            var errorDetalle = string.IsNullOrEmpty(result.Message)
                ? "Hubo un error al procesar la solicitud."
                : result.Message;
            return;
        }
    }

    private void Cancelar()
    {
        Navigation.NavigateTo("/");
    }
}
