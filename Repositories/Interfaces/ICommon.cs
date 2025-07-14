// namespace SISTEMALEGAL.Repositories.Interfaces;
//
// public interface ICommon
// {
//     #region Datos Requisicion
//     Task<List<ListModel>> GetTipoRequisiciones();
//     Task<List<ListModel>> GetClaseProyecto(int IdRegion);
//     Task<List<ListModel>> GetDirectoresUsuario(FullUserModel UserData);
//     Task<List<ListModel>> GetUnidadUsuario(FullUserModel UserData);
//     Task<List<ListModel>> GetCategorias(string Value);
//     Task<List<ListModel>> GetLugaresEntrega(int IdRegion);
//     Task<List<ListModel>> GetEntidadesFinancieras(int IdRegion);
//     Task<List<ListModel>> GetTipoPago();
//     Task<string> GetEjemploDescripcionGrupo();
//     #endregion
//     
//     #region Utilities
//     Task<string> GetTotalTexto(decimal Total);
//     Task<string> GetFakePassword();
//     #endregion
//     
//     #region Unidades
//     Task<List<ListModel>> GetRegiones();
//     Task<List<ListModel>> GetDirecciones(int IdRegion);
//     Task<List<ListModel>> GetSubDirecciones(int IdDireccion);
//     Task<List<ListModel>> GetDepartamentos(int IdDireccion, int IdSubDireccion);
//     #endregion
// }