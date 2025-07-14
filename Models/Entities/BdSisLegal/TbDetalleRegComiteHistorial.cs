using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbDetalleRegComiteHistorial
{
    public int RegComiteSolId { get; set; }

    public int ComiteId { get; set; }

    public int CoEstadoSolicitudId { get; set; }

    public string? ComentarioCo { get; set; }

    public string UsuarioRevisorCo { get; set; } = null!;

    public DateTime FechaCambioCo { get; set; }

    public virtual TbEstadoSolicitud CoEstadoSolicitud { get; set; } = null!;

    public virtual TbDetalleRegComite Comite { get; set; } = null!;
}
