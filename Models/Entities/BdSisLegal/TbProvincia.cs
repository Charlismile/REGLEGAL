using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbProvincia
{
    public int ProvinciaId { get; set; }

    public string NombreProvincia { get; set; } = null!;

    public int? RegionSaludId { get; set; }

    public virtual TbRegionSalud? RegionSalud { get; set; }

    public virtual ICollection<TbDatosComite> TbDatosComite { get; set; } = new List<TbDatosComite>();

    public virtual ICollection<TbDistrito> TbDistrito { get; set; } = new List<TbDistrito>();
}
