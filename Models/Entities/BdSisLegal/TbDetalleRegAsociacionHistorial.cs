using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbDetalleRegAsociacionHistorial
{
    public int HistorialId { get; set; }

    public int DetRegAsociacionId { get; set; }

    public int AsociacionId { get; set; }

    public string UsuarioId { get; set; } = null!;

    public DateTime? FechaModificacion { get; set; }

    public string? NumeroResolucion { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public string Accion { get; set; } = null!;

    public string? Comentario { get; set; }

    public virtual TbAsociacion Asociacion { get; set; } = null!;

    public virtual TbDetalleRegAsociacion DetRegAsociacion { get; set; } = null!;
}
