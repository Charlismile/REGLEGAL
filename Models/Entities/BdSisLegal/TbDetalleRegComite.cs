using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbDetalleRegComite
{
    public int DetRegComiteId { get; set; }

    public int ComiteId { get; set; }

    public int CoEstadoSolicitudId { get; set; }

    public DateTime? CreadaEn { get; set; }

    public string? CreadaPor { get; set; }

    public DateTime? ModificadaEn { get; set; }

    public string? ModificadaPor { get; set; }

    public string? NumeroRegistro { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public virtual TbComite Comite { get; set; } = null!;

    public virtual ICollection<TbDetalleRegComiteHistorial> TbDetalleRegComiteHistorial { get; set; } = new List<TbDetalleRegComiteHistorial>();
}
