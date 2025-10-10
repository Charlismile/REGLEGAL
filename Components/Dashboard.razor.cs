using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using REGISTROLEGAL.Data;

namespace REGISTROLEGAL.Components;

public partial class Dashboard : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    private ApplicationUser LoggedUser = new();
        
    
    private List<CardsEstadoDeSolicitudesModel> SolicitudesPersonas = new();
    private List<CardsEstadoDeSolicitudesModel> SolicitudesUsuarios = new();
    
    
    #region PANEL DE GESTIONES DE DATOS DE USUARIO
    
        private class CardsCambiosUsuariosModel
        {
            public string Titulo { get; set; } = "";
            public string Icono { get; set; } = "";
            public string Color { get; set; } = "";
            public string Borde { get; set; } = "";
            public string Route { get; set; } = "";
        }
        
        private List<CardsCambiosUsuariosModel> CambiosUsuarios = new()
        {
            new()
            {
                Titulo   = "Editar mis datos",
                Icono    = "bi bi-person-fill-gear",
                Color    = "info",
                Borde    = "#0dcaf0",
                Route    = "/Editar_datos_users"
            },
            new()
            {
                Titulo   = "Cambiar contraseña",
                Icono    = "bi bi-asterisk",
                Color    = "primary",
                Borde    = "#4B99DA",
                Route    = "/Account/Manage/Change-password"
            }
        };
        
        private void IrAGestionesDatosDeUsuario(string route)
        {
            if (!string.IsNullOrWhiteSpace(route)) { Navigation.NavigateTo(route); }
        }
        
    # endregion
    
    #region CREACIÓN DE PANEL DE SOLICITUDES DE REGISTROS DE PERSONAS Y EMPRESAS
    
        private class CardsEstadoDeSolicitudesModel
        {
            public byte Clave { get; init; }
            public string Titulo { get; init; } = "";
            public string Subtitulo { get; init; } = "";
            public string Icono { get; init; } = "";
            public string Color { get; init; } = "";
            public string Fondo { get; init; } = "";
            public string Borde { get; init; } = "";
            public int Cantidad { get; set; }
        }
        
        private static readonly Dictionary<byte, (string Subtitulo, string Icono, string Color, string Fondo, string Borde)> EstadoConfig
            = new()
            {
                { 0, ("Pendientes", "bi bi-send-fill", "info", "rgba(13,202,240,0.05)", "#0dcaf0") },
                { 1, ("Procesando", "bi bi-clock-fill", "warning", "rgba(255,193,7,0.05)", "#ffc107") },
                { 2, ("Completadas","bi bi-check-circle-fill","success", "rgba(25,135,84,0.05)", "#198754") },
                { 3, ("Denegadas",  "bi bi-x-circle-fill","danger", "rgba(220,53,69,0.05)", "#dc3545") }
            };
        
    #endregion
}