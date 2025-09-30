using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbDetalleRegComite
{
    public int DetalleRegComiteId { get; set; }

    public int ComiteId { get; set; }

    public int TipoTramiteId { get; set; }

    public DateTime CreadaEn { get; set; }

    public string CreadaPor { get; set; } = null!;

    public int NumRegCoSecuencia { get; set; }

    public int NomRegCoAnio { get; set; }

    public int NumRegCoMes { get; set; }

    public string? NumRegCoCompleta { get; set; }

    public string? NumeroNota { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaEleccion { get; set; }

    public string? NumeroResolucion { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public string? NombreComiteSalud { get; set; }

    public string? Comunidad { get; set; }

    public virtual TbDatosComite Comite { get; set; } = null!;

    public virtual ICollection<TbComiteArchivos> TbComiteArchivos { get; set; } = new List<TbComiteArchivos>();

    public virtual ICollection<TbDetalleRegComiteHistorial> TbDetalleRegComiteHistorial { get; set; } = new List<TbDetalleRegComiteHistorial>();

    public virtual TbTipoTramite TipoTramite { get; set; } = null!;
}
