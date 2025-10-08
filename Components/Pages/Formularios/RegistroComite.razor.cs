using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;
using REGISTROLEGAL.Repositories.Services;

namespace REGISTROLEGAL.Components.Pages.Formularios
{
    public partial class RegistroComite : ComponentBase
    {
        [Inject] private IRegistroComite RegistroComiteService { get; set; } = default!;
        [Inject] private ICommon _Commonservice { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IArchivoLegalService ArchivoLegalService { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        private ComiteModel CModel { get; set; } = new();
        private EditContext editContext;
        private ValidationMessageStore messageStore;
        private int? comiteSeleccionadoId;
        private string? _archivoResolucionUrl;
        private bool miembrosDesplegados = false;
        private const int NUMERO_MIEMBROS_FIJO = 7;
        private string UserName { get; set; } = "";
        private bool IsSubmitting = false;

        private IBrowserFile? _archivoResolucion;
        private List<IBrowserFile> ArchivosSeleccionados = new();

        private List<ListModel> comiteRegioneslist = new();
        private List<ListModel> comiteProvinciaList = new();
        private List<ListModel> comiteDistritoList = new();
        private List<ListModel> comiteCorregimientoList = new();
        private List<ComiteModel> comitesRegistrados = new();
        private List<ListModel> Cargos = new();

        // Métodos del formulario
        protected override async Task OnInitializedAsync()
        {
            editContext = new EditContext(CModel);
            messageStore = new ValidationMessageStore(editContext);
            await CargarListasIniciales();
        }

        private async Task HandleValidSubmit()
        {
            // Validar que todos los miembros tengan datos completos
            if (miembrosDesplegados && CModel.Miembros.Any(m => 
                    string.IsNullOrEmpty(m.NombreMiembro) || 
                    string.IsNullOrEmpty(m.ApellidoMiembro) ||
                    string.IsNullOrEmpty(m.CedulaMiembro) ||
                    m.CargoId == 0)) // Asumiendo que CargoId es int (no int?)
            {
                // Mostrar mensaje de error
                messageStore.Add(FieldIdentifier.Create(() => CModel.Miembros),
                    new[] { "Todos los campos de los 7 miembros son obligatorios." });
                editContext.NotifyValidationStateChanged();
                IsSubmitting = false;
                return;
            }

            IsSubmitting = true;
            CModel.CreadaPor = UserName;

            try
            {
                // Validar que hay archivos antes de proceder
                if (!ArchivosSeleccionados.Any())
                {
                    messageStore.Add(FieldIdentifier.Create(() => _archivoResolucion),
                        new[] { "Debe seleccionar al menos un archivo de resolución." });
                    editContext.NotifyValidationStateChanged();
                    IsSubmitting = false;
                    return;
                }

                var result = await RegistroComiteService.CrearComite(CModel);
                if (!result.Success)
                {
                    Console.WriteLine($"Error creando comité: {result.Message}");
                    IsSubmitting = false;
                    return;
                }

                Console.WriteLine($"Comité creado con ID: {result.Id}");

                // Guardar archivos en BD
                await GuardarArchivos(result.Id);

                Navigation.NavigateTo("/listado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en el proceso: {ex.Message}");
                IsSubmitting = false;
            }
        }

        private async Task CargarListasIniciales()
        {
            comiteRegioneslist = await _Commonservice.GetRegiones();
            Cargos = await _Commonservice.GetCargos();
        }

        private async Task OnTipoTramiteChanged()
        {
            if (CModel.TipoTramiteEnum is TipoTramite.CambioDirectiva or TipoTramite.JuntaInterventora)
                comitesRegistrados = await RegistroComiteService.ObtenerComites();
        }

        private void DesplegarMiembros()
        {
            if (CModel.Miembros.Count >= NUMERO_MIEMBROS_FIJO)
            {
                // Ya hay suficientes miembros, solo mostrar
                miembrosDesplegados = true;
            }
            else
            {
                miembrosDesplegados = true;

                // Agregar miembros hasta completar los 7
                int miembrosFaltantes = NUMERO_MIEMBROS_FIJO - CModel.Miembros.Count;
                for (int i = 0; i < miembrosFaltantes; i++)
                {
                    CModel.Miembros.Add(new MiembroComiteModel());
                }
            }

            StateHasChanged();
        }

        private async Task CargarDocumentos(InputFileChangeEventArgs e)
        {
            messageStore.Clear();

            foreach (var archivo in e.GetMultipleFiles())
            {
                // Validar extensión
                var extension = Path.GetExtension(archivo.Name).ToLower();
                if (extension != ".pdf")
                {
                    messageStore.Add(
                        FieldIdentifier.Create(() => _archivoResolucion),
                        new[] { $"El archivo {archivo.Name} debe ser un PDF." }
                    );
                    continue;
                }

                // Validar tipo MIME
                if (archivo.ContentType != "application/pdf")
                {
                    messageStore.Add(
                        FieldIdentifier.Create(() => _archivoResolucion),
                        new[] { $"Archivo {archivo.Name} no es un PDF válido." }
                    );
                    continue;
                }

                // Validar tamaño (10 MB máximo)
                if (archivo.Size > 10 * 1024 * 1024)
                {
                    messageStore.Add(
                        FieldIdentifier.Create(() => _archivoResolucion),
                        new[] { $"Archivo {archivo.Name} excede 10 MB." }
                    );
                    continue;
                }

                if (archivo.Size == 0)
                {
                    messageStore.Add(
                        FieldIdentifier.Create(() => _archivoResolucion),
                        new[] { $"Archivo {archivo.Name} está vacío." }
                    );
                    continue;
                }

                ArchivosSeleccionados.Add(archivo);
                Console.WriteLine($"Archivo agregado: {archivo.Name}, Tamaño: {archivo.Size} bytes");
            }

            editContext.NotifyValidationStateChanged();
            await InvokeAsync(StateHasChanged);
        }

        private void RemoverArchivoPendiente(IBrowserFile archivo)
        {
            ArchivosSeleccionados.Remove(archivo);
        }

        private async Task GuardarArchivos(int comiteId)
        {
            try
            {
                foreach (var archivo in ArchivosSeleccionados)
                {
                    var dto = await ArchivoLegalService.GuardarArchivoComiteAsync(
                        comiteId,
                        archivo,
                        "RESOLUCION" // Usar mayúsculas para consistencia
                    );

                    if (dto != null)
                    {
                        _archivoResolucionUrl = dto.RutaArchivo;
                        Console.WriteLine($"Archivo guardado: {dto.NombreArchivo}, Ruta: {dto.RutaArchivo}");
                    }
                    else
                    {
                        Console.WriteLine("Error: El servicio devolvió null");
                    }
                }

                ArchivosSeleccionados.Clear();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando archivos: {ex.Message}");
                // Aquí podrías mostrar un mensaje al usuario
            }
        }

        private async Task OnComiteSeleccionado(int? comiteId)
        {
            if (comiteId == null) return;

            var comite = comitesRegistrados.FirstOrDefault(c => c.ComiteId == comiteId);
            if (comite != null)
            {
                CModel.NombreComiteSalud = comite.NombreComiteSalud;
                CModel.Comunidad = comite.Comunidad;
                CModel.RegionSaludId = comite.RegionSaludId;
                CModel.ProvinciaId = comite.ProvinciaId;
                CModel.DistritoId = comite.DistritoId;
                CModel.CorregimientoId = comite.CorregimientoId;
                await RegionChanged(CModel.RegionSaludId ?? 0);
                await ProvinciaChanged(CModel.ProvinciaId ?? 0);
                await DistritoChanged(CModel.DistritoId ?? 0);
            }
        }

        private async Task RegionChanged(int regionId)
        {
            CModel.RegionSaludId = regionId;
            comiteProvinciaList = await _Commonservice.GetProvincias(regionId);
            comiteDistritoList.Clear();
            comiteCorregimientoList.Clear();
            CModel.ProvinciaId = null;
            CModel.DistritoId = null;
            CModel.CorregimientoId = null;
        }

        private async Task ProvinciaChanged(int provinciaId)
        {
            CModel.ProvinciaId = provinciaId;
            comiteDistritoList = await _Commonservice.GetDistritos(provinciaId);
            comiteCorregimientoList.Clear();
            CModel.DistritoId = null;
            CModel.CorregimientoId = null;
        }

        private async Task DistritoChanged(int distritoId)
        {
            CModel.DistritoId = distritoId;
            comiteCorregimientoList = await _Commonservice.GetCorregimientos(distritoId);
            CModel.CorregimientoId = null;
        }

        private void Cancelar() => Navigation.NavigateTo("/");
    }
}