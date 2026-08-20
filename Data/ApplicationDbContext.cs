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
