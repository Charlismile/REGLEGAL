using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;
using REGISTROLEGAL.Repositories.Services;

namespace REGISTROLEGAL.Components.Pages.Tramites;

public partial class Personeria : ComponentBase
{
    [Parameter] public int ComiteId { get; set; }
    [Inject] IRegistroComite RegistroComiteService { get; set; }
    [Inject] NavigationManager Navigation { get; set; }
    [Inject] IArchivoLegalService ArchivoLegalService { get; set; }
    [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] ICommon CommonService { get; set; }
    private PersoneriaModel CModel { get; set; } = new();
    private EditContext editContext = default!;
    private string MensajeExito = "";
    private string MensajeError = "";
    private bool IsSubmitting = false;
    private bool miembrosDesplegados = false;
    private const int NUMERO_MIEMBROS_FIJO = 7;
    private List<IBrowserFile> ArchivosSeleccionados = new();
    private string? _archivoResolucionUrl;
    private bool MostrarErrores = false;
    private List<string> erroresFormulario = new();
    private ValidationMessageStore messageStore = default!;

    // Listas
    private List<ListModel> Regiones = new();
    private List<ListModel> Provincias = new();
    private List<ListModel> Distritos = new();
    private List<ListModel> Corregimientos = new();
    private List<ListModel> Cargos = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            CModel = new PersoneriaModel();
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

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        messageStore.Clear();

        // Validaciones específicas para Personería
        if (string.IsNullOrWhiteSpace(CModel.NombreComiteSalud))
            messageStore.Add(() => CModel.NombreComiteSalud, "El nombre del comité es obligatorio.");

        if (string.IsNullOrWhiteSpace(CModel.Comunidad))
            messageStore.Add(() => CModel.Comunidad, "El nombre de la comunidad es obligatorio.");

        if (string.IsNullOrWhiteSpace(CModel.NumeroResolucion))
            messageStore.Add(() => CModel.NumeroResolucion, "Debe indicar el número de resolución.");

        if (string.IsNullOrWhiteSpace(CModel.NumeroNota))
            messageStore.Add(() => CModel.NumeroNota, "Debe indicar el número de nota.");

        if (CModel.FechaCreacion == default)
            messageStore.Add(() => CModel.FechaCreacion, "La fecha de creación es obligatoria.");

        // Validar ubicación para Personería
        if (!CModel.RegionSaludId.HasValue)
            messageStore.Add(() => CModel.RegionSaludId, "La región es obligatoria.");

        if (!CModel.ProvinciaId.HasValue)
            messageStore.Add(() => CModel.ProvinciaId, "La provincia es obligatoria.");

        // Validar miembros manualmente
        if (CModel.Miembros.Count != NUMERO_MIEMBROS_FIJO)
        {
            messageStore.Add(() => CModel.Miembros, $"Debe haber exactamente {NUMERO_MIEMBROS_FIJO} miembros.");
        }
        else
        {
            for (int i = 0; i < CModel.Miembros.Count; i++)
            {
                var miembro = CModel.Miembros[i];
                if (string.IsNullOrWhiteSpace(miembro.NombreMiembro))
                    messageStore.Add(() => CModel.Miembros, $"El miembro {i + 1} debe tener un nombre.");

                if (string.IsNullOrWhiteSpace(miembro.ApellidoMiembro))
                    messageStore.Add(() => CModel.Miembros, $"El miembro {i + 1} debe tener un apellido.");

                if (string.IsNullOrWhiteSpace(miembro.CedulaMiembro))
                    messageStore.Add(() => CModel.Miembros, $"El miembro {i + 1} debe tener una cédula.");

                if (miembro.CargoId == 0)
                    messageStore.Add(() => CModel.Miembros, $"El miembro {i + 1} debe tener un cargo.");
            }
        }

