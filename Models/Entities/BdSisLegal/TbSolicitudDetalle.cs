using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbSolicitudDetalle
{
    public int SolicitudDetalleId { get; set; }

    public int SolicitudId { get; set; }

    public string? Comentario { get; set; }

    public DateTime FechaCambio { get; set; }

    public string UsuarioCambio { get; set; } = null!;

    public virtual TbSolicitud Solicitud { get; set; } = null!;
}
