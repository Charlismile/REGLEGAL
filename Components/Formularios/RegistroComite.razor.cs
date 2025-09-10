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

    private bool IsSubmitting = false;

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

    // private Task OnFileSelected(InputFileChangeEventArgs e)
    // {
    //     _archivoResolucion = e.File;
    //     return Task.CompletedTask;
    // }

    private async Task HandleValidSubmit()
    {
        CModel.CreadaPor = UserName;

        var result = await RegistroComiteService.CrearComite(CModel);

        if (!result.Success || result.Id == 0)
        {
            var errorDetalle = string.IsNullOrEmpty(result.Message)
                ? "Hubo un error al procesar la solicitud."
                : result.Message;
            return;
        }

        // Validar reglas de trámite
        if (CModel.TipoTramiteEnum == TipoTramite.JuntaInterventora)
        {
            var idPresidente = Cargos.FirstOrDefault(c => c.Name == "Presidente")?.Id;
            var idTesorero = Cargos.FirstOrDefault(c => c.Name == "Tesorero(a)")?.Id;

            if (CModel.Miembros.Count != 2 ||
                !CModel.Miembros.Any(m => m.CargoId == idPresidente) ||
                !CModel.Miembros.Any(m => m.CargoId == idTesorero))
            {
                messageStore.Add(() => CModel.Miembros, "La Junta Interventora debe tener exactamente Presidente y Tesorero(a).");
                return;
            }
        }
        else if (CModel.Miembros.Count > 7)
        {
            messageStore.Add(() => CModel.Miembros, "No se permiten más de 7 miembros.");
            return;
        }

        // Guardar miembros
        foreach (var miembro in CModel.Miembros)
        {
            await RegistroComiteService.AgregarMiembro(result.Id, miembro);
        }

        // TOD0: Guardar archivo resolución
        // if (_archivoResolucion != null)
        // {
        //     using var stream = _archivoResolucion.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
        //     await RegistroComiteService.GuardarResolucion(result.IdGenerado.Value, _archivoResolucion.Name, stream);
        // }

        Navigation.NavigateTo("/comites");
    }

    private TipoTramite _tipoTramite;
    private TipoTramite TipoTramiteEnum
    {
        get => _tipoTramite;
        set
        {
            if (_tipoTramite != value)
            {
                _tipoTramite = value;
                OnTipoTramiteChanged(value);
            }
        }
    }

    private async void OnTipoTramiteChanged(TipoTramite tipo)
    {
        if (tipo == TipoTramite.CambioDirectiva || tipo == TipoTramite.JuntaInterventora)
        {
            int comiteId = 1; // reemplaza por Id real
            await CargarComiteExistente(comiteId);
        }
        else
        {
            CModel.Miembros.Clear();
            CModel.MiembrosInterventores.Clear();
        }

        StateHasChanged();
    }

    private async Task CargarComiteExistente(int comiteId)
    {
        var comiteExistente = await RegistroComiteService.GetComiteByIdAsync(comiteId);
        if (comiteExistente != null)
        {
            CModel.NombreComiteSalud = comiteExistente.NombreComiteSalud;
            CModel.Comunidad = comiteExistente.Comunidad;
            CModel.RegionSaludId = comiteExistente.RegionSaludId;
            CModel.ProvinciaId = comiteExistente.ProvinciaId;
            CModel.DistritoId = comiteExistente.DistritoId;
            CModel.CorregimientoId = comiteExistente.CorregimientoId;

            if (CModel.TipoTramiteEnum == TipoTramite.CambioDirectiva)
            {
                CModel.Miembros = comiteExistente.Miembros.Select(m => new MiembroComiteModel()
                {
                    MiembroId = m.MiembroId,
                    NombreMiembro = m.NombreMiembro,
                    ApellidoMiembro = m.ApellidoMiembro,
                    CedulaMiembro = m.CedulaMiembro,
                    CargoId = m.CargoId
                }).ToList();
            }
            else if (CModel.TipoTramiteEnum == TipoTramite.JuntaInterventora)
            {
                CModel.MiembrosInterventores = comiteExistente.Miembros.Take(2).Select(m => new MiembroComiteModel()
                {
                    MiembroId = m.MiembroId,
                    NombreMiembro = m.NombreMiembro,
                    ApellidoMiembro = m.ApellidoMiembro,
                    CedulaMiembro = m.CedulaMiembro,
                    CargoId = m.CargoId
                }).ToList();
            }
        }
    }
    private void AgregarMiembro()
    {
        CModel.Miembros.Add(new MiembroComiteModel());
        StateHasChanged(); // Fuerza el renderizado seguro
        
        CModel.MiembrosInterventores.Add(new MiembroComiteModel());
        StateHasChanged();

    }

    private void RemoverMiembro(int miembroId)
    {
        var miembro = CModel.Miembros.FirstOrDefault(m => m.MiembroId == miembroId);
        if (miembro != null)
        {
            CModel.Miembros.Remove(miembro);
            StateHasChanged(); // Fuerza el renderizado seguro
            
            CModel.MiembrosInterventores.Remove(miembro);
            StateHasChanged();

        }
    }


    private void Cancelar()
    {
        Navigation.NavigateTo("/");
    }
}
