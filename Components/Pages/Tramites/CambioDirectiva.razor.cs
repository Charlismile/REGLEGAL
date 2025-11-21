using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Pages.Tramites;

public partial class CambioDirectiva : ComponentBase
{
    [Inject] IRegistroComite RegistroComiteService { get; set; }
    [Inject] NavigationManager Navigation { get; set; }
    [Inject] IArchivoLegalService ArchivoLegalService { get; set; }
    [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] ICommon CommonService { get; set; }
    [Parameter] public int ComiteId { get; set; }

    private CambioDirectivaModel CModel { get; set; } = new();
    private EditContext editContext = default!;
    private string MensajeExito = "";
    private string MensajeError = "";
    private string MensajeBusqueda = "";
    private bool IsSubmitting = false;
    private bool miembrosDesplegados = false;
    private const int NUMERO_MIEMBROS_FIJO = 7;
    private List<IBrowserFile> ArchivosSeleccionados = new();
    private string? _archivoResolucionUrl;
    private bool MostrarErrores = false;
    private List<string> erroresFormulario = new();

    // Autocomplete
    private string searchText = "";
    private List<ComiteModel> sugerencias = new();
    private int? ComiteSeleccionadoId;

    private List<ListModel> Cargos = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            CModel = new CambioDirectivaModel();
            editContext = new EditContext(CModel);
        
            // 🔹 CORRECCIÓN: Inicializar listas antes de usarlas
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
            // 🔹 CORRECCIÓN: Manejar la excepción
            Console.WriteLine($"Error en OnInitializedAsync: {ex.Message}");
            MensajeError = "Error al inicializar el formulario";
        }
    }

    private async Task CargarListasIniciales()
    {
        try
        {
            // 🔹 CORRECCIÓN: Validar que el servicio esté inyectado
            if (CommonService == null)
            {
                throw new InvalidOperationException("CommonService no está disponible");
            }
        
            Cargos = await CommonService.GetCargos() ?? new List<ListModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar listas: {ex.Message}");
            Cargos = new List<ListModel>();
            MensajeError = "Error al cargar la lista de cargos";
        }
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
            // 🔹 CORRECCIÓN CRÍTICA: Asignar el NombreComiteSalud al modelo
            CModel.ComiteBaseId = comiteCompleto.ComiteId;
            CModel.NombreComiteSalud = comiteCompleto.NombreComiteSalud; // ¡ESTA LÍNEA FALTABA!
        
            ComiteSeleccionadoId = comiteCompleto.ComiteId;
            MensajeBusqueda = "✅ Comité seleccionado. Ahora agregue la nueva junta directiva.";
        }

        StateHasChanged();
    }

    private async Task HandleValidSubmit()
    {
        IsSubmitting = true;
        MensajeError = "";
        MensajeExito = "";
        MostrarErrores = true;
        
        // 🔹 CORRECCIÓN: Validación adicional para NombreComiteSalud
        if (string.IsNullOrWhiteSpace(CModel.NombreComiteSalud))
        {
            MensajeError = "Error: El nombre del comité no se asignó correctamente.";
            IsSubmitting = false;
            return;
        }

        // Validación específica para Cambio de Directiva
        if (ComiteSeleccionadoId == null)
        {
            MensajeError = "Debe seleccionar un comité con personería.";
            IsSubmitting = false;
            return;
        }

        try
        {
            // Validar miembros
            if (CModel.Miembros.Count != NUMERO_MIEMBROS_FIJO)
            {
                MensajeError = $"Debe haber exactamente {NUMERO_MIEMBROS_FIJO} miembros.";
                IsSubmitting = false;
                return;
            }

            foreach (var m in CModel.Miembros)
            {
                if (string.IsNullOrWhiteSpace(m.NombreMiembro) ||
                    string.IsNullOrWhiteSpace(m.ApellidoMiembro) ||
                    string.IsNullOrWhiteSpace(m.CedulaMiembro) ||
                    m.CargoId == 0)
                {
                    MensajeError = "Todos los miembros deben tener nombre, apellido, cédula y cargo.";
                    IsSubmitting = false;
                    return;
                }
            }

            if (!ArchivosSeleccionados.Any())
            {
                MensajeError = "Debe adjuntar al menos un archivo de resolución.";
                IsSubmitting = false;
                return;
            }

            if (string.IsNullOrEmpty(CModel.CreadaPor))
                await ObtenerUsuarioActual();

            var result = await RegistroComiteService.RegistrarCambioDirectiva(CModel);

            if (!result.Success)
            {
                MensajeError = $"Error al registrar cambio: {result.Message}";
                IsSubmitting = false;
                return;
            }

            foreach (var archivo in ArchivosSeleccionados)
            {
                await ArchivoLegalService.GuardarArchivoComiteAsync(result.Id, archivo, "RESOLUCION CAMBIO DIRECTIVA");
            }

            MensajeExito = "✅ Cambio de Junta Directiva registrado exitosamente!";
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

    private string SearchText
    {
        get => searchText;
        set
        {
            searchText = value;
            _ = BuscarComitesAsync();
        }
    }

    private void DesplegarMiembros()
    {
        if (CModel.Miembros.Count >= NUMERO_MIEMBROS_FIJO)
        {
            miembrosDesplegados = true;
        }
        else
        {
            miembrosDesplegados = true;
            int faltantes = NUMERO_MIEMBROS_FIJO - CModel.Miembros.Count;
            for (int i = 0; i < faltantes; i++)
            {
                CModel.Miembros.Add(new MiembroComiteModel());
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
        if (comite != null && comite.TipoTramiteEnum == TipoTramite.CambioDirectiva)
        {
            // Convertir ComiteModel a CambioDirectivaModel
            CModel.ComiteId = comite.ComiteId;
            CModel.ComiteBaseId = comite.ComiteBaseId ?? 0;
            CModel.NumeroResolucion = comite.NumeroResolucion;
            CModel.FechaResolucion = comite.FechaResolucion;
            CModel.FechaEleccion = comite.FechaEleccion ?? DateTime.Now;
            CModel.CreadaPor = comite.CreadaPor;
            CModel.Miembros = comite.Miembros;

            miembrosDesplegados = true;
            ComiteSeleccionadoId = comite.ComiteId;
            await CargarListasIniciales();
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

        // 3. 🔹 CORRECCIÓN: Validar que el nombre del comité no sea NULL
        if (string.IsNullOrWhiteSpace(CModel.NombreComiteSalud))
            return false;

        // 4. Validaciones básicas
        if (string.IsNullOrWhiteSpace(CModel.NumeroResolucion) ||
            CModel.FechaEleccion == default)
            return false;

        // 5. Validar miembros
        if (CModel.Miembros.Count != NUMERO_MIEMBROS_FIJO)
            return false;

        // 6. Validar que todos los miembros tengan datos
        if (CModel.Miembros.Any(m => 
                string.IsNullOrWhiteSpace(m.NombreMiembro) ||
                string.IsNullOrWhiteSpace(m.ApellidoMiembro) ||
                string.IsNullOrWhiteSpace(m.CedulaMiembro) ||
                m.CargoId == 0))
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