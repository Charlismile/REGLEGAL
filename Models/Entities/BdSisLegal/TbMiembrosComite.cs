using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbMiembrosComite
{
    public int DmiembroId { get; set; }

    public string NombreMiembro { get; set; } = null!;

    public string CedulaMiembro { get; set; } = null!;

    public int CargoId { get; set; }

    public int? DcomiteId { get; set; }

    public string? ApellidoMiembro { get; set; }

    public virtual TbComite? Dcomite { get; set; }

    public virtual ICollection<TbDatosMiembrosHistorial> TbDatosMiembrosHistorial { get; set; } = new List<TbDatosMiembrosHistorial>();
}
