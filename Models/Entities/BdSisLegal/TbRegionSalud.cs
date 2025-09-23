using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbRegionSalud
{
    public int RegionSaludId { get; set; }

    public string NombreRegion { get; set; } = null!;

    public virtual ICollection<TbProvincia> TbProvincia { get; set; } = new List<TbProvincia>();
}
