// using Microsoft.AspNetCore.Components;
//
// namespace REGISTROLEGAL.Components.Pages.Admin;
//
// public partial class ListadoComite : ComponentBase
// {
//     private List<ComiteSaludDto> comites = new();
//     private List<ComiteSaludDto> comitesPaginados => comites.Skip((paginaActual - 1) * elementosPorPagina).Take(elementosPorPagina).ToList();
//     private int paginaActual = 1;
//     private int elementosPorPagina = 10;
//     private int totalPaginas => (int)Math.Ceiling((double)comites.Count / elementosPorPagina);
//     private string terminoBusqueda = string.Empty;
//     private bool cargando = false;
//     private string mensajeError = string.Empty;
//     private ComiteSaludDto? comiteSeleccionado;
//     private bool mostrarModalDetalles = false;
//
//     [Inject] private IComiteRepository ComiteRepo { get; set; } = default!;
//
//     protected override async Task OnInitializedAsync()
//     {
//         await CargarComites();
//     }
//
//     private async Task CargarComites()
//     {
//         try
//         {
//             cargando = true;
//             mensajeError = string.Empty;
//             comites = await ComiteRepo.ObtenerTodosAsync();
//             paginaActual = 1;
//         }
//         catch (Exception ex)
//         {
//             mensajeError = "Error al cargar comités: " + ex.Message;
//         }
//         finally
//         {
//             cargando = false;
//         }
//     }
//
//     private void CambiarPagina(int nuevaPagina)
//     {
//         if (nuevaPagina >= 1 && nuevaPagina <= totalPaginas)
//         {
//             paginaActual = nuevaPagina;
//         }
//     }
//
//     private async Task BuscarComites()
//     {
//         try
//         {
//             cargando = true;
//             mensajeError = string.Empty;
//             var todos = await ComiteRepo.ObtenerTodosAsync();
//             comites = todos.Where(c =>
//                 (!string.IsNullOrEmpty(c.NombreComiteSalud) && c.NombreComiteSalud.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
//                 (!string.IsNullOrEmpty(c.Comunidad) && c.Comunidad.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
//                 (c.Miembros != null && c.Miembros.Any(m => m.Nombre.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase)))
//             ).ToList();
//             paginaActual = 1;
//         }
//         catch (Exception ex)
//         {
//             mensajeError = "Error al buscar comités: " + ex.Message;
//         }
//         finally
//         {
//             cargando = false;
//         }
//     }
//
//     private void OnBusquedaKeyPress(KeyboardEventArgs e)
//     {
//         if (e.Key == "Enter")
//         {
//             _ = BuscarComites();
//         }
//     }
//
//     private void VerDetalles(int id)
//     {
//         comiteSeleccionado = comites.FirstOrDefault(c => c.Id == id);
//         if (comiteSeleccionado != null)
//         {
//             mostrarModalDetalles = true;
//         }
//     }
//
//     private async Task ConfirmarEliminacion(ComiteSaludDto comite)
//     {
//         var confirmado = await JS.InvokeAsync<bool>("confirm", $"¿Está seguro que desea eliminar el comité \"{comite.NombreComiteSalud}\"?");
//         if (confirmado)
//         {
//             try
//             {
//                 await ComiteRepo.EliminarAsync(comite.Id);
//                 await CargarComites();
//             }
//             catch (Exception ex)
//             {
//                 mensajeError = "Error al eliminar comité: " + ex.Message;
//             }
//         }
//     }
//
//     [Inject] private IJSRuntime JS { get; set; } = default!;
// }