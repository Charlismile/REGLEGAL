using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbDetalleRegAsociacion
{
    public int AsociacionId { get; set; }

    public DateTime CreadaEn { get; set; }

    public string CreadaPor { get; set; } = null!;

    public int NumRegAsecuencia { get; set; }

    public int NomRegAanio { get; set; }

    public int NumRegAmes { get; set; }

    public string? NumRegAcompleta { get; set; }

    public virtual ICollection<TbAsociacionArchivos> TbAsociacionArchivos { get; set; } = new List<TbAsociacionArchivos>();

    public virtual ICollection<TbDetalleRegAsociacionHistorial> TbDetalleRegAsociacionHistorial { get; set; } = new List<TbDetalleRegAsociacionHistorial>();
}
