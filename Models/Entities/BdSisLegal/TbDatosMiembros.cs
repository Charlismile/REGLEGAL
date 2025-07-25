﻿using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbDatosMiembros
{
    public int DmiembroId { get; set; }

    public string NombreMiembro { get; set; } = null!;

    public string CedulaMiembro { get; set; } = null!;

    public int CargoId { get; set; }

    public int? DcomiteId { get; set; }

    public virtual TbCargosMiembrosComite Cargo { get; set; } = null!;

    public virtual TbDatosComite? Dcomite { get; set; }
}
