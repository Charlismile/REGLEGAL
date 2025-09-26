using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbDatosMiembrosHistorial
{
    public int HistorialMiembroId { get; set; }

    public int DmiembroId { get; set; }

    public int CargoId { get; set; }

    public int? DcomiteId { get; set; }

    public DateTime FechaModificacion { get; set; }

    public string NombreMiembro { get; set; } = null!;

    public string ApellidoMiembro { get; set; } = null!;

    public string CedulaMiembro { get; set; } = null!;

    public string? TelefonoMiembro { get; set; }

    public string? CorreoMiembro { get; set; }

    public DateTime FechaCambio { get; set; }
}
