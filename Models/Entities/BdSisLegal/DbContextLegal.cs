using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace REGISTROLEGAL.Models.Entities.BdSisLegal;

public partial class DbContextLegal : DbContext
{
    public DbContextLegal()
    {
    }

    public DbContextLegal(DbContextOptions<DbContextLegal> options)
        : base(options)
    {
    }

    public virtual DbSet<TbApoderadoFirma> TbApoderadoFirma { get; set; }

    public virtual DbSet<TbApoderadoLegal> TbApoderadoLegal { get; set; }

    public virtual DbSet<TbArchivos> TbArchivos { get; set; }

    public virtual DbSet<TbAsociacion> TbAsociacion { get; set; }

    public virtual DbSet<TbCargosMiembrosComite> TbCargosMiembrosComite { get; set; }

    public virtual DbSet<TbCorregimiento> TbCorregimiento { get; set; }

    public virtual DbSet<TbDatosComite> TbDatosComite { get; set; }

    public virtual DbSet<TbDatosMiembros> TbDatosMiembros { get; set; }

    public virtual DbSet<TbDetalleRegAsociacion> TbDetalleRegAsociacion { get; set; }

    public virtual DbSet<TbDetalleRegAsociacionHistorial> TbDetalleRegAsociacionHistorial { get; set; }

    public virtual DbSet<TbDetalleRegComite> TbDetalleRegComite { get; set; }

    public virtual DbSet<TbDetalleRegComiteHistorial> TbDetalleRegComiteHistorial { get; set; }

    public virtual DbSet<TbDistrito> TbDistrito { get; set; }

    public virtual DbSet<TbEstadoSolicitud> TbEstadoSolicitud { get; set; }

    public virtual DbSet<TbProvincia> TbProvincia { get; set; }

    public virtual DbSet<TbRegSecuencia> TbRegSecuencia { get; set; }

    public virtual DbSet<TbRegionSalud> TbRegionSalud { get; set; }

    public virtual DbSet<TbRepresentanteLegal> TbRepresentanteLegal { get; set; }

