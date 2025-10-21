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

    public virtual DbSet<TbArchivosAsociacion> TbArchivosAsociacion { get; set; }

    public virtual DbSet<TbArchivosComite> TbArchivosComite { get; set; }

    public virtual DbSet<TbAsociacion> TbAsociacion { get; set; }

    public virtual DbSet<TbCargosMiembrosComite> TbCargosMiembrosComite { get; set; }

    public virtual DbSet<TbComite> TbComite { get; set; }

    public virtual DbSet<TbCorregimiento> TbCorregimiento { get; set; }

    public virtual DbSet<TbDatosMiembrosHistorial> TbDatosMiembrosHistorial { get; set; }

    public virtual DbSet<TbDetalleRegAsociacion> TbDetalleRegAsociacion { get; set; }

    public virtual DbSet<TbDetalleRegAsociacionHistorial> TbDetalleRegAsociacionHistorial { get; set; }

    public virtual DbSet<TbDetalleRegComiteHistorial> TbDetalleRegComiteHistorial { get; set; }

    public virtual DbSet<TbDistrito> TbDistrito { get; set; }

    public virtual DbSet<TbMiembrosComite> TbMiembrosComite { get; set; }

    public virtual DbSet<TbProvincia> TbProvincia { get; set; }

    public virtual DbSet<TbRegSecuencia> TbRegSecuencia { get; set; }

    public virtual DbSet<TbRegionSalud> TbRegionSalud { get; set; }

    public virtual DbSet<TbRepresentanteLegal> TbRepresentanteLegal { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbApoderadoFirma>(entity =>
        {
            entity.HasKey(e => e.FirmaId).HasName("PK__TbApoder__CD9C5E2F21021BA6");

            entity.Property(e => e.CorreoFirma).HasMaxLength(200);
            entity.Property(e => e.DireccionFirma).HasMaxLength(500);
            entity.Property(e => e.NombreFirma).HasMaxLength(200);
            entity.Property(e => e.TelefonoFirma)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TbApoderadoLegal>(entity =>
        {
            entity.HasKey(e => e.ApoAbogadoId).HasName("PK__TbApoder__424F7A70D277BB25");

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

        modelBuilder.Entity<TbArchivosAsociacion>(entity =>
        {
            entity.HasKey(e => e.ArchivoId).HasName("PK__TbArchiv__3D24274AE173E900");

            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.FechaSubida)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActivo).HasDefaultValue(true);
            entity.Property(e => e.NombreArchivoGuardado).HasMaxLength(500);
            entity.Property(e => e.NombreOriginal).HasMaxLength(500);
            entity.Property(e => e.Url).HasMaxLength(1000);
            entity.Property(e => e.Version).HasDefaultValue(1);

            entity.HasOne(d => d.Asociacion).WithMany(p => p.TbArchivosAsociacion)
                .HasForeignKey(d => d.AsociacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArchivosAsociacion_Asociacion");
        });

        modelBuilder.Entity<TbArchivosComite>(entity =>
        {
            entity.HasKey(e => e.ArchivoId).HasName("PK__TbArchiv__3D24274A8A26F455");

            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.FechaSubida)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActivo).HasDefaultValue(true);
            entity.Property(e => e.NombreArchivoGuardado).HasMaxLength(500);
            entity.Property(e => e.NombreOriginal).HasMaxLength(500);
            entity.Property(e => e.Url).HasMaxLength(1000);
            entity.Property(e => e.Version).HasDefaultValue(1);

            entity.HasOne(d => d.Comite).WithMany(p => p.TbArchivosComite)
                .HasForeignKey(d => d.ComiteId)
                .HasConstraintName("FK_ArchivosComite_Comite");
        });

        modelBuilder.Entity<TbAsociacion>(entity =>
        {
            entity.HasKey(e => e.AsociacionId).HasName("PK__TbAsocia__5B58E10562FA7B35");

            entity.Property(e => e.Actividad).HasMaxLength(1000);
            entity.Property(e => e.FechaResolucion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreAsociacion).HasMaxLength(200);
            entity.Property(e => e.NumeroResolucion).HasMaxLength(50);

            entity.HasOne(d => d.ApoAbogado).WithMany(p => p.TbAsociacion)
                .HasForeignKey(d => d.ApoAbogadoId)
                .HasConstraintName("FK_Asociacion_ApoderadoLegal");

            entity.HasOne(d => d.RepLegal).WithMany(p => p.TbAsociacion)
                .HasForeignKey(d => d.RepLegalId)
                .HasConstraintName("FK_Asociacion_RepresentanteLegal");
        });

        modelBuilder.Entity<TbCargosMiembrosComite>(entity =>
        {
            entity.HasKey(e => e.CargoId).HasName("PK__TbCargos__B4E665CD127DC0A8");

            entity.Property(e => e.IsActivo).HasDefaultValue(true);
            entity.Property(e => e.NombreCargo).HasMaxLength(100);

            entity.HasOne(d => d.Miembro).WithMany(p => p.TbCargosMiembrosComite)
                .HasForeignKey(d => d.MiembroId)
                .HasConstraintName("FK_CargosMiembrosComite_MiembrosComite");
        });

        modelBuilder.Entity<TbComite>(entity =>
        {
            entity.HasKey(e => e.ComiteId).HasName("PK__TbComite__F23C5951B0FB3063");

            entity.Property(e => e.Comunidad).HasMaxLength(150);
            entity.Property(e => e.CreadaPor).HasMaxLength(150);
            entity.Property(e => e.FechaEleccion).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaResolucion).HasColumnType("datetime");
            entity.Property(e => e.NombreComiteSalud).HasMaxLength(200);
            entity.Property(e => e.NumeroNota).HasMaxLength(50);
            entity.Property(e => e.NumeroResolucion).HasMaxLength(50);
            entity.Property(e => e.TipoTramite).HasDefaultValue(1);

            entity.HasOne(d => d.Corregimiento).WithMany(p => p.TbComite)
                .HasForeignKey(d => d.CorregimientoId)
                .HasConstraintName("FK_Comite_Corregimiento");

            entity.HasOne(d => d.Distrito).WithMany(p => p.TbComite)
                .HasForeignKey(d => d.DistritoId)
                .HasConstraintName("FK_Comite_Distrito");

            entity.HasOne(d => d.Provincia).WithMany(p => p.TbComite)
                .HasForeignKey(d => d.ProvinciaId)
                .HasConstraintName("FK_Comite_Provincia");

            entity.HasOne(d => d.RegionSalud).WithMany(p => p.TbComite)
                .HasForeignKey(d => d.RegionSaludId)
                .HasConstraintName("FK_Comite_RegionSalud");
        });

        modelBuilder.Entity<TbCorregimiento>(entity =>
        {
            entity.HasKey(e => e.CorregimientoId).HasName("PK__TbCorreg__5F50F199E1422FDB");

            entity.Property(e => e.NombreCorregimiento).HasMaxLength(150);

            entity.HasOne(d => d.Distrito).WithMany(p => p.TbCorregimiento)
                .HasForeignKey(d => d.DistritoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TbCorregimiento_TbDistrito");
        });

        modelBuilder.Entity<TbDatosMiembrosHistorial>(entity =>
        {
            entity.HasKey(e => e.HistorialMiembroId).HasName("PK__TbDatosM__8D04E3D830B5DD71");

            entity.Property(e => e.ApellidoMiembro)
                .HasMaxLength(150)
                .HasDefaultValue("");
            entity.Property(e => e.CedulaMiembro)
                .HasMaxLength(20)
                .HasDefaultValue("");
            entity.Property(e => e.CorreoMiembro).HasMaxLength(150);
            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.NombreMiembro)
                .HasMaxLength(150)
                .HasDefaultValue("");
            entity.Property(e => e.TelefonoMiembro).HasMaxLength(20);

            entity.HasOne(d => d.Cargo).WithMany(p => p.TbDatosMiembrosHistorial)
                .HasForeignKey(d => d.CargoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialDatosMiembros_Cargo");

            entity.HasOne(d => d.Comite).WithMany(p => p.TbDatosMiembrosHistorial)
                .HasForeignKey(d => d.ComiteId)
                .HasConstraintName("FK_DatosMiembrosHistorial_Comite");

            entity.HasOne(d => d.Miembro).WithMany(p => p.TbDatosMiembrosHistorial)
                .HasForeignKey(d => d.MiembroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatosMiembrosHistorial_Miembro");
        });

        modelBuilder.Entity<TbDetalleRegAsociacion>(entity =>
        {
            entity.HasKey(e => e.DetRegAsociacionId).HasName("PK__TbDetall__E37EFEA09BB0E1D9");

            entity.HasIndex(e => e.CreadaPor, "IX_TbDetalleRegAsociacion_CreadaPor");

            entity.Property(e => e.CreadaEn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaResolucion).HasColumnType("datetime");
            entity.Property(e => e.NomRegAanio).HasColumnName("NomRegAAnio");
            entity.Property(e => e.NumRegAcompleta)
                .HasMaxLength(18)
                .HasComputedColumnSql("((((CONVERT([nvarchar](10),[NumRegASecuencia])+'/')+CONVERT([nvarchar](4),[NomRegAAnio]))+'/')+CONVERT([nvarchar](2),[NumRegAMes]))", true)
                .HasColumnName("NumRegACompleta");
            entity.Property(e => e.NumRegAmes).HasColumnName("NumRegAMes");
            entity.Property(e => e.NumRegAsecuencia).HasColumnName("NumRegASecuencia");
            entity.Property(e => e.NumeroResolucion).HasMaxLength(50);

            entity.HasOne(d => d.Asociacion).WithMany(p => p.TbDetalleRegAsociacion)
                .HasForeignKey(d => d.AsociacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TbDetalleRegAsociacion_TbAsociacion");
        });

        modelBuilder.Entity<TbDetalleRegAsociacionHistorial>(entity =>
        {
            entity.HasKey(e => e.HistorialId);

            entity.ToTable("TbDetalleRegAsociacion_Historial");

            entity.Property(e => e.Accion).HasMaxLength(50);
            entity.Property(e => e.Comentario).HasMaxLength(1000);
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaResolucion).HasColumnType("datetime");
            entity.Property(e => e.NumeroResolucion).HasMaxLength(50);
            entity.Property(e => e.UsuarioId).HasMaxLength(450);

            entity.HasOne(d => d.Asociacion).WithMany(p => p.TbDetalleRegAsociacionHistorial)
                .HasForeignKey(d => d.AsociacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialDetalle_AsociacionRef");

            entity.HasOne(d => d.DetRegAsociacion).WithMany(p => p.TbDetalleRegAsociacionHistorial)
                .HasForeignKey(d => d.DetRegAsociacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialDetalle_Asociacion");
        });

        modelBuilder.Entity<TbDetalleRegComiteHistorial>(entity =>
        {
            entity.HasKey(e => e.RegComiteSolId).HasName("PK__TbDetall__316C49768AB60196");

            entity.HasIndex(e => e.ComiteId, "IX_TbComiteHistorial_ComiteId");

            entity.Property(e => e.ComentarioCo).HasMaxLength(1000);
            entity.Property(e => e.FechaCambioCo)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UsuarioRevisorCo).HasMaxLength(450);

            entity.HasOne(d => d.Comite).WithMany(p => p.TbDetalleRegComiteHistorial)
                .HasForeignKey(d => d.ComiteId)
                .HasConstraintName("FK_ComiteHistorial_DetalleRegComite");
        });

        modelBuilder.Entity<TbDistrito>(entity =>
        {
            entity.HasKey(e => e.DistritoId).HasName("PK__TbDistri__BE6ADADD8C040015");

            entity.Property(e => e.NombreDistrito).HasMaxLength(150);

            entity.HasOne(d => d.Provincia).WithMany(p => p.TbDistrito)
                .HasForeignKey(d => d.ProvinciaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TbDistrito_TbProvincia");
        });

        modelBuilder.Entity<TbMiembrosComite>(entity =>
        {
            entity.HasKey(e => e.MiembroId).HasName("PK__TbMiembr__E71F412FE0D42C06");

            entity.HasIndex(e => e.ComiteId, "IX_TbDatosMiembros_DcomiteId");

            entity.Property(e => e.ApellidoMiembro).HasMaxLength(200);
            entity.Property(e => e.CedulaMiembro)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NombreMiembro).HasMaxLength(200);

            entity.HasOne(d => d.Comite).WithMany(p => p.TbMiembrosComite)
                .HasForeignKey(d => d.ComiteId)
                .HasConstraintName("FK_MiembrosComite_Comite");
        });

        modelBuilder.Entity<TbProvincia>(entity =>
        {
            entity.HasKey(e => e.ProvinciaId).HasName("PK__TbProvin__F7CBC77754EDEA95");

            entity.Property(e => e.NombreProvincia).HasMaxLength(150);

            entity.HasOne(d => d.RegionSalud).WithMany(p => p.TbProvincia)
                .HasForeignKey(d => d.RegionSaludId)
                .HasConstraintName("FK_Provincia_RegionSalud");
        });

        modelBuilder.Entity<TbRegSecuencia>(entity =>
        {
            entity.HasKey(e => e.SecuenciaId).HasName("PK__TbRegSec__00D46D28975D7D0A");

            entity.Property(e => e.Activo).HasDefaultValue(true);
        });

        modelBuilder.Entity<TbRegionSalud>(entity =>
        {
            entity.HasKey(e => e.RegionSaludId).HasName("PK__TbRegion__2B018A5411F2B54A");

            entity.Property(e => e.NombreRegion).HasMaxLength(150);
        });

        modelBuilder.Entity<TbRepresentanteLegal>(entity =>
        {
            entity.HasKey(e => e.RepLegalId).HasName("PK__TbRepres__4E77DD11E71F73D9");

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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
