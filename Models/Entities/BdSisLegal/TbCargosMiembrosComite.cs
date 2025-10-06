using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbCargosMiembrosComite
{
    public int CargoId { get; set; }

    public string NombreCargo { get; set; } = null!;

    public bool IsActivo { get; set; }

    public int? MiembroId { get; set; }

    public virtual ICollection<TbDatosMiembrosHistorial> TbDatosMiembrosHistorial { get; set; } = new List<TbDatosMiembrosHistorial>();

    public virtual ICollection<TbMiembrosComite> TbMiembrosComite { get; set; } = new List<TbMiembrosComite>();
}
