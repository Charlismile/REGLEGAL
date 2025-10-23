using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Pages.Tramites;

public partial class JuntaInterventora : ComponentBase
{
    [Parameter] public int ComiteId { get; set; }
    
    [Inject] IRegistroComite RegistroComiteService { get; set; }
    [Inject] NavigationManager Navigation { get; set; }
    [Inject] IArchivoLegalService ArchivoLegalService { get; set; }
    [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] ICommon CommonService { get; set; }

    private ComiteModel CModel { get; set; } = new();
    private EditContext editContext = default!;
    private string MensajeExito = "";
    private string MensajeError = "";
    private string MensajeBusqueda = "";
    private bool IsSubmitting = false;
    private bool interventoresDesplegados = false;
    private const int NUMERO_INTERVENTORES_FIJO = 2;
    private List<IBrowserFile> ArchivosSeleccionados = new();
    private string? _archivoResolucionUrl;
    private List<ListModel> CargosInterventores = new();
    private bool MostrarErrores = false;
    private List<string> erroresFormulario = new();

    // Autocomplete
    private string searchText = "";
    private List<ComiteModel> sugerencias = new();
    private int? ComiteSeleccionadoId;
    private List<MiembroComiteModel> miembrosActuales = new();

    // Nombres para mostrar ubicación (solo lectura)
    private string RegionNombre => Regiones.FirstOrDefault(r => r.Id == CModel.RegionSaludId)?.Name ?? "";
    private string ProvinciaNombre => Provincias.FirstOrDefault(r => r.Id == CModel.ProvinciaId)?.Name ?? "";
    private string DistritoNombre => Distritos.FirstOrDefault(r => r.Id == CModel.DistritoId)?.Name ?? "";
    private string CorregimientoNombre => Corregimientos.FirstOrDefault(r => r.Id == CModel.CorregimientoId)?.Name ?? "";

    // Listas
    private List<ListModel> Regiones = new();
    private List<ListModel> Provincias = new();
    private List<ListModel> Distritos = new();
    private List<ListModel> Corregimientos = new();
    private List<ListModel> Cargos = new();

    protected override async Task OnInitializedAsync()
    {
        CModel.TipoTramiteEnum = TipoTramite.JuntaInterventora;
        editContext = new EditContext(CModel);
        await CargarListasIniciales();
        await ObtenerUsuarioActual();

        if (ComiteId > 0)
        {
            await CargarComiteExistente();
        }
        else
        {
            CModel = new ComiteModel { TipoTramiteEnum = TipoTramite.JuntaInterventora };
        }
    }

