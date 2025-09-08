using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NextDocApi.Modelos;

namespace NextDocApi.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Clasificacion> Clasificacions { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Documento> Documentos { get; set; }

    public virtual DbSet<EstadosDocumento> EstadosDocumentos { get; set; }

    public virtual DbSet<HistorialDocumento> HistorialDocumentos { get; set; }

    public virtual DbSet<PermisosAcceso> PermisosAccesos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TiposDocumento> TiposDocumentos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Clasificacion>(entity =>
        {
            entity.HasKey(e => e.IdClasificacion).HasName("PRIMARY");

            entity.ToTable("Clasificacion");

            entity.Property(e => e.Estado).HasDefaultValueSql("'1'");
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.IdDepartamento).HasName("PRIMARY");

            entity.Property(e => e.Estado).HasDefaultValueSql("'1'");
            entity.Property(e => e.NombreDepartamento).HasMaxLength(100);
        });

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.HasKey(e => e.IdDocumento).HasName("PRIMARY");

            entity.HasIndex(e => e.IdClasificacion, "IdClasificacion");

            entity.HasIndex(e => e.IdDepartamento, "IdDepartamento");

            entity.HasIndex(e => e.IdEstado, "IdEstado");

            entity.HasIndex(e => e.IdTipoDocumento, "IdTipoDocumento");

            entity.HasIndex(e => e.IdUsuarioAsignado, "IdUsuarioAsignado");

            entity.HasIndex(e => e.IdUsuarioCreador, "IdUsuarioCreador");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.Estado).HasDefaultValueSql("'1'");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.RutaArchivo).HasMaxLength(255);
            entity.Property(e => e.Titulo).HasMaxLength(200);

            entity.HasOne(d => d.IdClasificacionNavigation).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdClasificacion)
                .HasConstraintName("Documentos_ibfk_2");

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdDepartamento)
                .HasConstraintName("Documentos_ibfk_6");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdEstado)
                .HasConstraintName("Documentos_ibfk_3");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdTipoDocumento)
                .HasConstraintName("Documentos_ibfk_1");

            entity.HasOne(d => d.IdUsuarioAsignadoNavigation).WithMany(p => p.DocumentoIdUsuarioAsignadoNavigations)
                .HasForeignKey(d => d.IdUsuarioAsignado)
                .HasConstraintName("Documentos_ibfk_5");

            entity.HasOne(d => d.IdUsuarioCreadorNavigation).WithMany(p => p.DocumentoIdUsuarioCreadorNavigations)
                .HasForeignKey(d => d.IdUsuarioCreador)
                .HasConstraintName("Documentos_ibfk_4");
        });

        modelBuilder.Entity<EstadosDocumento>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PRIMARY");

            entity.ToTable("EstadosDocumento");

            entity.Property(e => e.Estado).HasDefaultValueSql("'1'");
            entity.Property(e => e.NombreEstado).HasMaxLength(50);
        });

        modelBuilder.Entity<HistorialDocumento>(entity =>
        {
            entity.HasKey(e => e.IdHistorial).HasName("PRIMARY");

            entity.HasIndex(e => e.IdDocumento, "IdDocumento");

            entity.HasIndex(e => e.IdUsuario, "IdUsuario");

            entity.Property(e => e.Accion).HasMaxLength(100);
            entity.Property(e => e.Comentarios).HasColumnType("text");
            entity.Property(e => e.Estado).HasDefaultValueSql("'1'");
            entity.Property(e => e.FechaAccion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.IdDocumentoNavigation).WithMany(p => p.HistorialDocumentos)
                .HasForeignKey(d => d.IdDocumento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("HistorialDocumentos_ibfk_1");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.HistorialDocumentos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("HistorialDocumentos_ibfk_2");
        });

        modelBuilder.Entity<PermisosAcceso>(entity =>
        {
            entity.HasKey(e => e.IdPermiso).HasName("PRIMARY");

            entity.ToTable("PermisosAcceso");

            entity.HasIndex(e => e.IdUsuario, "IdUsuario");

            entity.Property(e => e.PuedeEditar).HasDefaultValueSql("'0'");
            entity.Property(e => e.PuedeVer).HasDefaultValueSql("'1'");
            entity.Property(e => e.TagPantallaId)
                .HasMaxLength(100)
                .HasColumnName("TagPantallaID");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.PermisosAccesos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("PermisosAcceso_ibfk_1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PRIMARY");

            entity.Property(e => e.Estado).HasDefaultValueSql("'1'");
            entity.Property(e => e.NombreRol).HasMaxLength(50);
        });

        modelBuilder.Entity<TiposDocumento>(entity =>
        {
            entity.HasKey(e => e.IdTipoDocumento).HasName("PRIMARY");

            entity.ToTable("TiposDocumento");

            entity.Property(e => e.Estado).HasDefaultValueSql("'1'");
            entity.Property(e => e.NombreTipo).HasMaxLength(100);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.IdDepartamento, "IdDepartamento");

            entity.HasIndex(e => e.IdRol, "IdRol");

            entity.Property(e => e.Apellidos).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Estado).HasDefaultValueSql("'1'");
            entity.Property(e => e.Nombres).HasMaxLength(100);
            entity.Property(e => e.NroWhatsapp).HasMaxLength(20);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdDepartamento)
                .HasConstraintName("Usuarios_ibfk_2");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Usuarios_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
