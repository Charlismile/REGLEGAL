using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbCorregimiento
{
    public int CorregimientoId { get; set; }

    public string NombreCorregimiento { get; set; } = null!;

    public int DistritoId { get; set; }

    public virtual TbDistrito Distrito { get; set; } = null!;
}
