using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Formularios;

public partial class RegistroComite : ComponentBase
{
    [Inject] private ICommon _Commonservice { get; set; }
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private DbContextLegal _context { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private ComiteModel CModel { get; set; } = new();
    private TbDetalleRegComiteHistorial CModelHistorial { get; set; } = new();

    private string MensajeExito { get; set; } = string.Empty;
    private string MensajeError { get; set; } = string.Empty;
    private List<ListModel> comiteRegioneslist { get; set; } = new();
    private List<ListModel> comiteProvinciaList { get; set; } = new();
    private List<ListModel> comiteDistritoList { get; set; } = new();
    private List<ListModel> comiteCorregimientoList { get; set; } = new();
    private List<ListModel> Cargos { get; set; } = new();

    private IBrowserFile _archivoResolucion;

    protected override async Task OnInitializedAsync()
    {
        Cargos = (await _Commonservice.GetCargos()).ToList();

        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        CModel.CreadaPor = user.Identity?.IsAuthenticated == true
            ? user.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value
              ?? user.FindFirst(c => c.Type == "sub")?.Value
              ?? "usuario-desconocido"
            : "invitado";
        
        comiteRegioneslist = await _Commonservice.GetRegiones();
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
        MensajeExito = string.Empty;
        MensajeError = string.Empty;

        try
        {
            // Validaciones según tipo de trámite
            if (CModel.TipoTramiteEnum == TipoTramite.Personeria && CModel.Miembros.Count != 7)
            {
                MensajeError = "Debe registrar exactamente 7 miembros para la Junta Directiva.";
                return;
            }
            if (CModel.TipoTramiteEnum == TipoTramite.JuntaInterventora && CModel.MiembrosInterventores.Count == 0)
            {
                MensajeError = "Debe registrar al menos un interventor.";
                return;
            }

            if (CModel.CreadaEn == default)
                CModel.CreadaEn = DateTime.Now;
            if (CModelHistorial.FechaCambioCo == default)
                CModelHistorial.FechaCambioCo = DateTime.Now;

            // Guardar en servicio
            var (exito, mensaje) = await _Commonservice.RegistrarComiteAsync(CModel, _archivoResolucion);

            if (exito)
            {
                MensajeExito = mensaje;
                // Opcional: Navigation.NavigateTo("/comites");
            }
            else
            {
                MensajeError = mensaje;
            }
        }
        catch (Exception ex)
        {
            MensajeError = "Error inesperado: " + ex.Message;
        }

        StateHasChanged();
    }

    private void AgregarMiembroVacio()
    {
        CModel.Miembros.Add(new MiembroComiteModel());
    }

    private void Cancelar()
    {
        Navigation.NavigateTo("/");
    }
}
