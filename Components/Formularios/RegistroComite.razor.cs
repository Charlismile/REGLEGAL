using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Formularios
{
    public partial class RegistroComite : ComponentBase
    {
        private ComiteModel CModel { get; set; } = new();
        private EditContext editContext;
        private ValidationMessageStore messageStore;
        private int? comiteSeleccionadoId;

        private string UserName { get; set; } = "";
        private bool IsSubmitting = false;

        private IBrowserFile? _archivoResolucion;
        private List<IBrowserFile> ArchivosSeleccionados = new(); // ✅ archivos pendientes por guardar

        private List<ListModel> comiteRegioneslist = new();
        private List<ListModel> comiteProvinciaList = new();
        private List<ListModel> comiteDistritoList = new();
        private List<ListModel> comiteCorregimientoList = new();
        private List<ComiteModel> comitesRegistrados = new();
        private List<ListModel> Cargos = new();

        [Inject] private ILogger<RegistroComite> _logger { get; set; } = default!;

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

            await CargarUbicacion(CModel);
        }

        private async Task CargarUbicacion(ComiteModel modelo)
        {
            if (modelo.RegionSaludId.HasValue && modelo.RegionSaludId != 0)
                comiteProvinciaList = await _Commonservice.GetProvincias(modelo.RegionSaludId.Value);
            if (modelo.ProvinciaId.HasValue && modelo.ProvinciaId != 0)
                comiteDistritoList = await _Commonservice.GetDistritos(modelo.ProvinciaId.Value);
            if (modelo.DistritoId.HasValue && modelo.DistritoId != 0)
                comiteCorregimientoList = await _Commonservice.GetCorregimientos(modelo.DistritoId.Value);
        }

        private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
        {
            messageStore.Clear();
            int maxMiembros = CModel.TipoTramiteEnum == TipoTramite.JuntaInterventora ? 2 : 7;

            if (CModel.Miembros == null || CModel.Miembros.Count != maxMiembros ||
                CModel.Miembros.Any(m => string.IsNullOrWhiteSpace(m.NombreMiembro) ||
                                         string.IsNullOrWhiteSpace(m.ApellidoMiembro) ||
                                         string.IsNullOrWhiteSpace(m.CedulaMiembro) ||
                                         m.CargoId == null || m.CargoId == 0))
            {
                messageStore.Add(() => CModel.Miembros,
                    $"Debe registrar exactamente {maxMiembros} miembros con todos sus datos.");
            }

            if (CModel.TipoTramiteEnum == TipoTramite.Personeria)
            {
                if (!CModel.RegionSaludId.HasValue || CModel.RegionSaludId == 0)
                    messageStore.Add(() => CModel.RegionSaludId, "La región es obligatoria.");
                if (!CModel.ProvinciaId.HasValue || CModel.ProvinciaId == 0)
                    messageStore.Add(() => CModel.ProvinciaId, "La provincia es obligatoria.");
                if (!CModel.DistritoId.HasValue || CModel.DistritoId == 0)
                    messageStore.Add(() => CModel.DistritoId, "El distrito es obligatorio.");
                if (!CModel.CorregimientoId.HasValue || CModel.CorregimientoId == 0)
                    messageStore.Add(() => CModel.CorregimientoId, "El corregimiento es obligatorio.");
            }

            if (_archivoResolucion != null)
            {
                var allowedExtensions = new[] { ".pdf", ".docx" };
                var ext = Path.GetExtension(_archivoResolucion.Name).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                    messageStore.Add(() => _archivoResolucion, "Formato no permitido. Solo PDF o DOCX.");
                if (_archivoResolucion.Size > 50 * 1024 * 1024)
                    messageStore.Add(() => _archivoResolucion, "El archivo no puede superar 50 MB.");
            }
        }
        
        private async Task HandleValidSubmit()
        {
            IsSubmitting = true;
            try
            {
                CModel.CreadaPor = UserName;

                var result = await RegistroComiteService.CrearComite(CModel);
                if (!result.Success || result.Id == 0)
                    return;

                // Guardar historial miembros si aplica
                if (CModel.TipoTramiteEnum == TipoTramite.CambioDirectiva ||
                    CModel.TipoTramiteEnum == TipoTramite.JuntaInterventora)
                {
                    await RegistroComiteService.GuardarHistorialMiembros(result.Id, CModel.Miembros);
                }

                // Guardar miembros
                foreach (var miembro in CModel.Miembros)
                {
                    await RegistroComiteService.AgregarMiembro(result.Id, miembro);
                }

                // Guardar resolución
                if (_archivoResolucion != null)
                {
                    var archivoResult = await RegistroComiteService.GuardarResolucionAsync(result.Id, _archivoResolucion);
                    if (!archivoResult.Success)
                        return;

                    CModel.NumeroResolucion = _archivoResolucion.Name;
                }

                // Guardar documentos adicionales
                foreach (var archivo in ArchivosSeleccionados)
                {
                    var saved = await ArchivoLegalService.GuardarArchivoComiteAsync(result.Id, archivo, "DocumentosComite");
                    if (saved != null)
                        CModel.Archivos.Add(saved);
                }

                ArchivosSeleccionados.Clear();
                Navigation.NavigateTo("/comites");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el comité");
            }
            finally
            {
                IsSubmitting = false;
            }
        }
        
        private async Task CargarDocumentos(InputFileChangeEventArgs e)
        {
            foreach (var file in e.GetMultipleFiles())
            {
                if (file != null)
                    ArchivosSeleccionados.Add(file);
            }
            await InvokeAsync(StateHasChanged);
        }

        private async Task RemoverDocumentoAsync(CArchivoModel archivo)
        {
            if (archivo.ComiteArchivoId != 0)
            {
                var eliminado = await ArchivoLegalService.DesactivarArchivoComiteAsync(archivo.ComiteArchivoId);
                if (eliminado)
                    CModel.Archivos.Remove(archivo);
            }
            await InvokeAsync(StateHasChanged);
        }

        private void RemoverArchivoPendiente(IBrowserFile archivo)
        {
            ArchivosSeleccionados.Remove(archivo);
        }
        
        private async Task AgregarMiembroAlModelo()
        {
            int maxMiembros = CModel.TipoTramiteEnum == TipoTramite.JuntaInterventora ? 2 : 7;
            if (CModel.Miembros.Count >= maxMiembros) return;

            var miembro = new MiembroComiteModel();
            CModel.Miembros.Add(miembro);

            if (CModel.ComiteId != 0)
                await RegistroComiteService.AgregarMiembro(CModel.ComiteId, miembro);
        }

        private async Task RemoverMiembro(MiembroComiteModel miembro)
        {
            if (CModel.Miembros.Contains(miembro))
            {
                CModel.Miembros.Remove(miembro);
                if (miembro.MiembroId != 0)
                    await RegistroComiteService.EliminarMiembro(miembro.MiembroId);
            }
        }
        
        private async Task OnTipoTramiteChanged()
        {
            if (CModel.TipoTramiteEnum == TipoTramite.Personeria)
            {
                CModel.NombreComiteSalud = "";
                CModel.Comunidad = "";
            }
            await InvokeAsync(StateHasChanged);
        }

        private async Task RegionChanged(int regionId)
        {
            CModel.RegionSaludId = regionId;
            comiteProvinciaList = await _Commonservice.GetProvincias(regionId);
            CModel.ProvinciaId = null;
            CModel.DistritoId = null;
            CModel.CorregimientoId = null;
            comiteDistritoList.Clear();
            comiteCorregimientoList.Clear();
        }

        private async Task ProvinciaChanged(int provinciaId)
        {
            CModel.ProvinciaId = provinciaId;
            comiteDistritoList = await _Commonservice.GetDistritos(provinciaId);
            CModel.DistritoId = null;
            CModel.CorregimientoId = null;
            comiteCorregimientoList.Clear();
        }

        private async Task DistritoChanged(int distritoId)
        {
            CModel.DistritoId = distritoId;
            comiteCorregimientoList = await _Commonservice.GetCorregimientos(distritoId);
            CModel.CorregimientoId = null;
        }

        private async Task<AutoCompleteDataProviderResult<ComiteModel>> BuscarComites(AutoCompleteDataProviderRequest<ComiteModel> request)
        {
            var filtro = (request.Filter?.Value?.ToString() ?? "").ToUpper();

            var filtradas = comitesRegistrados
                .Where(c => c.NombreComiteSalud != null && c.NombreComiteSalud.ToUpper().Contains(filtro))
                .ToList();

            return new AutoCompleteDataProviderResult<ComiteModel>
            {
                Data = filtradas,
                TotalCount = filtradas.Count
            };
        }

        private void OnComiteSeleccionado(int? comiteId)
        {
            if (comiteId.HasValue)
            {
                var comiteSeleccionado = comitesRegistrados.FirstOrDefault(c => c.ComiteId == comiteId.Value);
                if (comiteSeleccionado != null)
                {
                    CModel.ComiteId = comiteSeleccionado.ComiteId;
                    CModel.NombreComiteSalud = comiteSeleccionado.NombreComiteSalud;
                    CModel.Comunidad = comiteSeleccionado.Comunidad;
                    CModel.RegionSaludId = comiteSeleccionado.RegionSaludId;
                    CModel.ProvinciaId = comiteSeleccionado.ProvinciaId;
                    CModel.DistritoId = comiteSeleccionado.DistritoId;
                    CModel.CorregimientoId = comiteSeleccionado.CorregimientoId;
                
                    // Cargar la ubicación cuando se selecciona un comité
                    _ = CargarUbicacion(CModel);
                }
            }
            else
            {
                // Limpiar si no hay selección
                CModel.ComiteId = 0;
                CModel.NombreComiteSalud = "";
            }
            StateHasChanged();
        }

        private void Cancelar() => Navigation.NavigateTo("/comites");
    }
}
