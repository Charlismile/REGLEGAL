using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbDatosMiembrosHistorial
{
    public int HistorialMiembroId { get; set; }

    public int MiembroId { get; set; }

    public int CargoId { get; set; }

    public int? ComiteId { get; set; }

    public DateTime FechaModificacion { get; set; }

    public string NombreMiembro { get; set; } = null!;

    public string ApellidoMiembro { get; set; } = null!;

    public string CedulaMiembro { get; set; } = null!;

    public string? TelefonoMiembro { get; set; }

    public string? CorreoMiembro { get; set; }

    public DateTime FechaCambio { get; set; }

    public virtual TbCargosMiembrosComite Cargo { get; set; } = null!;

    public virtual TbComite? Comite { get; set; }

    public virtual TbMiembrosComite Miembro { get; set; } = null!;
}
