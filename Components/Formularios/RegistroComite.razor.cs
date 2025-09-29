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
        
        private ComiteModel CModel { get; set; } = new();
        private EditContext editContext;
        private ValidationMessageStore messageStore;
        private string UserName { get; set; } = "";
        private List<ListModel> comiteRegioneslist { get; set; } = new();
        private List<ListModel> comiteProvinciaList { get; set; } = new();
        private List<ListModel> comiteDistritoList { get; set; } = new();
        private List<ListModel> comiteCorregimientoList { get; set; } = new();
        private List<ListModel> Cargos { get; set; } = new();
        private List<ComiteModel> comitesRegistrados = new();
        
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
            if (CModel.TipoTramiteEnum == TipoTramite.Personeria || CModel.TipoTramiteEnum == TipoTramite.CambioDirectiva)
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
                    messageStore.Add(() => CModel.Miembros, "Debe registrar exactamente 2 miembros (Presidente y Tesorero) con todos sus datos.");
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

        // private async Task RegionChanged(ChangeEventArgs e)
        // {
        //     if (int.TryParse(e.Value?.ToString(), out int id))
        //         await RegionChanged(id);
        // }
        //
        // private async Task ProvinciaChanged(ChangeEventArgs e)
        // {
        //     if (int.TryParse(e.Value?.ToString(), out int id))
        //         await ProvinciaChanged(id);
        // }
        //
        // private async Task DistritoChanged(ChangeEventArgs e)
        // {
        //     if (int.TryParse(e.Value?.ToString(), out int id))
        //         await DistritoChanged(id);
        // }

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
                        var ultimoComite = await RegistroComiteService.ObtenerUltimoComiteConMiembrosAsync();
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
                        var comite = await RegistroComiteService.ObtenerUltimoComiteConMiembrosAsync();
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
        
        private async Task ComiteSeleccionado(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int comiteId) && comiteId > 0)
            {
                var comite = await RegistroComiteService.ObtenerComiteCompletoAsync(comiteId);
                if (comite != null)
                {
                    // Solo se actualizan los datos según tipo de trámite
                    if (CModel.TipoTramiteEnum == TipoTramite.CambioDirectiva)
                    {
                        CModel.NombreComiteSalud = comite.NombreComiteSalud;
                        CModel.Comunidad = comite.Comunidad;
                        CModel.RegionSaludId = comite.RegionSaludId ?? 0;
                        CModel.ProvinciaId = comite.ProvinciaId ?? 0;
                        CModel.DistritoId = comite.DistritoId ?? 0;
                        CModel.CorregimientoId = comite.CorregimientoId ?? 0;

                        CModel.Miembros = comite.Miembros.ToList(); // todos los miembros
                    }
                    else if (CModel.TipoTramiteEnum == TipoTramite.JuntaInterventora)
                    {
                        CModel.NombreComiteSalud = comite.NombreComiteSalud;
                        CModel.Comunidad = comite.Comunidad;

                        // Solo presidente y tesorero
                        CModel.Miembros = comite.Miembros
                            .Where(m => m.CargoId == 1 || m.CargoId == 5) // ajusta IDs según tus cargos
                            .ToList();
                    }
                }
            }
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