using System.Security.Claims;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Models.LegalModels;
using REGISTROLEGAL.Repositories.Interfaces;
using REGISTROLEGAL.Repositories.Services;

namespace REGISTROLEGAL.Components.Formularios
{
    public partial class RegistroComite : ComponentBase
    {
        [Inject] private ICommon _Commonservice { get; set; } = default!;
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
        private List<ComiteModel> comitesRegistrados { get; set; } = new();
        private List<ListModel> Cargos { get; set; } = new();

        private string selectedComiteNombre
        {
            get => CModel.NombreComiteSalud;
            set => CModel.NombreComiteSalud = value;
        }


        private bool IsSubmitting = false;

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

            // Cargos
            Cargos = (await _Commonservice.GetCargos()).ToList();

            // Regiones
            comiteRegioneslist = await _Commonservice.GetRegiones();

            // Si ya hay Region seleccionada (ej. edición de comité), cargamos provincias, distritos y corregimientos
            if (CModel.RegionSaludId.HasValue && CModel.RegionSaludId != 0)
            {
                comiteProvinciaList = await _Commonservice.GetProvincias(CModel.RegionSaludId.Value);
            }

            if (CModel.ProvinciaId.HasValue && CModel.ProvinciaId != 0)
            {
                comiteDistritoList = await _Commonservice.GetDistritos(CModel.ProvinciaId.Value);
            }

            if (CModel.DistritoId.HasValue && CModel.DistritoId != 0)
            {
                comiteCorregimientoList = await _Commonservice.GetCorregimientos(CModel.DistritoId.Value);
            }
        }


        private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
        {
            messageStore.Clear();

            // Validar miembros según el tipo de trámite
            if (CModel.TipoTramiteEnum == TipoTramite.Personeria ||
                CModel.TipoTramiteEnum == TipoTramite.CambioDirectiva)
            {
                if (CModel.Miembros == null || CModel.Miembros.Count != 7 ||
                    CModel.Miembros.Any(m => string.IsNullOrWhiteSpace(m.NombreMiembro) ||
                                             string.IsNullOrWhiteSpace(m.ApellidoMiembro) ||
                                             string.IsNullOrWhiteSpace(m.CedulaMiembro) ||
                                             m.CargoId == null || m.CargoId == 0))
                {
                    messageStore.Add(() => CModel.Miembros, "Debe registrar los 7 miembros con todos sus datos.");
                }
            }
            else if (CModel.TipoTramiteEnum == TipoTramite.JuntaInterventora)
            {
                if (CModel.Miembros == null || CModel.Miembros.Count != 2 ||
                    CModel.Miembros.Any(m => string.IsNullOrWhiteSpace(m.NombreMiembro) ||
                                             string.IsNullOrWhiteSpace(m.ApellidoMiembro) ||
                                             string.IsNullOrWhiteSpace(m.CedulaMiembro) ||
                                             m.CargoId == null || m.CargoId == 0))
                {
                    messageStore.Add(() => CModel.Miembros,
                        "Debe registrar exactamente 2 miembros (Presidente y Tesorero) con todos sus datos.");
                }
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
                    messageStore.Add(() => _archivoResolucion, "El archivo no puede superar 10 MB.");
            }
        }

        private async Task RegionChanged(int id)
        {
            CModel.RegionSaludId = id;

            // Cargar provincias de la región seleccionada
            comiteProvinciaList = await _Commonservice.GetProvincias(id);
            CModel.ProvinciaId = null;

            // Limpiar distritos y corregimientos
            comiteDistritoList.Clear();
            CModel.DistritoId = null;
            comiteCorregimientoList.Clear();
            CModel.CorregimientoId = null;
        }

        private async Task ProvinciaChanged(int id)
        {
            CModel.ProvinciaId = id;

            // Cargar distritos de la provincia seleccionada
            comiteDistritoList = await _Commonservice.GetDistritos(id);
            CModel.DistritoId = null;

            // Limpiar corregimientos
            comiteCorregimientoList.Clear();
            CModel.CorregimientoId = null;
        }

        private async Task DistritoChanged(int id)
        {
            CModel.DistritoId = id;

            // Cargar corregimientos del distrito seleccionado
            comiteCorregimientoList = await _Commonservice.GetCorregimientos(id);
            CModel.CorregimientoId = null;
        }


        private async Task HandleValidSubmit()
        {
            IsSubmitting = true;
            CModel.CreadaPor = UserName;

            // 1. Crear/actualizar comité
            var result = await RegistroComiteService.CrearComite(CModel);
            if (!result.Success || result.Id == 0)
            {
                IsSubmitting = false;
                return;
            }

            // 2. Guardar historial si aplica
            if (CModel.TipoTramiteEnum == TipoTramite.CambioDirectiva ||
                CModel.TipoTramiteEnum == TipoTramite.JuntaInterventora)
            {
                await RegistroComiteService.GuardarHistorialMiembros(result.Id, CModel.Miembros);
            }

            // 3. Guardar miembros actuales
            foreach (var miembro in CModel.Miembros)
            {
                await RegistroComiteService.AgregarMiembro(result.Id, miembro);
            }

            // 4. Guardar resolución si hay
            if (_archivoResolucion != null)
            {
                var archivoResult = await RegistroComiteService.GuardarResolucionAsync(result.Id, _archivoResolucion);
                if (!archivoResult.Success)
                {
                    IsSubmitting = false;
                    return;
                }
            }
            
            // Guardar los documentos asociados al comité
            foreach (var archivo in CModel.DocumentosSubir)
            {
                await _Commonservice.GuardarArchivoComiteAsync(
                    archivo,
                    categoria: "DocumentosComite",
                    comiteId: result.Id  
                );
            }

            Navigation.NavigateTo("/comites");
        }

