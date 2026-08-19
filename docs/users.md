# Sistema de Autorización por Permisos + JWT con Refresh Token (.NET Core)

## Objetivo

Implementar un sistema de autorización basado en **permisos** (no solo roles) sobre un proyecto .NET Core que ya usa **ASP.NET Identity** con Entity Framework. Los permisos se heredan por rol, pero un usuario puede tener permisos adicionales de forma individual. La autenticación usa **JWT de vida corta** + **refresh token** persistido en base de datos y rotado en cada uso.

No se deben crear tablas nuevas de `Permissions` ni `RolePermissions`. Los permisos se modelan como **Claims** dentro de las tablas nativas de Identity (`AspNetRoleClaims` y `AspNetUserClaims`), usando `ClaimType = "permission"`.

---

## Contexto del proyecto

- Ya existe autenticación con ASP.NET Identity (`ApplicationUser`, `IdentityRole`) y EF Core.
- Ya existe un `AppDbContext` (ajustar el nombre real si es distinto).
- Se busca que algunos endpoints solo respondan si el usuario tiene **token válido** Y **permiso específico** para esa acción.

---

## Alcance del trabajo

### 1. Catálogo de permisos en código

Crear una clase estática de constantes con todos los permisos del sistema, agrupados por módulo/entidad. Ejemplo de estructura (ajustar según los módulos reales del proyecto — revisar los controllers existentes para inferir los módulos: `Categories`, `SubCategories`, `Products`, `Extras`, `Sales`, `Users`, etc.):

```csharp
public static class Permissions
{
    public static class Products
    {
        public const string Create = "products.create";
        public const string Read = "products.read";
        public const string Update = "products.update";
        public const string Delete = "products.delete";
    }

    public static class Sales
    {
        public const string Create = "sales.create";
        public const string Read = "sales.read";
        public const string Cancel = "sales.cancel";
    }

    // Repetir el patrón para cada módulo/entidad del sistema
}
```

Agregar también un método `GetAll()` que devuelva (por reflexión) todos los valores de permisos definidos, para usarlo en el seed de datos.

### 2. Modelo de Refresh Token

Crear entidad `RefreshToken`:

```csharp
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}
```

- Agregar `DbSet<RefreshToken> RefreshTokens` al `AppDbContext`.
- Configurar relación con `ApplicationUser` (FK `UserId`).
- Crear la migración de EF Core correspondiente y aplicarla.

### 3. Seed de roles y permisos

Crear (o extender si ya existe) un seeder que se ejecute al iniciar la app (`Program.cs` o un `IHostedService`/método de startup) que:

1. Cree los roles base si no existen (ej. `Admin`, `Vendedor`, revisar si el proyecto ya tiene roles definidos y respetarlos).
2. Asigne claims de tipo `"permission"` a cada rol usando `RoleManager<IdentityRole>.AddClaimAsync`.
   - `Admin` → todos los permisos de `Permissions.GetAll()`.
   - Otros roles → subconjuntos razonables según el módulo (a definir según el dominio del proyecto; si no está claro, dejar comentarios `// TODO: ajustar permisos por rol` en vez de inventar reglas de negocio).
3. Ser idempotente: si el rol/claim ya existe, no duplicarlo.

### 4. Configuración de tiempos en `appsettings.json`

```json
"Jwt": {
  "Issuer": "...",
  "Audience": "...",
  "Key": "...",
  "AccessTokenMinutes": 15,
  "RefreshTokenDays": 7
}
```

Reutilizar la configuración de `Jwt:Issuer`, `Jwt:Audience` y `Jwt:Key` si ya existe en el proyecto; solo agregar las claves de tiempos si faltan.

### 5. `AuthService`

Crear (o extender el servicio de autenticación existente si ya hay uno) con:

- `LoginAsync(email, password)`:
  - Valida credenciales con `UserManager`.
  - Genera access token + refresh token (ver siguiente punto).
- `RefreshAsync(refreshTokenValue)`:
  - Busca el refresh token en BD.
  - Si no existe, ya expiró o ya fue revocado → `UnauthorizedAccessException`.
  - Si es válido: lo marca como revocado (`RevokedAt = UtcNow`), genera un nuevo par de tokens, guarda `ReplacedByToken` en el token viejo (rotación).
- `RevokeAsync(refreshTokenValue)`:
  - Marca el refresh token como revocado (para logout).
- `BuildClaimsAsync(user)` (privado):
  - Agrega claims base (`NameIdentifier`, `Name`).
  - Por cada rol del usuario: agrega `ClaimTypes.Role` + todos los claims `"permission"` de ese rol (vía `RoleManager.GetClaimsAsync`).
  - Agrega también los claims `"permission"` directos del usuario (`UserManager.GetClaimsAsync`), para soportar excepciones individuales.
  - Deduplica por `(Type, Value)`.
- Generación del access token: JWT firmado con `SymmetricSecurityKey`, expiración = `Jwt:AccessTokenMinutes`.
- Generación del refresh token: string aleatorio criptográficamente seguro (`RandomNumberGenerator.GetBytes(64)` → Base64), expiración = `Jwt:RefreshTokenDays`, persistido en `RefreshTokens`.

