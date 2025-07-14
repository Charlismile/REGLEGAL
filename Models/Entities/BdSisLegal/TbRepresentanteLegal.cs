using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbRepresentanteLegal
{
    public int RepLegalId { get; set; }

    public string NombreRepLegal { get; set; } = null!;

    public string CedulaRepLegal { get; set; } = null!;

    public string CargoRepLegal { get; set; } = null!;

    public string? TelefonoRepLegal { get; set; }

    public string? DireccionRepLegal { get; set; }

    public virtual ICollection<TbAsociacion> TbAsociacion { get; set; } = new List<TbAsociacion>();
}
