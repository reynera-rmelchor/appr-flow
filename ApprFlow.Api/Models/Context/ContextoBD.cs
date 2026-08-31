using Microsoft.EntityFrameworkCore;

namespace ApprFlow.Api.Models.Context;

public partial class ContextoBD : DbContext
{
    public ContextoBD()
    {
    }

    public ContextoBD(DbContextOptions<ContextoBD> options)
        : base(options)
    {
    }

    public virtual DbSet<Flujo> Flujos { get; set; }

    public virtual DbSet<FlujoPaso> FlujoPasos { get; set; }

    public virtual DbSet<Plantilla> Plantillas { get; set; }

    public virtual DbSet<PlantillaPaso> PlantillaPasos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:TestDbConn");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Flujo>(entity =>
        {
            entity.ToTable("flujos");

            entity.HasIndex(e => new { e.UsuarioCreadorId, e.Estado }, "IX_flujos_creador_estado");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualizadoEn).HasColumnName("actualizado_en");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_flujos_creado_en")
                .HasColumnName("creado_en");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasDefaultValue((byte)1, "DF_flujos_estado")
                .HasColumnName("estado");
            entity.Property(e => e.PlantillaId).HasColumnName("plantilla_id");
            entity.Property(e => e.Titulo)
                .HasMaxLength(200)
                .HasColumnName("titulo");
            entity.Property(e => e.UsuarioCreadorId).HasColumnName("usuario_creador_id");

            entity.HasOne(d => d.Plantilla).WithMany(p => p.Flujos)
                .HasForeignKey(d => d.PlantillaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_flujos_plantilla");

            entity.HasOne(d => d.UsuarioCreador).WithMany(p => p.Flujos)
                .HasForeignKey(d => d.UsuarioCreadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_flujos_usuario_creador");
        });

        modelBuilder.Entity<FlujoPaso>(entity =>
        {
            entity.ToTable("flujo_pasos");

            entity.HasIndex(e => new { e.FlujoId, e.Orden }, "IX_flujo_pasos_flujo_orden");

            entity.HasIndex(e => new { e.UsuarioAsignadoId, e.Estado }, "IX_flujo_pasos_usuario_estado");

            entity.HasIndex(e => new { e.FlujoId, e.Orden }, "UQ_flujo_pasos_orden").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Estado)
                .HasDefaultValue((byte)1, "DF_flujo_pasos_estado")
                .HasColumnName("estado");
            entity.Property(e => e.FechaDecision).HasColumnName("fecha_decision");
            entity.Property(e => e.FlujoId).HasColumnName("flujo_id");
            entity.Property(e => e.Observacion).HasColumnName("observacion");
            entity.Property(e => e.Orden).HasColumnName("orden");
            entity.Property(e => e.UsuarioAsignadoId).HasColumnName("usuario_asignado_id");
            entity.Property(e => e.UsuarioDecisionId).HasColumnName("usuario_decision_id");

            entity.HasOne(d => d.Flujo).WithMany(p => p.FlujoPasos)
                .HasForeignKey(d => d.FlujoId)
                .HasConstraintName("FK_flujo_pasos_flujo");

            entity.HasOne(d => d.UsuarioAsignado).WithMany(p => p.FlujoPasoUsuarioAsignados)
                .HasForeignKey(d => d.UsuarioAsignadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_flujo_pasos_usuario_asignado");

            entity.HasOne(d => d.UsuarioDecision).WithMany(p => p.FlujoPasoUsuarioDecisions)
                .HasForeignKey(d => d.UsuarioDecisionId)
                .HasConstraintName("FK_flujo_pasos_usuario_decision");
        });

        modelBuilder.Entity<Plantilla>(entity =>
        {
            entity.ToTable("plantillas");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true, "DF_plantillas_activo")
                .HasColumnName("activo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_plantillas_creado_en")
                .HasColumnName("creado_en");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<PlantillaPaso>(entity =>
        {
            entity.ToTable("plantilla_pasos");

            entity.HasIndex(e => new { e.PlantillaId, e.Orden }, "UQ_plantilla_pasos_orden").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_plantilla_pasos_creado_en")
                .HasColumnName("creado_en");
            entity.Property(e => e.Orden).HasColumnName("orden");
            entity.Property(e => e.PlantillaId).HasColumnName("plantilla_id");
            entity.Property(e => e.UsuarioAprobadorId).HasColumnName("usuario_aprobador_id");

            entity.HasOne(d => d.Plantilla).WithMany(p => p.PlantillaPasos)
                .HasForeignKey(d => d.PlantillaId)
                .HasConstraintName("FK_plantilla_pasos_plantilla");

            entity.HasOne(d => d.UsuarioAprobador).WithMany(p => p.PlantillaPasos)
                .HasForeignKey(d => d.UsuarioAprobadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_plantilla_pasos_usuario");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Email, "UQ_usuarios_email").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true, "DF_usuarios_activo")
                .HasColumnName("activo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_usuarios_creado_en")
                .HasColumnName("creado_en");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Rol)
                .HasDefaultValue((byte)2, "DF_usuarios_rol")
                .HasColumnName("rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
