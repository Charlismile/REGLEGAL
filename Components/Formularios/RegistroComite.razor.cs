using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Components.Formularios
{
    public partial class RegistroComite : ComponentBase
    {
        [Inject] private ICommon _Commonservice { get; set; }
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IRegistroComite RegistroComiteService { get; set; } = default!;

        private ComiteModel CModel { get; set; } = new();
        private EditContext editContext;
        private ValidationMessageStore messageStore;
        private string UserName { get; set; } = "";
        private List<ListModel> comiteRegioneslist { get; set; } = new();
        private List<ListModel> comiteProvinciaList { get; set; } = new();
        private List<ListModel> comiteDistritoList { get; set; } = new();
        private List<ListModel> comiteCorregimientoList { get; set; } = new();
        private List<ListModel> Cargos { get; set; } = new();
        private bool IsSubmitting = false;

        // Archivo resolución
        private IBrowserFile _archivoResolucion;
        private string FileName;

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

            if (!CModel.RegionSaludId.HasValue || CModel.RegionSaludId == 0)
                messageStore.Add(() => CModel.RegionSaludId, "La región es obligatoria.");
            if (!CModel.ProvinciaId.HasValue || CModel.ProvinciaId == 0)
                messageStore.Add(() => CModel.ProvinciaId, "La provincia es obligatoria.");
            if (!CModel.DistritoId.HasValue || CModel.DistritoId == 0)
                messageStore.Add(() => CModel.DistritoId, "El distrito es obligatorio.");
            if (!CModel.CorregimientoId.HasValue || CModel.CorregimientoId == 0)
                messageStore.Add(() => CModel.CorregimientoId, "El corregimiento es obligatorio.");

            if (_archivoResolucion != null)
            {
                var allowedExtensions = new[] { ".pdf", ".docx" };
                var ext = System.IO.Path.GetExtension(_archivoResolucion.Name).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                    messageStore.Add(() => _archivoResolucion, "Formato no permitido. Solo PDF o DOCX.");
                if (_archivoResolucion.Size > 10 * 1024 * 1024)
                    messageStore.Add(() => _archivoResolucion, "El archivo no puede superar 10 MB.");
            }
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

        private async Task HandleValidSubmit()
        {
            IsSubmitting = true;
            CModel.CreadaPor = UserName;

            var result = await RegistroComiteService.CrearComite(CModel);

            if (!result.Success || result.Id == 0)
            {
                IsSubmitting = false;
                return;
            }

            // Guardar miembros
            foreach (var miembro in CModel.Miembros)
            {
                await RegistroComiteService.AgregarMiembro(result.Id, miembro);
            }

            // Guardar archivo resolución dentro de HandleValidSubmit
            if (_archivoResolucion != null)
            {
                // 1️⃣ Guardar el archivo físicamente
                var rutaCarpeta = Path.Combine("wwwroot", "Archivos", "Resoluciones");
                if (!Directory.Exists(rutaCarpeta))
                    Directory.CreateDirectory(rutaCarpeta);

                var rutaArchivo = Path.Combine(rutaCarpeta, _archivoResolucion.Name);
                using (var stream = _archivoResolucion.OpenReadStream(10 * 1024 * 1024))
                using (var fileStream = new FileStream(rutaArchivo, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }

                // 2️⃣ Crear modelo para BD
                var archivoModel = new CArchivoModel
                {
                    NombreArchivo = _archivoResolucion.Name,
                    RutaArchivo = rutaArchivo
                };

                // 3️⃣ Llamar a tu método existente AgregarArchivo
                await RegistroComiteService.AgregarArchivo(result.Id, archivoModel);
            }

            Navigation.NavigateTo("/comites");
        }

        private void OnTipoTramiteChanged(ChangeEventArgs e)
        {
            if (Enum.TryParse<TipoTramite>(e.Value?.ToString(), out var tipo))
            {
                CModel.TipoTramiteEnum = tipo;
                if (tipo == TipoTramite.CambioDirectiva || tipo == TipoTramite.JuntaInterventora)
                {
                    // TODa: cargar comité existente si aplica
                }
                else
                {
                    CModel.Miembros.Clear();
                    CModel.MiembrosInterventores.Clear();
                }
            }
        }

        private void AgregarMiembro()
        {
            CModel.Miembros.Add(new MiembroComiteModel());
            StateHasChanged();
        }

        private void RemoverMiembro(int miembroId)
        {
            var miembro = CModel.Miembros.FirstOrDefault(m => m.MiembroId == miembroId);
            if (miembro != null) CModel.Miembros.Remove(miembro);
            StateHasChanged();
        }

        private void Cancelar()
        {
            Navigation.NavigateTo("/");
        }

        private void OnFileSelected(InputFileChangeEventArgs e)
        {
            _archivoResolucion = e.File;
            FileName = _archivoResolucion.Name;
        }
    }
}