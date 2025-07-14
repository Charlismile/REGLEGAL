using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbApoderadoFirma
{
    public int FirmaId { get; set; }

    public string NombreFirma { get; set; } = null!;

    public string? CorreoFirma { get; set; }

    public string? TelefonoFirma { get; set; }

    public string? DireccionFirma { get; set; }

    public virtual ICollection<TbApoderadoLegal> TbApoderadoLegal { get; set; } = new List<TbApoderadoLegal>();
}
