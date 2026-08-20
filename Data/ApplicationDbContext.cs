using DulceAtardecer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DulceAtardecer.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<SubCategoria> SubCategorias => Set<SubCategoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Extra> Extras => Set<Extra>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaItem> VentaItems => Set<VentaItem>();
    public DbSet<VentaItemExtra> VentaItemExtras => Set<VentaItemExtra>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Categoria>()
            .Property(c => c.Nombre)
            .HasMaxLength(100);

        builder.Entity<Categoria>()
            .Property(c => c.Descripcion)
            .HasMaxLength(255);

        builder.Entity<SubCategoria>()
            .Property(sc => sc.Nombre)
            .HasMaxLength(100);

        builder.Entity<SubCategoria>()
            .HasOne(sc => sc.Categoria)
            .WithMany(c => c.SubCategorias)
            .HasForeignKey(sc => sc.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Producto>()
            .Property(p => p.Nombre)
            .HasMaxLength(150);

        builder.Entity<Producto>()
            .Property(p => p.Descripcion)
            .HasMaxLength(255);

        builder.Entity<Producto>()
            .Property(p => p.Precio)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Producto>()
            .HasOne(p => p.SubCategoria)
            .WithMany()
            .HasForeignKey(p => p.SubCategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Extra>()
            .Property(e => e.Nombre)
            .HasMaxLength(100);

        builder.Entity<Extra>()
            .Property(e => e.Precio)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Venta>()
            .Property(v => v.Cliente)
            .HasMaxLength(100);

        builder.Entity<Venta>()
            .Property(v => v.Estado)
            .HasMaxLength(50);

        builder.Entity<Venta>()
            .Property(v => v.Total)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Venta>()
            .HasOne(v => v.Seller)
            .WithMany()
            .HasForeignKey(v => v.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Venta>()
            .HasOne(v => v.EstadoActualizadoPor)
            .WithMany()
            .HasForeignKey(v => v.EstadoActualizadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VentaItem>()
            .Property(vi => vi.Nombre)
            .HasMaxLength(150);

        builder.Entity<VentaItem>()
            .Property(vi => vi.Precio)
            .HasColumnType("decimal(18,2)");

        builder.Entity<VentaItem>()
            .Property(vi => vi.Subtotal)
            .HasColumnType("decimal(18,2)");

        builder.Entity<VentaItem>()
            .HasOne(vi => vi.Venta)
            .WithMany(v => v.Items)
            .HasForeignKey(vi => vi.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<VentaItem>()
            .HasOne(vi => vi.Producto)
            .WithMany()
            .HasForeignKey(vi => vi.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VentaItemExtra>()
            .HasKey(vie => new { vie.VentaItemId, vie.ExtraId });

        builder.Entity<VentaItemExtra>()
            .Property(vie => vie.Precio)
            .HasColumnType("decimal(18,2)");

        builder.Entity<VentaItemExtra>()
            .HasOne(vie => vie.VentaItem)
            .WithMany(vi => vi.Extras)
            .HasForeignKey(vie => vie.VentaItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<VentaItemExtra>()
            .HasOne(vie => vie.Extra)
            .WithMany()
            .HasForeignKey(vie => vie.ExtraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