Registrar `AuthService` en el contenedor de DI (`Program.cs`), scoped.

### 6. `AuthController` (endpoints)

```
POST /api/auth/login    [AllowAnonymous]  -> { accessToken, refreshToken, expiresAt }
POST /api/auth/refresh  [AllowAnonymous]  -> { accessToken, refreshToken, expiresAt }
POST /api/auth/revoke   [Authorize]       -> 204 No Content
```

Si ya existe un `AuthController`, extenderlo en vez de crear uno nuevo; no duplicar el endpoint de login si ya existe, solo ajustarlo para que use el nuevo flujo de tokens.

### 7. Sistema de autorización por permisos (el "guard")

Implementar usando el sistema nativo de autorización de ASP.NET Core (`IAuthorizationHandler` + `IAuthorizationPolicyProvider`), **no** un middleware custom manual, para poder usar `[Authorize(Policy = "...")]` de forma declarativa sin registrar cada política una por una.

**`PermissionRequirement.cs`:**

```csharp
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}
```

**`PermissionHandler.cs`:**

```csharp
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var hasPermission = context.User.Claims
            .Any(c => c.Type == "permission" && c.Value == requirement.Permission);

        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

**`PermissionPolicyProvider.cs`:**

```csharp
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
        return Task.FromResult(policy);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        FallbackPolicyProvider.GetFallbackPolicyAsync();
}
```

**`HasPermissionAttribute.cs`** (azúcar sintáctica, opcional pero recomendado):

```csharp
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: permission) { }
}
```

**Registro en `Program.cs`:**

```csharp
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddAuthorization();
```

Verificar que ya exista configurado `AddAuthentication().AddJwtBearer(...)`; si no existe, agregarlo usando `Jwt:Issuer`, `Jwt:Audience` y `Jwt:Key`.

### 8. Aplicar los permisos a los controladores existentes

Revisar los controllers actuales del proyecto y agregar el atributo de permiso correspondiente a cada acción, sin quitar el `[Authorize]` general. Ejemplo:

```csharp
[Authorize]
[HasPermission(Permissions.Products.Create)]
[HttpPost]
public async Task<IActionResult> CreateProduct(ProductDto dto) { ... }
```

Repetir para cada endpoint CRUD relevante (Create/Read/Update/Delete) en los controllers de: Categories, SubCategories, Products, Extras, Sales (y cualquier otro módulo existente en el proyecto).

> Si algún endpoint debe quedar accesible para cualquier usuario autenticado sin permiso específico (ej. "ver mi propio perfil"), dejarlo solo con `[Authorize]`, sin `[HasPermission]`.

### 9. Migraciones

- Generar la migración de EF Core para la tabla `RefreshTokens`.
- Aplicarla contra la base de datos de desarrollo.
- No modificar las tablas nativas de Identity (`AspNetRoleClaims`, `AspNetUserClaims`) vía migración manual; estas ya existen por Identity y solo se les insertan datos vía el seeder.

### 10. Pruebas manuales a validar al terminar

- [ ] Login devuelve `accessToken` + `refreshToken`.
- [ ] El `accessToken` decodificado (jwt.io) contiene los claims `"permission"` correctos según el rol del usuario.
- [ ] Un endpoint con `[HasPermission]` responde `403 Forbidden` si el usuario no tiene el permiso, aunque el token sea válido.
- [ ] Un endpoint con `[HasPermission]` responde `401 Unauthorized` si no hay token o está expirado.
- [ ] `/api/auth/refresh` con un refresh token válido devuelve un nuevo par de tokens y revoca el anterior.
- [ ] Reutilizar un refresh token ya revocado devuelve error (no genera un nuevo token).
- [ ] `/api/auth/revoke` invalida el refresh token indicado (logout funcional).
- [ ] Un usuario con un claim de permiso individual (asignado directo, no por rol) también pasa la validación de ese permiso.

---

## Restricciones / cosas a NO hacer

- No crear tablas nuevas `Permissions` o `RolePermissions`; usar exclusivamente `AspNetRoleClaims` / `AspNetUserClaims`.
- No guardar el `accessToken` en base de datos (solo el `refreshToken`).
- No inventar reglas de negocio de qué permisos van en cada rol si no están claras; dejar `TODO` explícito para revisión humana.
- No romper los endpoints/controllers existentes; solo agregar el atributo de permiso encima de lo ya funcional.
- Mantener el código en el mismo estilo/convenciones que ya usa el proyecto (revisar controllers y services existentes antes de escribir código nuevo).

---

## Entregable esperado

- Migración de EF Core aplicada con la tabla `RefreshTokens`.
- Clase `Permissions` con el catálogo completo.
- Seeder de roles + permisos ejecutándose al iniciar la app.
- `AuthService` con login / refresh / revoke funcionando end-to-end.
- Sistema de políticas dinámicas (`PermissionRequirement`, `PermissionHandler`, `PermissionPolicyProvider`) registrado y funcionando.
- Controllers existentes actualizados con `[HasPermission(...)]` en los endpoints que lo requieran.
