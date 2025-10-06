using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbRegionSalud
{
    public int RegionSaludId { get; set; }

    public string NombreRegion { get; set; } = null!;

    public virtual ICollection<TbComite> TbComite { get; set; } = new List<TbComite>();
}
