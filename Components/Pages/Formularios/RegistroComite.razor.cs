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
        private string searchText;

        private bool IsSubmitting = false;
        private string MensajeExito = "";
        private string MensajeError = "";
        private string comiteSeleccionadoIdString;
        private string mensajeBusqueda = "";

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
            await CargarComitesRegistrados();
            await ObtenerUsuarioActual();
        }

        private async Task ObtenerUsuarioActual()
        {
            try
            {
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                UserName = user.Identity?.Name ?? "Sistema";
                CModel.CreadaPor = UserName;
                Console.WriteLine($"🔐 Usuario autenticado: {UserName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo usuario: {ex.Message}");
                UserName = "Sistema";
                CModel.CreadaPor = UserName;
            }
        }
        private async Task HandleValidSubmit()
        {
            IsSubmitting = true;
            MensajeError = "";
            MensajeExito = "";

            try
            {
                // Validar que hay exactamente 7 miembros
                if (CModel.Miembros.Count != NUMERO_MIEMBROS_FIJO)
                {
                    MensajeError = $"Debe haber exactamente {NUMERO_MIEMBROS_FIJO} miembros del comité.";
                    IsSubmitting = false;
                    StateHasChanged();
                    return;
                }

                // Validar cada miembro individualmente
                var miembrosInvalidos = new List<string>();
                for (int i = 0; i < CModel.Miembros.Count; i++)
                {
                    var miembro = CModel.Miembros[i];
                    if (string.IsNullOrWhiteSpace(miembro.NombreMiembro) ||
                        string.IsNullOrWhiteSpace(miembro.ApellidoMiembro) ||
                        string.IsNullOrWhiteSpace(miembro.CedulaMiembro) ||
                        miembro.CargoId == 0)
                    {
                        miembrosInvalidos.Add($"Miembro {i + 1}");
                    }
                }

                if (miembrosInvalidos.Any())
                {
                    MensajeError =
                        $"Los siguientes miembros tienen campos incompletos: {string.Join(", ", miembrosInvalidos)}";
                    IsSubmitting = false;
                    StateHasChanged();
                    return;
                }

                if (!ArchivosSeleccionados.Any())
                {
                    MensajeError = "Debe seleccionar al menos un archivo de resolución.";
                    IsSubmitting = false;
                    StateHasChanged();
                    return;
                }

                // Asegurar que el usuario está asignado
                if (string.IsNullOrEmpty(CModel.CreadaPor))
                {
                    await ObtenerUsuarioActual();
                }

                Console.WriteLine($"Intentando crear comité: {CModel.NombreComiteSalud}");
                Console.WriteLine($"Usuario: {CModel.CreadaPor}");
                Console.WriteLine($"Miembros: {CModel.Miembros.Count}");

                // 1. Crear comité
                var result = await RegistroComiteService.CrearComite(CModel);

                if (!result.Success)
                {
                    Console.WriteLine($"❌ Error creando comité: {result.Message}");
                    MensajeError = $"❌ Error al crear comité: {result.Message}";
                    IsSubmitting = false;
                    StateHasChanged();
                    return;
                }

                Console.WriteLine($"✅ Comité creado con ID: {result.Id}");

                // 2. Guardar archivos después de crear el comité
                if (ArchivosSeleccionados.Any())
                {
                    Console.WriteLine($"Guardando {ArchivosSeleccionados.Count} archivos...");
                    await GuardarArchivos(result.Id);
                }

                // 3. Redirigir después de éxito
                MensajeExito = "✅ Registro completado exitosamente!";
                StateHasChanged();

                // Esperar un momento para mostrar el mensaje y luego redirigir
                await Task.Delay(2000);
                Navigation.NavigateTo("/admin/listado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Error crítico en el proceso: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                MensajeError = $"💥 Error: {ex.Message}";
                IsSubmitting = false;
                StateHasChanged();
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
            ArchivosSeleccionados.Clear();

            foreach (var archivo in e.GetMultipleFiles())
            {
                // Validar extensión
                var extension = Path.GetExtension(archivo.Name).ToLower();
                if (extension != ".pdf")
                {
                    messageStore.Add(FieldIdentifier.Create(() => _archivoResolucion),
                        new[] { $"El archivo {archivo.Name} debe ser un PDF." });
                    continue;
                }

                // Validar tipo MIME
                if (archivo.ContentType != "application/pdf")
                {
                    messageStore.Add(FieldIdentifier.Create(() => _archivoResolucion),
                        new[] { $"Archivo {archivo.Name} no es un PDF válido." });
                    continue;
                }

                // Validar tamaño (10 MB máximo)
                if (archivo.Size > 50 * 1024 * 1024)
                {
                    messageStore.Add(FieldIdentifier.Create(() => _archivoResolucion),
                        new[] { $"Archivo {archivo.Name} excede 10 MB." });
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

        private async Task<IFormFile> ConvertToIFormFileAsync(IBrowserFile browserFile)
        {
            var memoryStream = new MemoryStream();
            await browserFile.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024).CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return new FormFile(memoryStream, 0, memoryStream.Length, browserFile.Name, browserFile.Name)
            {
                Headers = new HeaderDictionary(),
                ContentType = browserFile.ContentType
            };
        }

        private async Task GuardarArchivos(int comiteId)
        {
            if (!ArchivosSeleccionados.Any()) return;

            try
            {
                foreach (var archivo in ArchivosSeleccionados)
                {
                    // Pasar el IBrowserFile directamente
                    var dto = await ArchivoLegalService.GuardarArchivoComiteAsync(
                        comiteId,
                        archivo,
                        "RESOLUCION COMITE"
                    );

                    if (dto != null)
                    {
                        _archivoResolucionUrl = dto.RutaArchivo;
                        Console.WriteLine($"Archivo guardado: {dto.NombreArchivo}, Ruta: {dto.RutaArchivo}");
                    }
                    else
                    {
                        Console.WriteLine($"Error: El servicio devolvió null para {archivo.Name}");
                    }
                }

                ArchivosSeleccionados.Clear();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando archivos: {ex.Message}");
            }
        }

        private async Task OnComiteSeleccionado(string comiteId)
        {
            mensajeBusqueda = "Buscando comité...";

            if (string.IsNullOrEmpty(comiteId))
            {
                mensajeBusqueda = "";
                return;
            }

            try
            {
                if (int.TryParse(comiteId, out int comiteIdInt))
                {
                    var comite = await RegistroComiteService.ObtenerComiteCompletoAsync(comiteIdInt);

                    if (comite != null)
                    {
                        await CargarDatosComite(comite);
                        DesplegarMiembros();
                        mensajeBusqueda = $"Comité '{comite.NombreComiteSalud}' cargado correctamente";
                    }
                    else
                    {
                        mensajeBusqueda = "Comité no encontrado";
                        LimpiarDatosComite();
                    }
                }
                else
                {
                    mensajeBusqueda = "ID de comité inválido";
                }
            }
            catch (Exception ex)
            {
                mensajeBusqueda = $"Error al cargar el comité: {ex.Message}";
                Console.WriteLine($"Error: {ex.Message}");
            }

            StateHasChanged();
        }

        private void LimpiarDatosComite()
        {
            CModel.NombreComiteSalud = "";
            CModel.Comunidad = "";
            CModel.RegionSaludId = null;
            CModel.ProvinciaId = null;
            CModel.DistritoId = null;
            CModel.CorregimientoId = null;
            CModel.Miembros = new List<MiembroComiteModel>();
            CModel.Archivos = new List<CArchivoModel>();
        }

        private async Task CargarDatosComite(ComiteModel comite)
        {
            CModel.NombreComiteSalud = comite.NombreComiteSalud;
            CModel.Comunidad = comite.Comunidad;
            CModel.RegionSaludId = comite.RegionSaludId;
            CModel.ProvinciaId = comite.ProvinciaId;
            CModel.DistritoId = comite.DistritoId;
            CModel.CorregimientoId = comite.CorregimientoId;
            CModel.FechaEleccion = comite.FechaEleccion;
            CModel.NumeroResolucion = comite.NumeroResolucion;
            CModel.FechaResolucion = comite.FechaResolucion;

            // Asegurar que siempre hay 7 miembros
            if (comite.Miembros != null && comite.Miembros.Any())
            {
                CModel.Miembros = comite.Miembros.Take(7).ToList();

                // Completar hasta 7 miembros si es necesario
                while (CModel.Miembros.Count < NUMERO_MIEMBROS_FIJO)
                {
                    CModel.Miembros.Add(new MiembroComiteModel());
                }
            }
            else
            {
                // Si no hay miembros, crear 7 vacíos
                CModel.Miembros = new List<MiembroComiteModel>();
                for (int i = 0; i < NUMERO_MIEMBROS_FIJO; i++)
                {
                    CModel.Miembros.Add(new MiembroComiteModel());
                }
            }

            if (comite.Archivos != null)
            {
                CModel.Archivos = comite.Archivos;
            }

            await RegionChanged(CModel.RegionSaludId ?? 0);
            await ProvinciaChanged(CModel.ProvinciaId ?? 0);
            await DistritoChanged(CModel.DistritoId ?? 0);

            miembrosDesplegados = true;
            StateHasChanged();
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

        private async Task CargarComitesRegistrados()
        {
            try
            {
                comitesRegistrados = await RegistroComiteService.ObtenerComites();
            }
            catch (Exception ex)
            {
                // Manejar error
                Console.WriteLine($"Error cargando comités: {ex.Message}");
            }
        }
        private void DebugInfo()
        {
            Console.WriteLine("=== DEBUG INFO ===");
            Console.WriteLine($"Miembros: {CModel.Miembros.Count}");
            Console.WriteLine($"Usuario: {CModel.CreadaPor}");
            Console.WriteLine($"Archivos: {ArchivosSeleccionados.Count}");
            Console.WriteLine($"Nombre Comité: {CModel.NombreComiteSalud}");
            Console.WriteLine($"Región: {CModel.RegionSaludId}");
            Console.WriteLine($"Provincia: {CModel.ProvinciaId}");
            Console.WriteLine($"Distrito: {CModel.DistritoId}");
            Console.WriteLine($"Corregimiento: {CModel.CorregimientoId}");
    
            for (int i = 0; i < CModel.Miembros.Count; i++)
            {
                var m = CModel.Miembros[i];
                Console.WriteLine($"Miembro {i+1}: {m.NombreMiembro} {m.ApellidoMiembro}, Cédula: {m.CedulaMiembro}, Cargo: {m.CargoId}");
            }
        }

        private void Cancelar() => Navigation.NavigateTo("/admin/listado");
    }
}