using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbSolicitud
{
    public int SolicitudId { get; set; }

    public string Entidad { get; set; } = null!;

    public int EntidadId { get; set; }

    public int EstadoId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreador { get; set; } = null!;

    public string? Comentario { get; set; }

    public virtual TbEstadoSolicitud Estado { get; set; } = null!;

    public virtual ICollection<TbSolicitudDetalle> TbSolicitudDetalle { get; set; } = new List<TbSolicitudDetalle>();
}
