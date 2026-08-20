using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DulceAtardecer.Common.Exceptions;
using DulceAtardecer.Data;
using DulceAtardecer.Models;
using DulceAtardecer.Models.Dtos.Auth;
using DulceAtardecer.Models.Dtos.Usuario;
using DulceAtardecer.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DulceAtardecer.Repository;

public class UserRepository(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration) : IUserRepository
{
    private readonly string _secretKey = configuration["ApiSettings:SecretKey"]
        ?? throw new InvalidOperationException("ApiSettings:SecretKey no está configurado.");
    private readonly int _accessTokenMinutes = configuration.GetValue("ApiSettings:AccessTokenMinutes", 15);
    private readonly int _refreshTokenDays = configuration.GetValue("ApiSettings:RefreshTokenDays", 7);

    public async Task<bool> IsUniqueUserAsync(string username, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByNameAsync(username);
        return user is null;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByNameAsync(loginDto.Username);
        if (user is null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            throw new UnauthorizedAccessException("Usuario o contraseña incorrectos.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = registerDto.Username,
            Email = registerDto.Email,
            Nombre = registerDto.Nombre
        };

        IdentityResult result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = new Dictionary<string, string[]>
            {
                ["register"] = result.Errors.Select(e => e.Description).ToArray()
            };
            throw new ValidationException(errors);
        }

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        await userManager.AddToRoleAsync(user, "User");

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        RefreshToken? existing = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (existing is null || !existing.IsActive || existing.User is null)
        {
            throw new UnauthorizedAccessException("Refresh token inválido, expirado o revocado.");
        }

        AuthResponseDto response = await IssueTokensAsync(existing.User, cancellationToken);

        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByToken = response.RefreshToken;
        await db.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task RevokeAsync(string refreshToken, string userId, CancellationToken cancellationToken = default)
    {
        RefreshToken? existing = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (existing is null || existing.UserId != userId || !existing.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token inválido o no pertenece al usuario.");
        }

        existing.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IEnumerable<UsuarioDto> Items, int Total)> GetUsuariosAsync(
        int page, int limit, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        IQueryable<ApplicationUser> query = userManager.Users.OrderBy(u => u.UserName);

        int total = await query.CountAsync(cancellationToken);
        List<ApplicationUser> users = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var items = new List<UsuarioDto>(users.Count);
        foreach (ApplicationUser user in users)
        {
            items.Add(await BuildUsuarioDtoAsync(user));
        }

        return (items, total);
    }

    public async Task<UsuarioDto> GetUsuarioByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        ApplicationUser user = await userManager.FindByIdAsync(id)
            ?? throw new NotFoundException(nameof(ApplicationUser), id);
        return await BuildUsuarioDtoAsync(user);
    }

    public async Task<UsuarioDto> CreateUsuarioAsync(CreateUsuarioDto createDto, CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(createDto.Email) is not null)
        {
            throw new ConflictException("El email ya está en uso.");
        }

        var user = new ApplicationUser
        {
            UserName = createDto.Username,
            Email = createDto.Email,
            EmailConfirmed = true,
            Nombre = createDto.Nombre
        };

        IdentityResult result = await userManager.CreateAsync(user, createDto.Password);
        if (!result.Succeeded)
        {
            var errors = new Dictionary<string, string[]>
            {
                ["password"] = result.Errors.Select(e => e.Description).ToArray()
            };
            throw new ValidationException(errors);
        }

        await AssignRoleAsync(user, createDto.Role);

        return await BuildUsuarioDtoAsync(user);
    }

    public async Task UpdateUsuarioAsync(string id, UpdateUsuarioDto updateDto, CancellationToken cancellationToken = default)
    {
        ApplicationUser user = await userManager.FindByIdAsync(id)
            ?? throw new NotFoundException(nameof(ApplicationUser), id);

        ApplicationUser? existingWithEmail = await userManager.FindByEmailAsync(updateDto.Email);
        if (existingWithEmail is not null && existingWithEmail.Id != user.Id)
        {
            throw new ConflictException("El email ya está en uso por otro usuario.");
        }

        user.Nombre = updateDto.Nombre;
        user.Email = updateDto.Email;
        user.NormalizedEmail = updateDto.Email.ToUpperInvariant();

        user.LockoutEnabled = true;
        user.LockoutEnd = updateDto.IsActive ? null : DateTimeOffset.MaxValue;

        await AssignRoleAsync(user, updateDto.Role);

        IdentityResult result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = new Dictionary<string, string[]>
            {
                ["usuario"] = result.Errors.Select(e => e.Description).ToArray()
            };
            throw new ValidationException(errors);
        }
    }

    public async Task DeleteUsuarioAsync(string id, CancellationToken cancellationToken = default)
    {
        ApplicationUser user = await userManager.FindByIdAsync(id)
            ?? throw new NotFoundException(nameof(ApplicationUser), id);

        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        await userManager.UpdateAsync(user);
    }

    public async Task ResetPasswordAsync(string id, string newPassword, CancellationToken cancellationToken = default)
    {
        ApplicationUser user = await userManager.FindByIdAsync(id)
            ?? throw new NotFoundException(nameof(ApplicationUser), id);

        string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        IdentityResult result = await userManager.ResetPasswordAsync(user, resetToken, newPassword);
        if (!result.Succeeded)
        {
            var errors = new Dictionary<string, string[]>
            {
                ["password"] = result.Errors.Select(e => e.Description).ToArray()
            };
            throw new ValidationException(errors);
        }
    }

    private async Task AssignRoleAsync(ApplicationUser user, string role)
    {
        IList<string> currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        await userManager.AddToRoleAsync(user, role);
    }

    private async Task<UsuarioDto> BuildUsuarioDtoAsync(ApplicationUser user)
    {
        IList<string> roles = await userManager.GetRolesAsync(user);
        bool isActive = user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow;

        return new UsuarioDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.Nombre,
            roles,
            isActive);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        IList<string> roles = await userManager.GetRolesAsync(user);
        IList<Claim> claims = await BuildClaimsAsync(user, roles);

        DateTime expiresAt = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);
        string accessToken = GenerateAccessToken(claims, expiresAt);
        string refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id, cancellationToken);

        return new AuthResponseDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.Nombre,
            roles,
            accessToken,
            refreshToken,
            expiresAt);
    }

    private async Task<IList<Claim>> BuildClaimsAsync(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new("username", user.UserName ?? string.Empty)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        foreach (string roleName in roles)
        {
            IdentityRole? role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            IList<Claim> roleClaims = await roleManager.GetClaimsAsync(role);
            claims.AddRange(roleClaims.Where(c => c.Type == "permission"));
        }

        IList<Claim> userClaims = await userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims.Where(c => c.Type == "permission"));

        return claims
            .GroupBy(c => (c.Type, c.Value))
            .Select(g => g.First())
            .ToList();
    }

    private string GenerateAccessToken(IList<Claim> claims, DateTime expiresAt)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.UTF8.GetBytes(_secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<string> GenerateAndStoreRefreshTokenAsync(string userId, CancellationToken cancellationToken)
    {
        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        db.RefreshTokens.Add(new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenDays)
        });
        await db.SaveChangesAsync(cancellationToken);

        return token;
    }
}
