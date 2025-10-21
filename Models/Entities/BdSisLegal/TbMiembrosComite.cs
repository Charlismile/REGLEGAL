using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbMiembrosComite
{
    public int MiembroId { get; set; }

    public string NombreMiembro { get; set; } = null!;

    public string CedulaMiembro { get; set; } = null!;

    public int CargoId { get; set; }

    public int? ComiteId { get; set; }

    public string? ApellidoMiembro { get; set; }

    public virtual TbComite? Comite { get; set; }

    public virtual ICollection<TbCargosMiembrosComite> TbCargosMiembrosComite { get; set; } = new List<TbCargosMiembrosComite>();

    public virtual ICollection<TbDatosMiembrosHistorial> TbDatosMiembrosHistorial { get; set; } = new List<TbDatosMiembrosHistorial>();
}
