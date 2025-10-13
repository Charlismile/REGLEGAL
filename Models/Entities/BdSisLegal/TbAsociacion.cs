using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbAsociacion
{
    public int AsociacionId { get; set; }

    public string NombreAsociacion { get; set; } = null!;

    public int? RepresentanteLegalId { get; set; }

    public int? ApoderadoLegalId { get; set; }

    public int Folio { get; set; }

    public string? Actividad { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public string? NumeroResolucion { get; set; }

    public virtual TbApoderadoLegal? ApoderadoLegal { get; set; }

    public virtual TbRepresentanteLegal? RepresentanteLegal { get; set; }

    public virtual ICollection<TbArchivosAsociacion> TbArchivosAsociacion { get; set; } = new List<TbArchivosAsociacion>();

    public virtual ICollection<TbDetalleRegAsociacion> TbDetalleRegAsociacion { get; set; } = new List<TbDetalleRegAsociacion>();

    public virtual ICollection<TbDetalleRegAsociacionHistorial> TbDetalleRegAsociacionHistorial { get; set; } = new List<TbDetalleRegAsociacionHistorial>();
}
