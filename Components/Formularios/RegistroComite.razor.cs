using System.Security.Claims;
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
        [Inject] private IRegistroComite ComiteService { get; set; } = default!;


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
                if (_archivoResolucion.Size > 10 * 1024 * 1024)
                    messageStore.Add(() => _archivoResolucion, "El archivo no puede superar 10 MB.");
            }
        }

        private async Task RegionChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int id))
                await RegionChanged(id);
        }

        private async Task ProvinciaChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int id))
                await ProvinciaChanged(id);
        }

        private async Task DistritoChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int id))
                await DistritoChanged(id);
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
                await GuardarResolucionEnDisco(result.Id);
            }

            Navigation.NavigateTo("/comites");
        }


        private async Task OnTipoTramiteChanged(ChangeEventArgs e)
        {
            if (Enum.TryParse<TipoTramite>(e.Value?.ToString(), out var tipo))
            {
                CModel.TipoTramiteEnum = tipo;

                switch (tipo)
                {
                    case TipoTramite.Personeria:
                        // Inicializamos con 7 miembros vacíos
                        CModel.Miembros = new List<MiembroComiteModel>();
                        var cargosFijos = await _Commonservice.GetCargosPrincipales(); // Presidente, Vice, etc.
                        foreach (var cargo in cargosFijos)
                        {
                            CModel.Miembros.Add(new MiembroComiteModel
                            {
                                CargoId = cargo.Id,
                                NombreCargo = cargo.Nombre
                            });
                        }

                        break;

                    case TipoTramite.CambioDirectiva:
                        var ultimoComite = await ComiteService.ObtenerUltimoComiteConMiembrosAsync();
                        if (ultimoComite != null)
                        {
                            CModel.NombreComiteSalud = ultimoComite.NombreComiteSalud;
                            CModel.Comunidad = ultimoComite.Comunidad;
                            CModel.RegionSaludId = ultimoComite.RegionSaludId ?? 0;
                            CModel.ProvinciaId = ultimoComite.ProvinciaId ?? 0;
                            CModel.DistritoId = ultimoComite.DistritoId ?? 0;
                            CModel.CorregimientoId = ultimoComite.CorregimientoId ?? 0;

                            // Copiamos miembros actuales
                            CModel.Miembros = ultimoComite.Miembros.ToList();
                        }

                        break;

                    case TipoTramite.JuntaInterventora:
                        var comite = await ComiteService.ObtenerUltimoComiteConMiembrosAsync();
                        if (comite != null)
                        {
                            CModel.NombreComiteSalud = comite.NombreComiteSalud;
                            CModel.Comunidad = comite.Comunidad;

                            // Solo presidente y tesorero
                            CModel.Miembros = comite.Miembros
                                .Where(m => m.CargoId == 1 || m.CargoId == 5) // ej: 1=Presidente, 5=Tesorero
                                .ToList();
                        }

                        break;
                }
            }
        }


        private void AgregarMiembro()
        {
            CModel.Miembros.Add(new MiembroComiteModel());
        }

        private void RemoverMiembro(int miembroId)
        {
            var miembro = CModel.Miembros.FirstOrDefault(x => x.MiembroId == miembroId);
            if (miembro != null)
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