    public virtual DbSet<TbTipoTramite> TbTipoTramite { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbApoderadoFirma>(entity =>
        {
            entity.HasKey(e => e.FirmaId).HasName("PK__TbApoder__CD9C5E2F9FEE8FEB");

            entity.Property(e => e.CorreoFirma).HasMaxLength(200);
            entity.Property(e => e.DireccionFirma).HasMaxLength(500);
            entity.Property(e => e.NombreFirma).HasMaxLength(200);
            entity.Property(e => e.TelefonoFirma)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TbApoderadoLegal>(entity =>
        {
            entity.HasKey(e => e.ApoAbogadoId).HasName("PK__TbApoder__424F7A70EBE2E5E9");

            entity.Property(e => e.ApellidoApoAbogado).HasMaxLength(200);
            entity.Property(e => e.CedulaApoAbogado)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CorreoApoAbogado).HasMaxLength(200);
            entity.Property(e => e.DireccionApoAbogado).HasMaxLength(500);
            entity.Property(e => e.NombreApoAbogado).HasMaxLength(200);
            entity.Property(e => e.TelefonoApoAbogado)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ApoderadoFirma).WithMany(p => p.TbApoderadoLegal)
                .HasForeignKey(d => d.ApoderadoFirmaId)
                .HasConstraintName("FK_ApoderadoLegal_ApoderadoFirma");
        });

        modelBuilder.Entity<TbArchivos>(entity =>
        {
            entity.HasKey(e => e.ArchivoId).HasName("PK__TbArchiv__3D24274A8FA1F287");

            entity.HasIndex(e => e.DetRegAsociacionId, "IX_TbArchivos_DetRegAsociacion");

            entity.HasIndex(e => e.DetRegComiteId, "IX_TbArchivos_DetRegComite");

            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.FechaSubida)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActivo).HasDefaultValue(true);
            entity.Property(e => e.NombreArchivoGuardado).HasMaxLength(500);
            entity.Property(e => e.NombreOriginal).HasMaxLength(500);
            entity.Property(e => e.Url).HasMaxLength(1000);
            entity.Property(e => e.Version).HasDefaultValue(1);

            entity.HasOne(d => d.DetRegAsociacion).WithMany(p => p.TbArchivos)
                .HasForeignKey(d => d.DetRegAsociacionId)
                .HasConstraintName("FK_Archivos_DetalleRegAsociacion");

            entity.HasOne(d => d.DetRegComite).WithMany(p => p.TbArchivos)
                .HasForeignKey(d => d.DetRegComiteId)
                .HasConstraintName("FK_Archivos_DetalleRegComite");
        });

        modelBuilder.Entity<TbAsociacion>(entity =>
        {
            entity.HasKey(e => e.AsociacionId).HasName("PK__TbAsocia__5B58E10505500571");

            entity.Property(e => e.Actividad).HasMaxLength(1000);
            entity.Property(e => e.NombreAsociacion).HasMaxLength(200);

            entity.HasOne(d => d.ApoderadoLegal).WithMany(p => p.TbAsociacion)
                .HasForeignKey(d => d.ApoderadoLegalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asociacion_ApoderadoLegal");

            entity.HasOne(d => d.RepresentanteLegal).WithMany(p => p.TbAsociacion)
                .HasForeignKey(d => d.RepresentanteLegalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asociacion_RepresentanteLegal");
        });

        modelBuilder.Entity<TbCargosMiembrosComite>(entity =>
        {
            entity.HasKey(e => e.CargoId).HasName("PK__TbCargos__B4E665CD75F0DDFE");

            entity.Property(e => e.IsActivo).HasDefaultValue(true);
            entity.Property(e => e.NombreCargo).HasMaxLength(100);
        });

        modelBuilder.Entity<TbCorregimiento>(entity =>
        {
            entity.HasKey(e => e.CorregimientoId).HasName("PK__TbCorreg__5F50F199086D1C25");

            entity.Property(e => e.NombreCorregimiento).HasMaxLength(100);

            entity.HasOne(d => d.Distrito).WithMany(p => p.TbCorregimiento)
                .HasForeignKey(d => d.DistritoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Corregimiento_Distrito");
        });

        modelBuilder.Entity<TbDatosComite>(entity =>
        {
            entity.HasKey(e => e.DcomiteId).HasName("PK__TbDatosC__F23C59512BD93BC1");

            entity.HasIndex(e => e.CorregimientoId, "IX_TbDatosComite_Corregimiento");

            entity.HasIndex(e => e.DistritoId, "IX_TbDatosComite_Distrito");

            entity.HasIndex(e => e.ProvinciaId, "IX_TbDatosComite_Provincia");

            entity.HasIndex(e => e.RegionSaludId, "IX_TbDatosComite_RegionSalud");

            entity.Property(e => e.DcomiteId).HasColumnName("DComiteId");
            entity.Property(e => e.Comunidad).HasMaxLength(200);
            entity.Property(e => e.NombreComiteSalud).HasMaxLength(200);

            entity.HasOne(d => d.Corregimiento).WithMany(p => p.TbDatosComite)
                .HasForeignKey(d => d.CorregimientoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatosComite_Corregimiento");

            entity.HasOne(d => d.Distrito).WithMany(p => p.TbDatosComite)
                .HasForeignKey(d => d.DistritoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatosComite_Distrito");

            entity.HasOne(d => d.Provincia).WithMany(p => p.TbDatosComite)
                .HasForeignKey(d => d.ProvinciaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatosComite_Provincia");

            entity.HasOne(d => d.RegionSalud).WithMany(p => p.TbDatosComite)
                .HasForeignKey(d => d.RegionSaludId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatosComite_RegionSalud");
        });

        modelBuilder.Entity<TbDatosMiembros>(entity =>
        {
            entity.HasKey(e => e.DmiembroId).HasName("PK__TbDatosM__E71F412FE58100EB");

            entity.HasIndex(e => e.DcomiteId, "IX_TbDatosMiembros_DcomiteId");

            entity.Property(e => e.DmiembroId).HasColumnName("DMiembroId");
            entity.Property(e => e.ApellidoMiembro).HasMaxLength(200);
            entity.Property(e => e.CedulaMiembro)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NombreMiembro).HasMaxLength(200);

            entity.HasOne(d => d.Cargo).WithMany(p => p.TbDatosMiembros)
                .HasForeignKey(d => d.CargoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatosMiembros_CargosMiembros");

            entity.HasOne(d => d.Dcomite).WithMany(p => p.TbDatosMiembros)
                .HasForeignKey(d => d.DcomiteId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_DatosMiembros_Comite");
        });

        modelBuilder.Entity<TbDetalleRegAsociacion>(entity =>
        {
            entity.HasKey(e => e.AsociacionId).HasName("PK__TbDetall__5B58E10548EEA62B");

            entity.HasIndex(e => e.CreadaPor, "IX_TbDetalleRegAsociacion_CreadaPor");

            entity.Property(e => e.CreadaEn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NomRegAanio).HasColumnName("NomRegAAnio");
            entity.Property(e => e.NumRegAcompleta)
                .HasMaxLength(18)
                .HasComputedColumnSql("((((CONVERT([nvarchar](10),[NumRegASecuencia])+'/')+CONVERT([nvarchar](4),[NomRegAAnio]))+'/')+CONVERT([nvarchar](2),[NumRegAMes]))", true)
                .HasColumnName("NumRegACompleta");
            entity.Property(e => e.NumRegAmes).HasColumnName("NumRegAMes");
            entity.Property(e => e.NumRegAsecuencia).HasColumnName("NumRegASecuencia");
        });

        modelBuilder.Entity<TbDetalleRegAsociacionHistorial>(entity =>
        {
            entity.HasKey(e => e.RegAsociacionSolId).HasName("PK__TbDetall__83B0F9633E50A930");

            entity.HasIndex(e => e.AsociacionId, "IX_TbAsociacionHistorial_AsociacionId");

            entity.Property(e => e.ComentarioAso).HasMaxLength(1000);
            entity.Property(e => e.FechaCambioAso)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UsuarioRevisorAso).HasMaxLength(450);

            entity.HasOne(d => d.AsEstadoSolicitud).WithMany(p => p.TbDetalleRegAsociacionHistorial)
                .HasForeignKey(d => d.AsEstadoSolicitudId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AsociacionHistorial_EstadoSolicitud");

            entity.HasOne(d => d.Asociacion).WithMany(p => p.TbDetalleRegAsociacionHistorial)
                .HasForeignKey(d => d.AsociacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AsociacionHistorial_DetalleRegAsociacion");
        });

        modelBuilder.Entity<TbDetalleRegComite>(entity =>
        {
            entity.HasKey(e => e.ComiteId).HasName("PK__TbDetall__5799B3A3328837BC");

            entity.HasIndex(e => e.CreadaPor, "IX_TbDetalleRegComite_CreadaPor");

            entity.HasIndex(e => e.TipoTramiteId, "IX_TbDetalleRegComite_TipoTramite");

            entity.Property(e => e.CreadaEn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NumRegCoCompleta)
                .HasMaxLength(18)
                .HasComputedColumnSql("((((CONVERT([nvarchar](10),[NumRegCoSecuencia])+'/')+CONVERT([nvarchar](4),[NomRegCoAnio]))+'/')+CONVERT([nvarchar](2),[NumRegCoMes]))", true);

            entity.HasOne(d => d.TipoTramite).WithMany(p => p.TbDetalleRegComite)
                .HasForeignKey(d => d.TipoTramiteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleRegComite_TipoTramite");
        });

        modelBuilder.Entity<TbDetalleRegComiteHistorial>(entity =>
        {
            entity.HasKey(e => e.RegComiteSolId).HasName("PK__TbDetall__316C49763E04D4D3");

            entity.HasIndex(e => e.ComiteId, "IX_TbComiteHistorial_ComiteId");

            entity.Property(e => e.ComentarioCo).HasMaxLength(1000);
            entity.Property(e => e.FechaCambioCo)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UsuarioRevisorCo).HasMaxLength(450);

            entity.HasOne(d => d.CoEstadoSolicitud).WithMany(p => p.TbDetalleRegComiteHistorial)
                .HasForeignKey(d => d.CoEstadoSolicitudId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ComiteHistorial_EstadoSolicitud");

            entity.HasOne(d => d.Comite).WithMany(p => p.TbDetalleRegComiteHistorial)
                .HasForeignKey(d => d.ComiteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ComiteHistorial_DetalleRegComite");
        });

        modelBuilder.Entity<TbDistrito>(entity =>
        {
            entity.HasKey(e => e.DistritoId).HasName("PK__TbDistri__BE6ADADD32E7548A");

            entity.Property(e => e.NombreDistrito).HasMaxLength(100);

            entity.HasOne(d => d.Provincia).WithMany(p => p.TbDistrito)
                .HasForeignKey(d => d.ProvinciaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Distrito_Provincia");
        });

        modelBuilder.Entity<TbEstadoSolicitud>(entity =>
        {
            entity.HasKey(e => e.Estado).HasName("PK__TbEstado__36DF552E57242512");

            entity.Property(e => e.Estado).ValueGeneratedNever();
            entity.Property(e => e.Descripcion).HasMaxLength(100);
        });

        modelBuilder.Entity<TbProvincia>(entity =>
        {
            entity.HasKey(e => e.ProvinciaId).HasName("PK__TbProvin__F7CBC777111E15E1");

            entity.Property(e => e.NombreProvincia).HasMaxLength(100);

            entity.HasOne(d => d.RegionSalud).WithMany(p => p.TbProvincia)
                .HasForeignKey(d => d.RegionSaludId)
                .HasConstraintName("FK_Provincia_RegionSalud");
        });

        modelBuilder.Entity<TbRegSecuencia>(entity =>
        {
            entity.HasKey(e => e.SecuenciaId).HasName("PK__TbRegSec__00D46D281C7C4C3E");

            entity.Property(e => e.Activo).HasDefaultValue(true);
        });

        modelBuilder.Entity<TbRegionSalud>(entity =>
        {
            entity.HasKey(e => e.RegionSaludId).HasName("PK__TbRegion__2B018A54B22A0920");

            entity.Property(e => e.NombreRegion).HasMaxLength(150);
        });

        modelBuilder.Entity<TbRepresentanteLegal>(entity =>
        {
            entity.HasKey(e => e.RepLegalId).HasName("PK__TbRepres__4E77DD119801C2BF");

            entity.Property(e => e.ApellidoRepLegal).HasMaxLength(200);
            entity.Property(e => e.CargoRepLegal).HasMaxLength(100);
            entity.Property(e => e.CedulaRepLegal)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DireccionRepLegal).HasMaxLength(500);
            entity.Property(e => e.NombreRepLegal).HasMaxLength(200);
            entity.Property(e => e.TelefonoRepLegal)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TbTipoTramite>(entity =>
        {
            entity.HasKey(e => e.TramiteId).HasName("PK__TbTipoTr__BDB7931664664CFC");

            entity.Property(e => e.IsActivo).HasDefaultValue(true);
            entity.Property(e => e.NombreTramite).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