        private async Task OnTipoTramiteChanged()
        {
            var tipo = CModel.TipoTramiteEnum; 

            switch (tipo)
            {
                case TipoTramite.Personeria:
                    CModel.Miembros = new List<MiembroComiteModel>();
                    var cargosFijos = await _Commonservice.GetCargos();
                    foreach (var cargo in cargosFijos)
                    {
                        CModel.Miembros.Add(new MiembroComiteModel
                        {
                            CargoId = cargo.Id,
                            NombreCargo = cargo.Name
                        });
                    }

                    break;

                case TipoTramite.CambioDirectiva:
                case TipoTramite.JuntaInterventora:
                    comitesRegistrados = await RegistroComiteService.ObtenerComites();
                    CModel.ComiteId = 0;
                    CModel.Miembros = new List<MiembroComiteModel>(); // limpiamos miembros hasta seleccionar comité
                    break;
            }
        }

        // Método para filtrar comités según el texto de búsqueda
        private async Task<AutoCompleteDataProviderResult<ComiteModel>> AutoCompleteComiteDataProvider(
            AutoCompleteDataProviderRequest<ComiteModel> request)
        {
            var filtro = (request.Filter?.Value?.ToString() ?? "").ToUpper();

            var comites = await RegistroComiteService.ObtenerComites();

            var filtrados = comites
                .Where(c => !string.IsNullOrEmpty(c.NombreComiteSalud) &&
                            c.NombreComiteSalud.ToUpper().Contains(filtro))
                .ToList();

            return new AutoCompleteDataProviderResult<ComiteModel>
            {
                Data = filtrados,
                TotalCount = filtrados.Count
            };
        }

        private async Task OnAutoCompleteComiteChanged(ComiteModel? sel)
        {
            if (sel != null)
            {
                CModel.ComiteId = sel.ComiteId;
                CModel.NombreComiteSalud = sel.NombreComiteSalud;
                await CargarDatosComite(sel.ComiteId);
            }
            else
            {
                // Limpiar selección
                CModel.ComiteId = 0;
                CModel.NombreComiteSalud = string.Empty;
                CModel.Comunidad = string.Empty;
                CModel.Miembros.Clear();
            }
        }

// Método para cargar datos completos del comité seleccionado
        private async Task CargarDatosComite(int comiteId)
        {
            var comite = await RegistroComiteService.ObtenerComiteCompletoAsync(comiteId);
            if (comite != null)
            {
                CModel.NombreComiteSalud = comite.NombreComiteSalud;
                CModel.Comunidad = comite.Comunidad;
                CModel.RegionSaludId = comite.RegionSaludId;
                CModel.ProvinciaId = comite.ProvinciaId;
                CModel.DistritoId = comite.DistritoId;
                CModel.CorregimientoId = comite.CorregimientoId;

                // Cargar miembros
                CModel.Miembros = comite.Miembros?.Select(m => new MiembroComiteModel
                {
                    MiembroId = m.MiembroId,
                    ComiteId = m.ComiteId,
                    NombreMiembro = m.NombreMiembro,
                    ApellidoMiembro = m.ApellidoMiembro,
                    CedulaMiembro = m.CedulaMiembro,
                    CargoId = m.CargoId,
                    NombreCargo = m.NombreCargo,
                    TelefonoMiembro = m.TelefonoMiembro,
                    CorreoMiembro = m.CorreoMiembro
                }).ToList() ?? new List<MiembroComiteModel>();
            }
        }

        // Cargar documentos al modelo de comité
        private async Task CargarDocumentos(InputFileChangeEventArgs e)
        {
            foreach (var archivo in e.GetMultipleFiles())
            {
                CModel.DocumentosSubir.Add(archivo);
            }

            await InvokeAsync(StateHasChanged);
        }

// Remover un documento cargado
        private void RemoverDocumento(IBrowserFile archivo)
        {
            CModel.DocumentosSubir.Remove(archivo);
        }

        private void AgregarMiembro()
        {
            CModel.Miembros.Add(new MiembroComiteModel());
        }

        private void RemoverMiembro(MiembroComiteModel miembro)
        {
            if (CModel.Miembros.Contains(miembro))
                CModel.Miembros.Remove(miembro);
        }

        private void Cancelar() => Navigation.NavigateTo("/comites");

        private void OnFileSelected(InputFileChangeEventArgs e)
        {
            _archivoResolucion = e.File;
            FileName = e.File.Name;
        }
    }
}