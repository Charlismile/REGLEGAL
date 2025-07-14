using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbDetalleRegAsociacionHistorial
{
    public int RegAsociacionSolId { get; set; }

    public int AsociacionId { get; set; }

    public int AsEstadoSolicitudId { get; set; }

    public string? ComentarioAso { get; set; }

    public string UsuarioRevisorAso { get; set; } = null!;

    public DateTime FechaCambioAso { get; set; }

    public virtual TbEstadoSolicitud AsEstadoSolicitud { get; set; } = null!;

    public virtual TbDetalleRegAsociacion Asociacion { get; set; } = null!;
}
