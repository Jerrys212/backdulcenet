using DulceAtardecer.Constants;
using DulceAtardecer.Models;
using Microsoft.AspNetCore.Identity;

namespace DulceAtardecer.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        SeedRolesAndUsers(context);
        await context.SaveChangesAsync(cancellationToken);

        SeedCategoriasYProductos(context);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static void SeedRolesAndUsers(ApplicationDbContext context)
    {
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new IdentityRole { Id = "role-admin", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "role-user", Name = "User", NormalizedName = "USER" });

            context.RoleClaims.AddRange(Permissions.GetAll()
                .Select(permission => new IdentityRoleClaim<string>
                {
                    RoleId = "role-admin",
                    ClaimType = "permission",
                    ClaimValue = permission
                }));

            // TODO: ajustar permisos por rol si se agregan módulos nuevos (por ahora User solo lee).
            context.RoleClaims.AddRange(
                new[] { Permissions.Categorias.Read, Permissions.Productos.Read }
                    .Select(permission => new IdentityRoleClaim<string>
                    {
                        RoleId = "role-user",
                        ClaimType = "permission",
                        ClaimValue = permission
                    }));
        }

        if (context.Users.Any())
        {
            return;
        }

        var hasher = new PasswordHasher<ApplicationUser>();

        var admin = new ApplicationUser
        {
            Id = "user-admin",
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@dulceatardecer.com",
            NormalizedEmail = "ADMIN@DULCEATARDECER.COM",
            EmailConfirmed = true,
            Nombre = "Administrador",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

        var estandar = new ApplicationUser
        {
            Id = "user-estandar",
            UserName = "usuario",
            NormalizedUserName = "USUARIO",
            Email = "usuario@dulceatardecer.com",
            NormalizedEmail = "USUARIO@DULCEATARDECER.COM",
            EmailConfirmed = true,
            Nombre = "Usuario de Prueba",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        estandar.PasswordHash = hasher.HashPassword(estandar, "Usuario123!");

        context.Users.AddRange(admin, estandar);
        context.UserRoles.AddRange(
            new IdentityUserRole<string> { UserId = admin.Id, RoleId = "role-admin" },
            new IdentityUserRole<string> { UserId = estandar.Id, RoleId = "role-user" });
    }

    private static void SeedCategoriasYProductos(ApplicationDbContext context)
    {
        if (context.Categorias.Any())
        {
            return;
        }

        var tortas = new Categoria { Nombre = "Tortas" };
        var cupcakes = new Categoria { Nombre = "Cupcakes" };
        var galletas = new Categoria { Nombre = "Galletas" };

        context.Categorias.AddRange(tortas, cupcakes, galletas);

        context.Productos.AddRange(
            new Producto
            {
                Nombre = "Torta de Chocolate",
                Descripcion = "Bizcocho de chocolate con ganache y frutos rojos",
                Precio = 18500m,
                ImgUrl = "https://placehold.co/400x400?text=Torta+Chocolate",
                ImgUrlLocal = string.Empty,
                Categoria = tortas
            },
            new Producto
            {
                Nombre = "Torta Red Velvet",
                Descripcion = "Bizcocho aterciopelado con frosting de queso crema",
                Precio = 19500m,
                ImgUrl = "https://placehold.co/400x400?text=Red+Velvet",
                ImgUrlLocal = string.Empty,
                Categoria = tortas
            },
            new Producto
            {
                Nombre = "Cupcake de Vainilla",
                Descripcion = "Cupcake de vainilla con buttercream",
                Precio = 2500m,
                ImgUrl = "https://placehold.co/400x400?text=Cupcake+Vainilla",
                ImgUrlLocal = string.Empty,
                Categoria = cupcakes
            },
            new Producto
            {
                Nombre = "Galletas de Avena",
                Descripcion = "Galletas artesanales de avena y pasas",
                Precio = 1800m,
                ImgUrl = "https://placehold.co/400x400?text=Galletas+Avena",
                ImgUrlLocal = string.Empty,
                Categoria = galletas
            });
    }
}