        editContext.NotifyValidationStateChanged();
    }

    private async Task CargarListasIniciales()
    {
        Regiones = await CommonService.GetRegiones();
        Cargos = await CommonService.GetCargos();
    }

    private async Task RegionChanged(int? regionId)
    {
        CModel.RegionSaludId = regionId;
        Provincias = regionId.HasValue ? await CommonService.GetProvincias(regionId.Value) : new();
        Distritos = new();
        Corregimientos = new();
        CModel.ProvinciaId = null;
        CModel.DistritoId = null;
        CModel.CorregimientoId = null;
        StateHasChanged();
    }

    private async Task ProvinciaChanged(int? provinciaId)
    {
        CModel.ProvinciaId = provinciaId;
        Distritos = provinciaId.HasValue ? await CommonService.GetDistritos(provinciaId.Value) : new();
        Corregimientos = new();
        CModel.DistritoId = null;
        CModel.CorregimientoId = null;
        StateHasChanged();
    }

    private async Task DistritoChanged(int? distritoId)
    {
        CModel.DistritoId = distritoId;
        Corregimientos = distritoId.HasValue ? await CommonService.GetCorregimientos(distritoId.Value) : new();
        CModel.CorregimientoId = null;
        StateHasChanged();
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
        if (comite != null && comite.TipoTramiteEnum == TipoTramite.Personeria)
        {
            // CORRECCIÓN: Mapear las propiedades del ComiteModel al PersoneriaModel existente
            MapearComiteAPersoneria(comite, CModel);
            miembrosDesplegados = true;
            // Recargar listas para ubicación
            await RegionChanged(CModel.RegionSaludId);
            if (CModel.ProvinciaId.HasValue) await ProvinciaChanged(CModel.ProvinciaId);
            if (CModel.DistritoId.HasValue) await DistritoChanged(CModel.DistritoId);
        }
    }

    // CORRECCIÓN: Método para mapear ComiteModel a PersoneriaModel existente
    private void MapearComiteAPersoneria(ComiteModel comite, PersoneriaModel personeria)
    {
        personeria.ComiteId = comite.ComiteId;
        personeria.UsuarioId = comite.UsuarioId;
        personeria.CreadaPor = comite.CreadaPor;
        personeria.EstadoId = comite.EstadoId;
        personeria.NumeroResolucion = comite.NumeroResolucion;
        personeria.FechaResolucion = comite.FechaResolucion;
        personeria.DocumentosSubir = comite.DocumentosSubir;
        personeria.Archivos = comite.Archivos;
        personeria.TipoTramiteEnum = comite.TipoTramiteEnum;
        personeria.NombreComiteSalud = comite.NombreComiteSalud;
        personeria.Comunidad = comite.Comunidad;
        personeria.FechaCreacion = comite.FechaCreacion ?? DateTime.Now;
        personeria.FechaEleccion = comite.FechaEleccion;
        personeria.NumeroNota = comite.NumeroNota;
        personeria.RegionSaludId = comite.RegionSaludId;
        personeria.ProvinciaId = comite.ProvinciaId;
        personeria.DistritoId = comite.DistritoId;
        personeria.CorregimientoId = comite.CorregimientoId;
        personeria.Miembros = comite.Miembros;
        personeria.MiembrosInterventores = comite.MiembrosInterventores;
        personeria.Historial = comite.Historial;
        personeria.CedulaFile = comite.CedulaFile;
        personeria.CedulaPreviewUrl = comite.CedulaPreviewUrl;
        personeria.PasaporteFile = comite.PasaporteFile;
        personeria.PasaportePreviewUrl = comite.PasaportePreviewUrl;
        personeria.ComiteBaseId = comite.ComiteBaseId;
    }

    private async Task OnSubmit()
    {
        MostrarErrores = true;
        if (!editContext.Validate()) return;
        await HandleValidSubmit();
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

        var isValid = editContext.Validate();
        if (!isValid)
        {
            MensajeError = "Por favor, complete todos los campos obligatorios antes de enviar.";
            return;
        }

        try
        {
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

            // 🔹 USAR EL MÉTODO ESPECÍFICO PARA PERSONERÍA
            var result = await RegistroComiteService.CrearPersoneria(CModel);

            if (!result.Success)
            {
                MensajeError = $"Error al crear comité: {result.Message}";
                IsSubmitting = false;
                return;
            }

            foreach (var archivo in ArchivosSeleccionados)
            {
                await ArchivoLegalService.GuardarArchivoComiteAsync(result.Id, archivo, "RESOLUCION COMITE");
            }

            MensajeExito = "✅ Registro completado exitosamente!";
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

        // 2. Validaciones básicas del formulario
        if (string.IsNullOrWhiteSpace(CModel.NombreComiteSalud) ||
            string.IsNullOrWhiteSpace(CModel.Comunidad) ||
            string.IsNullOrWhiteSpace(CModel.NumeroResolucion) ||
            string.IsNullOrWhiteSpace(CModel.NumeroNota) ||
            CModel.FechaCreacion == default ||
            !CModel.RegionSaludId.HasValue ||
            !CModel.ProvinciaId.HasValue)
            return false;

        // 3. Validar miembros
        if (CModel.Miembros.Count != NUMERO_MIEMBROS_FIJO)
            return false;

        // 4. Validar que todos los miembros tengan datos
        if (CModel.Miembros.Any(m => 
                string.IsNullOrWhiteSpace(m.NombreMiembro) ||
                string.IsNullOrWhiteSpace(m.ApellidoMiembro) ||
                string.IsNullOrWhiteSpace(m.CedulaMiembro) ||
                m.CargoId == 0))
            return false;

        // 5. Validar archivos
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