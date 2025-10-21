using System;
using System.Collections.Generic;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class TbComite
{
    public int ComiteId { get; set; }

    public string? NumeroNota { get; set; }

    public string NombreComiteSalud { get; set; } = null!;

    public string? Comunidad { get; set; }

    public DateTime? FechaEleccion { get; set; }

    public string? NumeroResolucion { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public string? CreadaPor { get; set; }

    public int? RegionSaludId { get; set; }

    public int? ProvinciaId { get; set; }

    public int? DistritoId { get; set; }

    public int? CorregimientoId { get; set; }

    public int TipoTramite { get; set; }

    public virtual TbCorregimiento? Corregimiento { get; set; }

    public virtual TbDistrito? Distrito { get; set; }

    public virtual TbProvincia? Provincia { get; set; }

    public virtual TbRegionSalud? RegionSalud { get; set; }

    public virtual ICollection<TbArchivosComite> TbArchivosComite { get; set; } = new List<TbArchivosComite>();

    public virtual ICollection<TbDatosMiembrosHistorial> TbDatosMiembrosHistorial { get; set; } = new List<TbDatosMiembrosHistorial>();

    public virtual ICollection<TbDetalleRegComiteHistorial> TbDetalleRegComiteHistorial { get; set; } = new List<TbDetalleRegComiteHistorial>();

    public virtual ICollection<TbMiembrosComite> TbMiembrosComite { get; set; } = new List<TbMiembrosComite>();
}
