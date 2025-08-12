// using Microsoft.AspNetCore.Components;
// using Microsoft.AspNetCore.Components.Forms;
// using REGISTROLEGAL.DTOs;
// using REGISTROLEGAL.Models.Entities.BdSisLegal;
//
// namespace REGISTROLEGAL.Components.Componente;
//
// public partial class RegistroComite : ComponentBase
// {
//     private RegistroDto Model = new();
//     private List<TbRegionSalud> Regiones = new();
//     private List<TbProvincia> Provincias = new();
//     private List<TbDistrito> Distritos = new();
//     private List<TbCorregimiento> Corregimientos = new();
//     private List<TbTipoTramite> TipoTramites = new();
//     private List<TbCargosMiembrosComite> Cargos = new();
//     private bool IsSubmitting = false;
//     private string MensajeExito = "";
//     private string MensajeError = "";
//
//     // Cargos disponibles según el tipo de trámite
//     private List<TbCargosMiembrosComite> CargosDisponibles => 
//         Model.TipoTramiteId == 3 ? 
//             Cargos.Where(c => c.NombreCargo == "Presidente" || c.NombreCargo == "Tesorero(a)").ToList() : 
//             Cargos;
//
//     protected override async Task OnInitializedAsync()
//     {
//         // Cargar datos iniciales
//         Regiones = await RegistroService.ObtenerRegionesAsync();
//         Provincias = await RegistroService.ObtenerProvinciasAsync();
//         TipoTramites = await RegistroService.ObtenerTipoTramitesAsync();
//         Cargos = await RegistroService.ObtenerCargosMiembrosAsync();
//     }
//     
//     // private async Task CargarProvincias(ChangeEventArgs e)
//     // {
//     //     if (int.TryParse(e.Value?.ToString(), out int regionId) && regionId > 0)
//     //     {
//     //         // ✅ CORRECTO: Usar await con métodos asincrónicos
//     //         Provincias = await RegistroService.ObtenerProvinciasPorRegionAsync(regionId);
//     //         Distritos.Clear(); 
//     //         Corregimientos.Clear();
//     //         Model.ProvinciaId = 0; 
//     //         Model.DistritoId = 0; 
//     //         Model.CorregimientoId = 0;
//     //     }
//     //     else
//     //     {
//     //         Provincias.Clear(); 
//     //         Distritos.Clear(); 
//     //         Corregimientos.Clear();
//     //     }
//     // }
//
//     private async Task CargarDistritos(ChangeEventArgs e)
//     {
//         if (int.TryParse(e.Value?.ToString(), out int provinciaId) && provinciaId > 0)
//         {
//             // ✅ CORRECTO: Usar await con métodos asincrónicos
//             Distritos = await RegistroService.ObtenerDistritosPorProvinciaAsync(provinciaId);
//             Corregimientos = new List<TbCorregimiento>();
//             // Corregimientos.Clear();
//             Model.DId = 0; 
//             Model.CId = 0;
//             StateHasChanged();
//         }
//         else
//         {
//             
//             Distritos.Clear(); 
//             Corregimientos.Clear();
//         }
//     }
//
//     private async Task CargarCorregimientos(ChangeEventArgs e)
//     {
//         if (int.TryParse(e.Value?.ToString(), out int distritoId) && distritoId > 0)
//         {
//             // ✅ CORRECTO: Usar await con métodos asincrónicos
//             Corregimientos = await RegistroService.ObtenerCorregimientosPorDistritoAsync(distritoId);
//             Model.CId = 0;
//             StateHasChanged();
//         }
//         else
//         {
//             Corregimientos.Clear();
//         }
//     }
//
//     private async Task OnTramiteChanged(ChangeEventArgs e)
//     {
//         if (int.TryParse(e.Value?.ToString(), out int tramiteId))
//         {
//             Model.TipoTramiteId = tramiteId;
//             Model.Miembros.Clear();
//             
//             // Configurar miembros según el tipo de trámite
//             if (tramiteId == 1) // Personería
//             {
//                 for (int i = 0; i < 7; i++)
//                 {
//                     Model.Miembros.Add(new RegistroDto());
//                 }
//             }
//             else if (tramiteId == 3) // Junta Interventora
//             {
//                 for (int i = 0; i < 2; i++)
//                 {
//                     Model.Miembros.Add(new RegistroDto());
//                 }
//             }
//         }
//     }
//
//     private void AddMiembro()
//     {
//         if ((Model.TipoTramiteId == 1 && Model.Miembros.Count < 7) || 
//             (Model.TipoTramiteId == 3 && Model.Miembros.Count < 2))
//         {
//             Model.Miembros.Add(new RegistroDto());
//         }
//     }
//
//     private async Task CargarDocumentos(InputFileChangeEventArgs e)
//     {
//         foreach (var file in e.GetMultipleFiles())
//         {
//             // Validar tamaño (máximo 10 MB)
//             if (file.Size > 10 * 1024 * 1024)
//             {
//                 MensajeError = $"El archivo {file.Name} excede el tamaño máximo de 10 MB.";
//                 continue;
//             }
//
//             // Validar tipo
//             var extension = Path.GetExtension(file.Name).ToLower();
//             var permitidas = new[] { ".pdf", ".docx", ".jpg", ".png", ".jpeg" };
//             if (!permitidas.Contains(extension))
//             {
//                 MensajeError = $"El archivo {file.Name} no tiene un formato permitido.";
//                 continue;
//             }
//
//             Model.DocumentosSubir.Add(file);
//         }
//     }
//
//     private void RemoverDocumento(int index)
//     {
//         if (index >= 0 && index < Model.DocumentosSubir.Count)
//         {
//             Model.DocumentosSubir.RemoveAt(index);
//         }
//     }
//
//     private async Task HandleValidSubmit()
//     {
//         IsSubmitting = true;
//         MensajeError = "";
//         MensajeExito = "";
//
//         try
//         {
//             // Validaciones adicionales
//             if (Model.Miembros.Any(m => string.IsNullOrWhiteSpace(m.Nombre)))
//             {
//                 MensajeError = "Todos los miembros deben tener un nombre.";
//                 return;
//             }
//
//             if (Model.Miembros.Any(m => string.IsNullOrWhiteSpace(m.Cedula)))
//             {
//                 MensajeError = "Todos los miembros deben tener una cédula.";
//                 return;
//             }
//
//             if (Model.Miembros.Any(m => m.CargoId == 0))
//             {
//                 MensajeError = "Todos los miembros deben tener un cargo asignado.";
//                 return;
//             }
//
//             // Validar cargos únicos para los tipos de trámite que lo requieren
//             if (Model.TipoTramiteId == 1 || Model.TipoTramiteId == 3)
//             {
//                 var cargosRepetidos = Model.Miembros
//                     .GroupBy(m => m.CargoId)
//                     .Where(g => g.Count() > 1)
//                     .Select(g => g.First().CargoId)
//                     .ToList();
//
//                 if (cargosRepetidos.Any())
//                 {
//                     var nombresCargos = Cargos
//                         .Where(c => cargosRepetidos.Contains(c.CargoId))
//                         .Select(c => c.NombreCargo);
//                     
//                     MensajeError = $"Los siguientes cargos no pueden repetirse: {string.Join(", ", nombresCargos)}";
//                     return;
//                 }
//             }
//
//             // Registrar el comité
//             var resultado = await RegistroService.CrearComiteAsync(Model);
//             
//             if (resultado)
//             {
//                 MensajeExito = "Comité registrado con éxito.";
//                 Navigation.NavigateTo("/comites");
//             }
//             else
//             {
//                 MensajeError = "Error al registrar el comité. Por favor, intente nuevamente.";
//             }
//         }
//         catch (Exception ex)
//         {
//             MensajeError = $"Error inesperado: {ex.Message}";
//         }
//         finally
//         {
//             IsSubmitting = false;
//         }
//     }
//
//     private void Cancelar() => Navigation.NavigateTo("/comites");
// }