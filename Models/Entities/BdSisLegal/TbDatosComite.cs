using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbDatosComite
{
    public int DcomiteId { get; set; }

    public string NombreComiteSalud { get; set; } = null!;

    public string? Comunidad { get; set; }

    public int RegionSaludId { get; set; }

    public int ProvinciaId { get; set; }

    public int DistritoId { get; set; }

    public int CorregimientoId { get; set; }

    public virtual ICollection<TbDatosMiembros> TbDatosMiembros { get; set; } = new List<TbDatosMiembros>();
}
