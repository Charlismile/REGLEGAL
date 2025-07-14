using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbAsociacion
{
    public int AsociacionId { get; set; }

    public string NombreAsociacion { get; set; } = null!;

    public int RepresentanteLegalId { get; set; }

    public int ApoderadoLegalId { get; set; }

    public int Folio { get; set; }

    public string? Actividad { get; set; }

    public virtual TbApoderadoLegal ApoderadoLegal { get; set; } = null!;

    public virtual TbRepresentanteLegal RepresentanteLegal { get; set; } = null!;
}
