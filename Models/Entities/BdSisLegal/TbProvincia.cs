using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbProvincia
{
    public int ProvinciaId { get; set; }

    public string NombreProvincia { get; set; } = null!;

    public int? RegionSaludId { get; set; }

    public virtual ICollection<TbComite> TbComite { get; set; } = new List<TbComite>();

    public virtual ICollection<TbDistrito> TbDistrito { get; set; } = new List<TbDistrito>();
}
