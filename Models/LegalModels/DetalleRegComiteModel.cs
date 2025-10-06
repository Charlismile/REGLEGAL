// using REGISTROLEGAL.Models.Entities.BdSisLegal;

namespace REGISTROLEGAL.Models.LegalModels
{
    public class DetalleRegComiteModel
    {
        public int RegComiteSolId { get; set; }
        public int ComiteId { get; set; }
        public int CoEstadoSolicitudId { get; set; }
        public string? ComentarioCo { get; set; }
        public string UsuarioRevisorCo { get; set; } = string.Empty;
        public DateTime FechaCambioCo { get; set; }

        // Agregamos estas propiedades para mapear TbDatosMiembrosHistorial
        public int DMiembroId { get; set; }
        public string NombreMiembro { get; set; } = string.Empty;
        public int CargoId { get; set; }
        public DateTime FechaCambio { get; set; }

        // Relación con el comité (opcional, si usas EF Core)
        // public virtual TbComite? Comite { get; set; }
    }
}