using DssMultiEmpresa.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DssMultiEmpresa.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<CategoriaContable> CategoriasContables => Set<CategoriaContable>();
    public DbSet<CuentaContable> CuentasContables => Set<CuentaContable>();
    public DbSet<Tercero> Terceros => Set<Tercero>();
    public DbSet<CentroCosto> CentrosCosto => Set<CentroCosto>();
    public DbSet<TransaccionContable> TransaccionesContables => Set<TransaccionContable>();
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<IndicadorDss> IndicadoresDss => Set<IndicadorDss>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Usuario ──
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
        });

        // ── Empresa ──
        modelBuilder.Entity<Empresa>(e =>
        {
            e.HasOne(emp => emp.Usuario)
             .WithMany(u => u.Empresas)
             .HasForeignKey(emp => emp.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── CategoriaContable ──
        modelBuilder.Entity<CategoriaContable>(e =>
        {
            e.HasOne(c => c.Empresa)
             .WithMany(emp => emp.Categorias)
             .HasForeignKey(c => c.EmpresaId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── CuentaContable (PUC) ──
        modelBuilder.Entity<CuentaContable>(e =>
        {
            e.HasOne(cc => cc.Empresa)
             .WithMany(emp => emp.CuentasContables)
             .HasForeignKey(cc => cc.EmpresaId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(cc => cc.Categoria)
             .WithMany()
             .HasForeignKey(cc => cc.CategoriaId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(cc => new { cc.EmpresaId, cc.Codigo }).IsUnique();
            e.HasIndex(cc => new { cc.EmpresaId, cc.Clase });
        });

        // ── Tercero ──
        modelBuilder.Entity<Tercero>(e =>
        {
            e.HasOne(t => t.Empresa)
             .WithMany(emp => emp.Terceros)
             .HasForeignKey(t => t.EmpresaId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(t => new { t.EmpresaId, t.NumeroDocumento }).IsUnique();
        });

        // ── CentroCosto ──
        modelBuilder.Entity<CentroCosto>(e =>
        {
            e.HasOne(cc => cc.Empresa)
             .WithMany(emp => emp.CentrosCosto)
             .HasForeignKey(cc => cc.EmpresaId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(cc => new { cc.EmpresaId, cc.Codigo }).IsUnique();
        });

        // ── TransaccionContable ──
        modelBuilder.Entity<TransaccionContable>(e =>
        {
            e.HasOne(t => t.Empresa)
             .WithMany(emp => emp.Transacciones)
             .HasForeignKey(t => t.EmpresaId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(t => t.Categoria)
             .WithMany(c => c.Transacciones)
             .HasForeignKey(t => t.CategoriaId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.CuentaContable)
             .WithMany(cc => cc.Transacciones)
             .HasForeignKey(t => t.CuentaContableId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.Tercero)
             .WithMany(ter => ter.Transacciones)
             .HasForeignKey(t => t.TerceroId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.CentroCosto)
             .WithMany(cc => cc.Transacciones)
             .HasForeignKey(t => t.CentroCostoId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(t => new { t.EmpresaId, t.Fecha });
        });

        // ── Presupuesto ──
        modelBuilder.Entity<Presupuesto>(e =>
        {
            e.HasOne(p => p.Empresa)
             .WithMany(emp => emp.Presupuestos)
             .HasForeignKey(p => p.EmpresaId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.Categoria)
             .WithMany(c => c.Presupuestos)
             .HasForeignKey(p => p.CategoriaId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.CuentaContable)
             .WithMany(cc => cc.Presupuestos)
             .HasForeignKey(p => p.CuentaContableId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(p => new { p.EmpresaId, p.PeriodoAnio, p.PeriodoMes, p.CategoriaId })
             .IsUnique();
        });

        // ── IndicadorDss ──
        modelBuilder.Entity<IndicadorDss>(e =>
        {
            e.HasOne(i => i.Empresa)
             .WithMany(emp => emp.Indicadores)
             .HasForeignKey(i => i.EmpresaId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
