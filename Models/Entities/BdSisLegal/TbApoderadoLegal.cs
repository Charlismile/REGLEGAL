using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbApoderadoLegal
{
    public int ApoAbogadoId { get; set; }

    public string NombreApoAbogado { get; set; } = null!;

    public string CedulaApoAbogado { get; set; } = null!;

    public string? TelefonoApoAbogado { get; set; }

    public string? DireccionApoAbogado { get; set; }

    public string? CorreoApoAbogado { get; set; }

    public int? ApoderadoFirmaId { get; set; }

    public string? ApellidoApoAbogado { get; set; }

    public virtual TbApoderadoFirma? ApoderadoFirma { get; set; }

    public virtual ICollection<TbAsociacion> TbAsociacion { get; set; } = new List<TbAsociacion>();
}
