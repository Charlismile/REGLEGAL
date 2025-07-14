// using BlazorBootstrap;
// using Microsoft.AspNetCore.Components;
// using Microsoft.AspNetCore.Components.Authorization;
// using REGISTROLEGAL.Data;
// using REGISTROLEGAL.Models.Forms;
//
// namespace REGISTROLEGAL.Components.Pages.Public;
//
// public partial class RegComite : ComponentBase
// {
//     private Modal ModalFormulario = default!;
//     
//     [Parameter] public int? ComiteId { get; set; }
//     
//     [Inject] protected ToastService _ToastService { get; set; } = default!;
//     [Inject] protected PreloadService PreloadService { get; set; } = default!;
//     
//     #region PARAMETERS
//     
//     private RegistroComiteDTO ComiteData { get; set; } = new();
//     private string UserName { get; set; } = "";
//     private FullUserModel UserData { get; set; } = new();
//     
//     #endregion
//     
//     #region LISTAS
//     
//     private List<ListModel> Comites { get; set; } = new();
//     private List<ListModel> TipoTramite { get; set; } = new();
//     private List<ListModel> Directores { get; set; } = new();
//     private List<ListModel> Categorias { get; set; } = new();
//     
//     #endregion
//     
//     #region INITIAL EVENT
//     
//     protected override async Task OnInitializedAsync()
//     {
//         PreloadService.Show(SpinnerColor.Light, "Cargando Formulario...");
//     
//         ComiteId ??= 0;
//     
//         var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
//         UserName = authState.User.Identity?.Name ?? "";
//         UserData = await _UserDataService.GetUser(UserName);
//     
//         Comites = await _CommonServices.GetTipoRequisiciones();
//         TipoTramite = await _CommonServices.GetClaseProyecto(UserData.IdRegion);
//         Directores = await _CommonServices.GetDirectoresUsuario(UserData);
//         EntidadesFinancieras = await _CommonServices.GetEntidadesFinancieras(UserData.IdRegion);
//         LugaresEntrega = await _CommonServices.GetLugaresEntrega(UserData.IdRegion);
//         FormaPago = await _CommonServices.GetTipoPago();
//     
//         await InicializarForm();
//         PreloadService.Hide();
//     }
//     
//     private async Task InicializarForm()
//     {
//         if (ComiteId == 0)
//         {
//             DateTime actualDate = DateTime.Now;
//             ComiteData = new RegistroComiteDTO()
//             {
//                 NombreComiteSalud = UserName,
//                 RegionSaludId = UserData.IdRegion,
//                 ProvinciaId = UserData.IdProvincia,
//                 DistritoId = UserData.IdDireccion,
//                 CorregimientoId = UserData.IdSubDireccion,
//                 Comunidad = UserData.Comunidad,
//                 CargoId = UserData.Cargo,
//                 CedulaMiembro = UserData.Cedula,
//                 NombreMiembro = await _CommonServices.GetEjemploDescripcionGrupo(),
//                 EditMode = false,
//             };
//     
//             if (UserData.UserNivel == "N1")
//             {
//                 ComiteData.UnidadName = UserData.Direccion;
//             }
//             else if (UserData.UserNivel == "N2")
//             {
//                 RequisicionData.UnidadName = UserData.SubDireccion;
//             }
//             else if (UserData.UserNivel == "N3")
//             {
//                 RequisicionData.UnidadName = UserData.Departamento;
//             }
//             else if (UserData.UserNivel == "N4")
//             {
//                 RequisicionData.UnidadName = UserData.Departamento;
//             }
//         }
//         else
//         {
//             RequisicionData = await _RequisicionService.GetRequisicionById(IdRequisicion ?? 0);
//             Renglones = RequisicionData.Items;
//             await RequisicionChanged(RequisicionData.TipoRequisicionCode);
//         }
//     }
//     
//     #endregion
//     
//     #region EVENTOS DURANTE EL FORMULARIO
//     
//     private async Task RequisicionChanged(string Value)
//     {
//         RequisicionData.TipoRequisicionCode = Value ?? "";
//         Categorias = new();
//         if (!String.IsNullOrEmpty(RequisicionData.TipoRequisicionCode))
//         {
//             Categorias = await _CommonServices.GetCategorias(RequisicionData.TipoRequisicionCode);
//         }
//     
//         if (String.IsNullOrEmpty(RequisicionData.TipoProyectoCode))
//             RequisicionData.TipoProyectoCode = "";
//     }
//     
//     #endregion
//     
//     #region RENGLONES
//     
//     // VARIABLES
//     private RequisicionItemsModel RenglonForm { get; set; } = new();
//     private List<RequisicionItemsModel> Renglones { get; set; } = new();
//     
//     private ResultModel ResultRenglon { get; set; } = new ResultModel()
//     {
//         Success = true,
//     };
//     
//     private bool RenglonFormValid { get; set; } = true;
//     private bool RenglonEditing { get; set; } = false;
//     
//     // EVENTOS
//     private void InsertItemSubmit()
//     {
//         ResultRenglon.Errores.Clear();
//         ResultRenglon.Success = true;
//     
//         if (RenglonForm.Cantidad == 0)
//         {
//             ResultRenglon.Errores.Add("Debe ingresar la cantidad.");
//             ResultRenglon.Success = false;
//         }
//     
//         if (String.IsNullOrEmpty(RenglonForm.Unidad))
//         {
//             ResultRenglon.Errores.Add("Debe ingresar la unidad.");
//             ResultRenglon.Success = false;
//         }
//     
//         if (String.IsNullOrEmpty(RenglonForm.Descripcion))
//         {
//             ResultRenglon.Errores.Add("Debe ingresar la descripción.");
//             ResultRenglon.Success = false;
//         }
//     
//         if (RenglonForm.PrecioUnitario == 0)
//         {
//             ResultRenglon.Errores.Add("Debe ingresar el precio unitario.");
//             ResultRenglon.Success = false;
//         }
//     
//         if (ResultRenglon.Success)
//         {
//             if (RenglonForm.ITBMS)
//             {
//                 decimal itbms = Convert.ToDecimal(0.07);
//                 RenglonForm.ValorSubTotal = (decimal)RenglonForm.Cantidad * RenglonForm.PrecioUnitario;
//                 RenglonForm.ValorImpuesto = RenglonForm.ValorSubTotal * itbms;
//                 RenglonForm.ValorTotal = RenglonForm.ValorSubTotal + RenglonForm.ValorImpuesto;
//             }
//             else
//             {
//                 RenglonForm.ValorImpuesto = 0;
//                 RenglonForm.ValorSubTotal = (decimal)RenglonForm.Cantidad * RenglonForm.PrecioUnitario;
//                 RenglonForm.ValorTotal = RenglonForm.ValorSubTotal;
//             }
//     
//             RenglonForm.Codigo = RenglonForm.Codigo.ToUpper();
//             RenglonForm.Unidad = RenglonForm.Unidad.ToUpper();
//             RenglonForm.Descripcion = RenglonForm.Descripcion.ToUpper();
//     
//             if (RenglonEditing)
//             {
//                 int index = Renglones.FindIndex(x => x.TempId == RenglonForm.TempId);
//                 if (index != -1)
//                 {
//                     Renglones[index].Cantidad = RenglonForm.Cantidad;
//                     Renglones[index].Codigo = RenglonForm.Codigo;
//                     Renglones[index].Unidad = RenglonForm.Unidad;
//                     Renglones[index].Descripcion = RenglonForm.Descripcion;
//                     Renglones[index].PrecioUnitario = RenglonForm.PrecioUnitario;
//                     Renglones[index].ValorSubTotal = RenglonForm.ValorSubTotal;
//                     Renglones[index].ValorImpuesto = RenglonForm.ValorImpuesto;
//                     Renglones[index].ValorTotal = RenglonForm.ValorTotal;
//                     Renglones[index].ITBMS = RenglonForm.ITBMS;
//                     Renglones[index].UpdateRow = true;
//                     Renglones[index].InsertRow = false;
//                     Renglones[index].DeleteRow = false;
//                     Renglones[index].ShowRow = true;
//                 }
//     
//                 RenglonEditing = false;
//             }
//             else
//             {
//                 RenglonForm.UpdateRow = false;
//                 RenglonForm.InsertRow = true;
//                 RenglonForm.DeleteRow = false;
//                 RenglonForm.ShowRow = true;
//                 Renglones.Add(RenglonForm);
//             }
//     
//             RequisicionData.Items = Renglones;
//             UpdateTotales();
//             RenglonForm = new();
//             ResultRenglon.Success = true;
//         }
//     }
//     
//     private void EditRenglon(string Id)
//     {
//         RenglonEditing = false;
//         var data = Renglones.Where(x => x.TempId == Id).FirstOrDefault();
//         if (data != null)
//         {
//             RenglonForm = new RequisicionItemsModel()
//             {
//                 Id = data.Id,
//                 TempId = data.TempId,
//                 IdRequisicion = data.IdRequisicion,
//                 Cantidad = data.Cantidad,
//                 Codigo = data.Codigo,
//                 Unidad = data.Unidad,
//                 Descripcion = data.Descripcion,
//                 PrecioUnitario = data.PrecioUnitario,
//                 ValorImpuesto = data.ValorImpuesto,
//                 ValorSubTotal = data.ValorSubTotal,
//                 ValorTotal = data.ValorTotal,
//                 ITBMS = data.ITBMS,
//                 ShowRow = data.ShowRow,
//                 UpdateRow = data.UpdateRow,
//                 DeleteRow = data.DeleteRow,
//                 InsertRow = data.InsertRow,
//             };
//             RenglonEditing = true;
//         }
//     }
//     
//     private void DeleteRenglon(string Id)
//     {
//         var Renglon = Renglones.Where(x => x.TempId == Id).FirstOrDefault();
//         Renglon.DeleteRow = true;
//         Renglon.ShowRow = false;
//         RequisicionData.Items = Renglones;
//         UpdateTotales();
//     }
//     
//     private void CancelEditRenglon()
//     {
//         RenglonForm = new();
//         Renglones = RequisicionData.Items;
//         RenglonEditing = false;
//     }
//     
//     #endregion
//     
//     #region GUARDAR REQUISICION
//     
//     private ResultModel RequisicionResult { get; set; } = new ResultModel()
//     {
//         Success = true,
//     };
//     
//     private async Task CreateRequisicionSubmit()
//     {
//         RequisicionResult.Success = true;
//         RequisicionResult.Errores.Clear();
//         if (Renglones == null || Renglones.Count == 0)
//         {
//             RequisicionResult.Success = false;
//             RequisicionResult.Errores.Add("Debe ingresar los renglones.");
//     
//             await ModalFormulario.ShowAsync();
//             return;
//         }
//     
//         RequisicionData.Items = Renglones;
//         int IdDirector = Convert.ToInt32(RequisicionData.IdDirector);
//         RequisicionData.DirectorName = Directores
//             .Where(x => x.Id == IdDirector)
//             .Select(x => x.Name)
//             .FirstOrDefault();
//     
//         ResultModel Resultado = new ResultModel();
//     
//         if (RequisicionData.Id == 0)
//         {
//             RequisicionData.IdUnidadEjecutora = RequisicionData.IdDireccion;
//             Resultado = await _RequisicionService.SaveRequisicion(RequisicionData);
//         }
//         else
//         {
//             Resultado = await _RequisicionService.UpdateRequisicion(RequisicionData);
//         }
//     
//         _ToastService.Notify(new(Resultado.Success ? ToastType.Success : ToastType.Danger, "", $"Mensaje",
//             $"{DateTime.Now}",
//             Resultado.Message));
//     
//         if (RequisicionData.EditMode == false)
//         {
//             NavigationProvider.NavigateTo("/requisiciones/index");
//         }
//     }
//     
//     private async Task UpdateTotales()
//     {
//         decimal impuesto = Convert.ToDecimal(0.07);
//         decimal subtotal = 0;
//         decimal total = 0;
//         decimal impuestoTotal = 0;
//     
//         foreach (var item in Renglones.Where(x => x.DeleteRow == false))
//         {
//             subtotal += item.ValorSubTotal;
//             impuestoTotal += item.ValorImpuesto;
//         }
//     
//         total = subtotal + impuestoTotal;
//         RequisicionData.PrecioITBMSRequisicion = impuestoTotal;
//         RequisicionData.PrecioSubTotalRequisicion = subtotal;
//         RequisicionData.PrecioTotalRequisicion = total;
//         RequisicionData.TotalTexto = await _CommonServices.GetTotalTexto(RequisicionData.PrecioTotalRequisicion);
//     }
//     
//     #endregion
//     
//     #region LOAD FICHA
//     
//     private async Task LoadFicha()
//     {
//         string codigo = RenglonForm.Codigo;
//         FichaModel ficha = await _RequisicionService.GetFicha(codigo);
//         if (ficha != null && ficha.Resultado == true)
//         {
//             RenglonForm.Descripcion = $"Nombre Genérico: {ficha.NombreGenerico}\nDescripción: {ficha.Descripcion}";
//         }
//     }
//     
//     #endregion
// }