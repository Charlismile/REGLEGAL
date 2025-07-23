// using Microsoft.AspNetCore.Components;
//
// namespace REGISTROLEGAL.Components.Componente;
//
// public partial class EditarRegistroComite : ComponentBase
// {
//     [Inject] private IRegistroComiteService RegistroService { get; set; }
//     [Inject] private NavigationManager Navigation { get; set; }
//     [Inject] private IJSRuntime JSRuntime { get; set; }
//     [Parameter] public int Id { get; set; }
//
//     private RegistroComiteDTO? registroComite;
//     private List<TbTipoTramite> tramites = new();
//     private List<TbRegionSalud> regiones = new();
//     private List<TbProvincia> provincias = new();
//     private List<TbDistrito> distritos = new();
//     private List<TbCorregimiento> corregimientos = new();
//     private List<TbCargosMiembrosComite> cargos = new();
//
//     private bool cargando = true;
//     private bool guardando = false;
//     private bool eliminando = false;
//     private bool MostrarModalEliminar = false;
//     private string mensajeError = string.Empty;
//     private string mensajeExito = string.Empty;
//
//     protected override async Task OnInitializedAsync()
//     {
//         try
//         {
//             cargando = true;
//             
//             // Cargar datos básicos
//             var tareasParalelas = new List<Task>
//             {
//                 CargarTramites(),
//                 CargarRegiones(),
//                 CargarCargos(),
//                 CargarRegistroComite()
//             };
//
//             await Task.WhenAll(tareasParalelas);
//         }
//         catch (Exception ex)
//         {
//             mensajeError = $"Error al cargar los datos: {ex.Message}";
//         }
//         finally
//         {
//             cargando = false;
//             StateHasChanged();
//         }
//     }
//
//     private async Task CargarTramites()
//     {
//         try
//         {
//             tramites = await RegistroService.GetTramitesAsync();
//         }
//         catch (Exception ex)
//         {
//             mensajeError += $"Error al cargar trámites: {ex.Message}. ";
//         }
//     }
//
//     private async Task CargarRegiones()
//     {
//         try
//         {
//             regiones = await RegistroService.GetRegionesAsync();
//         }
//         catch (Exception ex)
//         {
//             mensajeError += $"Error al cargar regiones: {ex.Message}. ";
//         }
//     }
//
//     private async Task CargarCargos()
//     {
//         try
//         {
//             cargos = await RegistroService.GetCargosAsync();
//         }
//         catch (Exception ex)
//         {
//             mensajeError += $"Error al cargar cargos: {ex.Message}. ";
//         }
//     }
//
//     private async Task CargarRegistroComite()
//     {
//         try
//         {
//             registroComite = await RegistroService.GetComitePorIdAsync(Id);
//             
//             if (registroComite == null)
//             {
//                 mensajeError = "No se encontró el registro del comité especificado.";
//                 return;
//             }
//
//             // Cargar datos en cascada si existen
//             await CargarDatosEnCascada();
//         }
//         catch (Exception ex)
//         {
//             mensajeError = $"Error al cargar el registro del comité: {ex.Message}";
//         }
//     }
//
//     private async Task CargarDatosEnCascada()
//     {
//         if (registroComite == null) return;
//
//         try
//         {
//             // Cargar provincias si hay región seleccionada
//             if (registroComite.RegionSaludId > 0)
//             {
//                 provincias = await RegistroService.GetProvinciasAsync(registroComite.RegionSaludId);
//             }
//
//             // Cargar distritos si hay provincia seleccionada
//             if (registroComite.ProvinciaId > 0)
//             {
//                 distritos = await RegistroService.GetDistritosAsync(registroComite.ProvinciaId);
//             }
//
//             // Cargar corregimientos si hay distrito seleccionado
//             if (registroComite.DistritoId > 0)
//             {
//                 corregimientos = await RegistroService.GetCorregimientosAsync(registroComite.DistritoId);
//             }
//         }
//         catch (Exception ex)
//         {
//             mensajeError += $"Error al cargar datos de ubicación: {ex.Message}. ";
//         }
//     }
//
//     private async Task CargarProvincias(ChangeEventArgs e)
//     {
//         if (registroComite == null) return;
//
//         try
//         {
//             var regionId = int.Parse(e.Value?.ToString() ?? "0");
//             
//             if (regionId == 0)
//             {
//                 provincias.Clear();
//                 distritos.Clear();
//                 corregimientos.Clear();
//             }
//             else
//             {
//                 provincias = await RegistroService.GetProvinciasAsync(regionId);
//             }
//
//             // Limpiar selecciones dependientes
//             registroComite.ProvinciaId = 0;
//             registroComite.DistritoId = 0;
//             registroComite.CorregimientoId = 0;
//             distritos.Clear();
//             corregimientos.Clear();
//         }
//         catch (Exception ex)
//         {
//             mensajeError = $"Error al cargar provincias: {ex.Message}";
//         }
//     }
//
//     private async Task CargarDistritos(ChangeEventArgs e)
//     {
//         if (registroComite == null) return;
//
//         try
//         {
//             var provinciaId = int.Parse(e.Value?.ToString() ?? "0");
//             
//             if (provinciaId == 0)
//             {
//                 distritos.Clear();
//                 corregimientos.Clear();
//             }
//             else
//             {
//                 distritos = await RegistroService.GetDistritosAsync(provinciaId);
//             }
//
//             // Limpiar selecciones dependientes
//             registroComite.DistritoId = 0;
//             registroComite.CorregimientoId = 0;
//             corregimientos.Clear();
//         }
//         catch (Exception ex)
//         {
//             mensajeError = $"Error al cargar distritos: {ex.Message}";
//         }
//     }
//
//     private async Task CargarCorregimientos(ChangeEventArgs e)
//     {
//         if (registroComite == null) return;
//
//         try
//         {
//             var distritoId = int.Parse(e.Value?.ToString() ?? "0");
//             
//             if (distritoId == 0)
//             {
//                 corregimientos.Clear();
//             }
//             else
//             {
//                 corregimientos = await RegistroService.GetCorregimientosAsync(distritoId);
//             }
//
//             // Limpiar selección dependiente
//             registroComite.CorregimientoId = 0;
//         }
//         catch (Exception ex)
//         {
//             mensajeError = $"Error al cargar corregimientos: {ex.Message}";
//         }
//     }
//
//     private void OnTramiteChanged(ChangeEventArgs e)
//     {
//         if (registroComite == null) return;
//
//         try
//         {
//             // Confirmar si el usuario quiere cambiar el trámite y perder los miembros actuales
//             if (registroComite.Miembros.Any())
//             {
//                 // En una implementación real, podrías usar un modal de confirmación
//                 // Por ahora, simplemente limpiamos y añadimos un miembro por defecto
//             }
//
//             // Limpiar miembros al cambiar trámite
//             registroComite.Miembros.Clear();
//             
//             // Añadir un miembro por defecto si el trámite requiere miembros
//             var tramiteId = int.Parse(e.Value?.ToString() ?? "0");
//             if (tramiteId == 1 || tramiteId == 2 || tramiteId == 3)
//             {
//                 AddMiembro();
//             }
//         }
//         catch (Exception ex)
//         {
//             mensajeError = $"Error al cambiar trámite: {ex.Message}";
//         }
//     }
//
//     private void AddMiembro()
//     {
//         if (registroComite == null) return;
//
//         if (registroComite.Miembros.Count < GetMaxMiembros())
//         {
//             registroComite.Miembros.Add(new MiembroComiteDTO());
//         }
//     }
//
//     private void EliminarMiembro(int index)
//     {
//         if (registroComite == null || index < 0 || index >= registroComite.Miembros.Count) return;
//
//         registroComite.Miembros.RemoveAt(index);
//     }
//
//     private int GetMaxMiembros()
//     {
//         if (registroComite == null) return 0;
//         return registroComite.TramiteId == 3 ? 2 : 7;
//     }
//
//     private bool IsMaxMiembrosAlcanzado()
//     {
//         if (registroComite == null) return true;
//         return registroComite.Miembros.Count >= GetMaxMiembros();
//     }
//
//     private List<TbCargosMiembrosComite> GetCargosPorTramite()
//     {
//         if (registroComite?.TramiteId == 3)
//         {
//             // Para trámite 3: Solo Presidente (1) y Tesorero (4)
//             return cargos.Where(c => c.CargoId == 1 || c.CargoId == 4).ToList();
//         }
//         return cargos;
//     }
//
//     private async Task CargarArchivos(InputFileChangeEventArgs e)
//     {
//         if (registroComite == null) return;
//
//         try
//         {
//             var archivosPermitidos = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
//             const long tamanoMaximo = 10 * 1024 * 1024; // 10 MB
//
//             foreach (var archivo in e.GetMultipleFiles(10)) // Máximo 10 archivos
//             {
//                 // Validar tamaño
//                 if (archivo.Size > tamanoMaximo)
//                 {
//                     mensajeError = $"El archivo '{archivo.Name}' es demasiado grande. Tamaño máximo: 10MB.";
//                     continue;
//                 }
//
//                 // Validar extensión
//                 var extension = Path.GetExtension(archivo.Name).ToLower();
//                 if (!archivosPermitidos.Contains(extension))
//                 {
//                     mensajeError = $"El archivo '{archivo.Name}' tiene un formato no permitido. Use: PDF, JPG, JPEG, PNG.";
//                     continue;
//                 }
//
//                 try
//                 {
//                     // Crear directorio si no existe
//                     var directorioSubida = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "comites");
//                     if (!Directory.Exists(directorioSubida))
//                     {
//                         Directory.CreateDirectory(directorioSubida);
//                     }
//
//                     // Generar nombre único
//                     var nombreGuardado = $"{Guid.NewGuid()}{extension}";
//                     var rutaCompleta = Path.Combine(directorioSubida, nombreGuardado);
//                     var rutaRelativa = $"/uploads/comites/{nombreGuardado}";
//
//                     // Guardar archivo
//                     await using var stream = new FileStream(rutaCompleta, FileMode.Create);
//                     await archivo.OpenReadStream(tamanoMaximo).CopyToAsync(stream);
//
//                     // Añadir a la lista de archivos
//                     registroComite.Archivos.Add(new ArchivoDTO
//                     {
//                         NombreOriginal = archivo.Name,
//                         Ruta = rutaRelativa,
//                         Categoria = "Documento General"
//                     });
//
//                     mensajeExito = $"Archivo '{archivo.Name}' cargado exitosamente.";
//                 }
//                 catch (Exception ex)
//                 {
//                     mensajeError = $"Error al guardar el archivo '{archivo.Name}': {ex.Message}";
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             mensajeError = $"Error al procesar archivos: {ex.Message}";
//         }
//         
//         StateHasChanged();
//     }
//
//     private void EliminarArchivo(ArchivoDTO archivo)
//     {
//         if (registroComite == null) return;
//
//         try
//         {
//             // Eliminar de la lista
//             registroComite.Archivos.Remove(archivo);
//
//             // Intentar eliminar el archivo físico
//             try
//             {
//                 var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", archivo.Ruta.TrimStart('/'));
//                 if (File.Exists(rutaCompleta))
//                 {
//                     File.Delete(rutaCompleta);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 // Log del error pero no interrumpir el proceso
//                 Console.WriteLine($"No se pudo eliminar el archivo físico: {ex.Message}");
//             }
//
//             mensajeExito = "Archivo eliminado exitosamente.";
//         }
//         catch (Exception ex)
//         {
//             mensajeError = $"Error al eliminar archivo: {ex.Message}";
//         }
//     }
//
//     private async Task HandleValidSubmit()
//     {
//         if (registroComite == null) return;
//
//         try
//         {
//             guardando = true;
//             mensajeError = string.Empty;
//             mensajeExito = string.Empty;
//
//             // Validaciones adicionales
//             if (!ValidarDatos())
//             {
//                 return;
//             }
//
//             var resultado = await RegistroService.ActualizarComiteAsync(registroComite);
//             
//             if (resultado)
//             {
//                 mensajeExito = "Registro actualizado exitosamente.";
//                 
//                 // Opcional: Redirigir después de un breve delay
//                 await Task.Delay(1500);
//                 Navigation.NavigateTo("/comites");
//             }
//             else
//             {
//                 mensajeError = "No se pudo actualizar el registro. Intente nuevamente.";
//             }
//         }
//         catch (Exception ex)
//         {
//             mensajeError = $"Error al actualizar el registro: {ex.Message}";
//         }
//         finally
//         {
//             guardando = false;
//             StateHasChanged();
//         }
//     }
//
//     private bool ValidarDatos()
//     {
//         var errores = new List<string>();
//
//         // Validar campos requeridos
//         if (string.IsNullOrWhiteSpace(registroComite?.NombreComiteSalud))
//             errores.Add("El nombre del comité es requerido.");
//
//         if (registroComite?.RegionSaludId <= 0)
//             errores.Add("Debe seleccionar una región.");
//
//         if (registroComite?.ProvinciaId <= 0)
//             errores.Add("Debe seleccionar una provincia.");
//
//         if (registroComite?.DistritoId <= 0)
//             errores.Add("Debe seleccionar un distrito.");
//
//         if (registroComite?.CorregimientoId <= 0)
//             errores.Add("Debe seleccionar un corregimiento.");
//
//         if (registroComite?.TramiteId <= 0)
//             errores.Add("Debe seleccionar un tipo de trámite.");
//
//         // Validar miembros si el trámite los requiere
//         if (registroComite?.TramiteId == 1 || registroComite?.TramiteId == 2 || registroComite?.TramiteId == 3)
//         {
//             if (!registroComite.Miembros.Any())
//             {
//                 errores.Add("Debe agregar al menos un miembro al comité.");
//             }
//             else
//             {
//                 for (int i = 0; i < registroComite.Miembros.Count; i++)
//                 {
//                     var miembro = registroComite.Miembros[i];
//                     
//                     if (string.IsNullOrWhiteSpace(miembro.Nombre))
//                         errores.Add($"El nombre del miembro {i + 1} es requerido.");
//                     
//                     if (string.IsNullOrWhiteSpace(miembro.Cedula))
//                         errores.Add($"La cédula del miembro {i + 1} es requerida.");
//                     
//                     if (miembro.CargoId <= 0)
//                         errores.Add($"Debe seleccionar un cargo para el miembro {i + 1}.");
//                 }
//
//                 // Validar cargos únicos para trámite tipo 3
//                 if (registroComite.TramiteId == 3)
//                 {
//                     var cargosSeleccionados = registroComite.Miembros.Where(m => m.CargoId > 0).Select(m => m.CargoId).ToList();
//                     if (cargosSeleccionados.Count != cargosSeleccionados.Distinct().Count())
//                     {
//                         errores.Add("No puede haber miembros con el mismo cargo.");
//                     }
//                 }
//             }
//         }
//
//         if (errores.Any())
//         {
//             mensajeError = string.Join(" ", errores);
//             return false;
//         }
//
//         return true;
//     }
//
//     private async Task ConfirmarEliminacion()
//     {
//         if (registroComite == null) return;
//
//         try
//         {
//             eliminando = true;
//             
//             var resultado = await RegistroService.EliminarComiteAsync(Id);
//             
//             if (resultado)
//             {
//                 // Mostrar mensaje de éxito con JavaScript
//                 await JSRuntime.InvokeVoidAsync("alert", "Registro eliminado exitosamente.");
//                 Navigation.NavigateTo("/comites");
//             }
//             else
//             {
//                 mensajeError = "No se pudo eliminar el registro. Intente nuevamente.";
//                 MostrarModalEliminar = false;
//             }
//         }
//         catch (Exception ex)
//         {
//             mensajeError = $"Error al eliminar el registro: {ex.Message}";
//             MostrarModalEliminar = false;
//         }
//         finally
//         {
//             eliminando = false;
//             StateHasChanged();
//         }
//     }
//
//     private void Cancelar()
//     {
//         Navigation.NavigateTo("/comites");
//     }
//
//     // Método para confirmar cambios antes de salir (opcional)
//     private async Task<bool> ConfirmarSalida()
//     {
//         // Implementar lógica para detectar cambios no guardados
//         // y mostrar confirmación si es necesario
//         return await JSRuntime.InvokeAsync<bool>("confirm", 
//             "¿Está seguro que desea salir? Los cambios no guardados se perderán.");
//     }
// }