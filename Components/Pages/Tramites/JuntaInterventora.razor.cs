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

    private JuntaInterventoraModel CModel { get; set; } = new();
    private EditContext editContext = default!;
    private ValidationMessageStore messageStore = default!;
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
    
    private List<ListModel> Cargos = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // 🔹 CORRECCIÓN: Inicializar la lista de interventores
            CModel = new JuntaInterventoraModel 
            { 
                MiembrosInterventores = new List<MiembroComiteModel>() 
            };
            
            editContext = new EditContext(CModel);
            editContext.OnFieldChanged += OnFieldChanged;
            messageStore = new ValidationMessageStore(editContext);
        
            Cargos = new List<ListModel>();
        
            await CargarListasIniciales();
            await ObtenerUsuarioActual();

            if (ComiteId > 0)
            {
                await CargarComiteExistente();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en OnInitializedAsync: {ex.Message}");
            MensajeError = "Error al inicializar el formulario";
        }
    }

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        messageStore.Clear();

        // Validaciones específicas para Junta Interventora
        if (ComiteSeleccionadoId == null)
            messageStore.Add(() => CModel.NombreComiteSalud, "Debe seleccionar un comité con personería.");

        if (string.IsNullOrWhiteSpace(CModel.NumeroResolucion))
            messageStore.Add(() => CModel.NumeroResolucion, "El número de resolución es obligatorio.");

        if (CModel.FechaEleccion == default)
            messageStore.Add(() => CModel.FechaEleccion, "La fecha de elección es obligatoria.");

        // 🔹 CORRECCIÓN: Usar MiembrosInterventores en lugar de MiembrosActuales
        if (CModel.MiembrosInterventores.Count != NUMERO_INTERVENTORES_FIJO)
        {
            messageStore.Add(() => CModel.MiembrosInterventores, $"Debe haber exactamente {NUMERO_INTERVENTORES_FIJO} interventores.");
        }
        else
        {
            for (int i = 0; i < CModel.MiembrosInterventores.Count; i++)
            {
                var miembro = CModel.MiembrosInterventores[i];
                if (string.IsNullOrWhiteSpace(miembro.NombreMiembro))
                    messageStore.Add(() => CModel.MiembrosInterventores, $"El interventor {i + 1} debe tener un nombre.");
            
                if (string.IsNullOrWhiteSpace(miembro.ApellidoMiembro))
                    messageStore.Add(() => CModel.MiembrosInterventores, $"El interventor {i + 1} debe tener un apellido.");
            
                if (string.IsNullOrWhiteSpace(miembro.CedulaMiembro))
                    messageStore.Add(() => CModel.MiembrosInterventores, $"El interventor {i + 1} debe tener una cédula.");
            
                if (miembro.CargoId == 0)
                    messageStore.Add(() => CModel.MiembrosInterventores, $"El interventor {i + 1} debe tener un cargo.");
            }
        }

        editContext.NotifyValidationStateChanged();
    }

    private async Task CargarListasIniciales()
    {
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
            // Asignar ComiteBaseId al modelo específico
            CModel.ComiteBaseId = comiteCompleto.ComiteId;

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
        // 🔹 CORRECCIÓN: Usar MiembrosInterventores y asegurar que esté inicializada
        if (CModel.MiembrosInterventores == null)
        {
            CModel.MiembrosInterventores = new List<MiembroComiteModel>();
        }

        interventoresDesplegados = true;
        
        // Agregar los 2 interventores requeridos si no existen
        int faltantes = NUMERO_INTERVENTORES_FIJO - CModel.MiembrosInterventores.Count; 
        for (int i = 0; i < faltantes; i++)
        {
            CModel.MiembrosInterventores.Add(new MiembroComiteModel()); 
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
            // Convertir ComiteModel a JuntaInterventoraModel
            CModel.ComiteId = comite.ComiteId;
            CModel.ComiteBaseId = comite.ComiteBaseId ?? 0;
            CModel.NumeroResolucion = comite.NumeroResolucion;
            CModel.FechaResolucion = comite.FechaResolucion;
            CModel.FechaEleccion = comite.FechaEleccion ?? DateTime.Now;
            CModel.CreadaPor = comite.CreadaPor;
        
            // 🔹 CORRECCIÓN: Asignar correctamente los interventores
            CModel.MiembrosInterventores = comite.MiembrosInterventores ?? new List<MiembroComiteModel>();

            interventoresDesplegados = CModel.MiembrosInterventores.Any();
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
            // 🔹 CORRECCIÓN: Usar MiembrosInterventores
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
            
            var result = await RegistroComiteService.RegistrarJuntaInterventora(CModel);
            
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
    
    private bool PuedeGuardar()
    {
        // 1. Verificar que no esté en proceso
        if (IsSubmitting)
            return false;

        // 2. Validar comité seleccionado
        if (ComiteSeleccionadoId == null || CModel.ComiteBaseId <= 0)
            return false;

        // 3. Validaciones básicas
        if (string.IsNullOrWhiteSpace(CModel.NumeroResolucion) ||
            CModel.FechaEleccion == default)
            return false;

        // 4. Validar interventores - 🔹 CORRECCIÓN: Usar MiembrosInterventores
        if (CModel.MiembrosInterventores?.Count != NUMERO_INTERVENTORES_FIJO)
            return false;

        // 5. Validar que todos los interventores tengan datos
        if (CModel.MiembrosInterventores.Any(m => 
                string.IsNullOrWhiteSpace(m.NombreMiembro) ||
                string.IsNullOrWhiteSpace(m.ApellidoMiembro) ||
                string.IsNullOrWhiteSpace(m.CedulaMiembro) ||
                m.CargoId == 0))
            return false;

        // 6. Validar cargos únicos (Presidente y Tesorero)
        var cargosAsignados = CModel.MiembrosInterventores.Select(m => m.CargoId).ToList();
        if (cargosAsignados.Distinct().Count() != cargosAsignados.Count)
            return false;

        // 7. Validar archivos
        if (!ArchivosSeleccionados.Any())
            return false;

        return true;
    }
    
    private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        StateHasChanged(); // Fuerza la actualización del botón
    }

    private void Cancelar() => Navigation.NavigateTo("/Dashboard");
}