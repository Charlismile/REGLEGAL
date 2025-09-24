using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbEstadoSolicitud
{
    public int Estado { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<TbDetalleRegAsociacionHistorial> TbDetalleRegAsociacionHistorial { get; set; } = new List<TbDetalleRegAsociacionHistorial>();

    public virtual ICollection<TbDetalleRegComiteHistorial> TbDetalleRegComiteHistorial { get; set; } = new List<TbDetalleRegComiteHistorial>();

    public virtual ICollection<TbSolicitud> TbSolicitud { get; set; } = new List<TbSolicitud>();
}
