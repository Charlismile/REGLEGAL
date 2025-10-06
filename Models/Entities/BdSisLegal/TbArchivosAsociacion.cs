using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbArchivosAsociacion
{
    public int ArchivoId { get; set; }

    public int AsociacionId { get; set; }

    public string Categoria { get; set; } = null!;

    public string NombreOriginal { get; set; } = null!;

    public string NombreArchivoGuardado { get; set; } = null!;

    public string Url { get; set; } = null!;

    public DateTime FechaSubida { get; set; }

    public int Version { get; set; }

    public bool IsActivo { get; set; }

    public virtual TbAsociacion Asociacion { get; set; } = null!;
}
