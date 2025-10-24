// using REGISTROLEGAL.Models.Entities.BdSisLegal;

using REGISTROLEGAL.Models.Entities.BdSisLegal;

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
        
        public int DMiembroId { get; set; }
        public string NombreMiembro { get; set; } = string.Empty;
        public int CargoId { get; set; }
        public DateTime FechaCambio { get; set; }
        
        public virtual TbComite? Comite { get; set; }
    }
}