    private async Task CargarListasIniciales()
    {
        Regiones = await CommonService.GetRegiones();
    
        // Cargar SOLO Presidente y Tesorero
        var todosLosCargos = await CommonService.GetCargos();
        CargosInterventores = todosLosCargos
            .Where(c => c.Name.Equals("Presidente", StringComparison.OrdinalIgnoreCase) ||
                        c.Name.Equals("Tesorero", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task BuscarComitesAsync()
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            sugerencias.Clear();
            StateHasChanged();
            return;
        }

        // Solo comités con Personería
        var comites = await RegistroComiteService.ObtenerComitesPorTipo(TipoTramite.Personeria);
        sugerencias = comites
            .Where(c => c.NombreComiteSalud.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        c.Comunidad.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();
        StateHasChanged();
    }

    private async Task SeleccionarComite(ComiteModel seleccionado)
    {
        sugerencias.Clear();
        searchText = seleccionado.NombreComiteSalud;
        var comiteCompleto = await RegistroComiteService.ObtenerComiteCompletoAsync(seleccionado.ComiteId);
        if (comiteCompleto != null)
        {
            // Copiar datos heredables
            CModel.NombreComiteSalud = comiteCompleto.NombreComiteSalud;
            CModel.Comunidad = comiteCompleto.Comunidad;
            CModel.RegionSaludId = comiteCompleto.RegionSaludId;
            CModel.ProvinciaId = comiteCompleto.ProvinciaId;
            CModel.DistritoId = comiteCompleto.DistritoId;
            CModel.CorregimientoId = comiteCompleto.CorregimientoId;

            // Guardar miembros actuales para mostrarlos
            miembrosActuales = comiteCompleto.Miembros;

            ComiteSeleccionadoId = comiteCompleto.ComiteId;
            MensajeBusqueda = "✅ Comité seleccionado. Ahora agregue los interventores.";
        }
        StateHasChanged();
    }

    private string SearchText
    {
        get => searchText;
        set
        {
            searchText = value;
            _ = BuscarComitesAsync();
        }
    }

    private void DesplegarInterventores()
    {
        if (CModel.MiembrosInterventores.Count >= NUMERO_INTERVENTORES_FIJO)
        {
            interventoresDesplegados = true;
        }
        else
        {
            interventoresDesplegados = true;
            int faltantes = NUMERO_INTERVENTORES_FIJO - CModel.MiembrosInterventores.Count;
            for (int i = 0; i < faltantes; i++)
            {
                CModel.MiembrosInterventores.Add(new MiembroComiteModel());
            }
        }
        StateHasChanged();
    }

    private async Task CargarDocumentos(InputFileChangeEventArgs e)
    {
        ArchivosSeleccionados.Clear();
        foreach (var file in e.GetMultipleFiles())
        {
            if (Path.GetExtension(file.Name).ToLower() != ".pdf")
            {
                MensajeError = $"El archivo {file.Name} debe ser PDF.";
                continue;
            }
            if (file.Size > 50 * 1024 * 1024)
            {
                MensajeError = $"El archivo {file.Name} excede el límite de 50 MB.";
                continue;
            }
            ArchivosSeleccionados.Add(file);
        }
        StateHasChanged();
    }

    private void RemoverArchivoPendiente(IBrowserFile file)
    {
        ArchivosSeleccionados.Remove(file);
        StateHasChanged();
    }

    private async Task ObtenerUsuarioActual()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        CModel.CreadaPor = user.Identity?.Name ?? "Sistema";
    }

    private async Task CargarComiteExistente()
    {
        var comite = await RegistroComiteService.ObtenerComiteCompletoAsync(ComiteId);
        if (comite != null && comite.TipoTramiteEnum == TipoTramite.JuntaInterventora)
        {
            CModel = comite;
            interventoresDesplegados = true;
            ComiteSeleccionadoId = comite.ComiteId;

           
            miembrosActuales = comite.Miembros; 

            await CargarListasIniciales();
        }
    }

    private void HandleInvalidSubmit(EditContext context)
    {
        erroresFormulario.Clear();
        var mensajes = context.GetValidationMessages();
        foreach (var msg in mensajes)
        {
            erroresFormulario.Add(msg);
        }
    }

    private async Task HandleValidSubmit()
    {
        IsSubmitting = true;
        MensajeError = "";
        MensajeExito = "";
        MostrarErrores = true;

        if (ComiteSeleccionadoId == null)
        {
            MensajeError = "Debe seleccionar un comité con personería.";
            IsSubmitting = false;
            return;
        }

        try
        {
            if (CModel.MiembrosInterventores.Count != NUMERO_INTERVENTORES_FIJO)
            {
                MensajeError = "Debe registrar exactamente 2 interventores: Presidente y Tesorero.";
                IsSubmitting = false;
                return;
            }

            var cargosValidos = new HashSet<int>(CargosInterventores.Select(c => c.Id));
            foreach (var m in CModel.MiembrosInterventores)
            {
                if (string.IsNullOrWhiteSpace(m.NombreMiembro) ||
                    string.IsNullOrWhiteSpace(m.ApellidoMiembro) ||
                    string.IsNullOrWhiteSpace(m.CedulaMiembro) ||
                    !cargosValidos.Contains(m.CargoId))
                {
                    MensajeError = "Cada interventor debe tener nombre, apellido, cédula y un cargo válido (Presidente o Tesorero).";
                    IsSubmitting = false;
                    return;
                }
            }
            
            var cargosAsignados = CModel.MiembrosInterventores.Select(m => m.CargoId).ToList();
            if (cargosAsignados.Distinct().Count() != cargosAsignados.Count)
            {
                MensajeError = "No se permiten cargos duplicados. Debe haber un Presidente y un Tesorero.";
                IsSubmitting = false;
                return;
            }

            if (!ArchivosSeleccionados.Any())
            {
                MensajeError = "Debe adjuntar al menos un archivo de resolución.";
                IsSubmitting = false;
                return;
            }

            if (string.IsNullOrEmpty(CModel.CreadaPor))
                await ObtenerUsuarioActual();

            CModel.FechaCreacion = DateTime.Now;

            var result = await RegistroComiteService.CrearComite(CModel);
            if (!result.Success)
            {
                MensajeError = $"Error al registrar junta interventora: {result.Message}";
                IsSubmitting = false;
                return;
            }

            foreach (var archivo in ArchivosSeleccionados)
            {
                await ArchivoLegalService.GuardarArchivoComiteAsync(result.Id, archivo, "RESOLUCION JUNTA INTERVENTORA");
            }

            MensajeExito = "✅ Junta Interventora registrada exitosamente!";
            StateHasChanged();
            await Task.Delay(2000);
            Navigation.NavigateTo("/admin/listado");
        }
        catch (Exception ex)
        {
            MensajeError = $"Error: {ex.Message}";
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    private void Cancelar() => Navigation.NavigateTo("/Dashboard");
}