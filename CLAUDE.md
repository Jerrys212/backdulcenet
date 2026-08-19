# CLAUDE.md

Este archivo guía a Claude Code cuando trabaja en este repositorio. Léelo por completo antes de proponer cambios. Está pensado para que, con una instrucción breve ("creá el módulo de Órdenes"), Claude pueda generar un módulo completo replicando los patrones ya establecidos en el proyecto, sin tener que redescubrirlos cada vez.

## 1. Proyecto

- **Nombre**: `DulceAtardecer` (`DulceAtardecer.csproj`)
- **Tipo**: ASP.NET Core 10 Web API — backend de e-commerce de pastelería/repostería (Categorías, Productos, Usuarios/Auth)
- **Solución**: un único `.csproj` en la raíz
- **Tests**: no hay proyecto de tests todavía

## 2. Stack tecnológico

- **Runtime**: .NET 10 (LTS, soportado hasta 2028)
- **Lenguaje**: C# 14, `Nullable` habilitado
- **Framework web**: ASP.NET Core 10 con **Controllers** (`ControllerBase`), no Minimal APIs
- **ORM**: Entity Framework Core 10 + SQL Server (`ApplicationDbContext`)
- **Mapeo DTO ↔ Entidad**: **Mapster** (no AutoMapper), configuración centralizada
- **Auth**: ASP.NET Core Identity + JWT (HMAC-SHA256) de vida corta + **refresh token** persistido y rotable, más autorización por **permisos** vía Claims (ver sección 7)
- **Validación**: **FluentValidation**, disparada por un `IAsyncActionFilter` global (no MediatR — ver sección 10)
- **Respuestas**: envelope estandarizado (`{ success, data, meta }` / `{ success: false, error }`) vía filtro global + middleware de excepciones (ver secciones 8-9)
- **Versionado de API**: `Asp.Versioning.Mvc`, versionado por segmento de URL
- **Docs**: Swagger / OpenAPI 3.1 nativo (`AddOpenApi()`), con docs separados por versión si aplica
- **DB local**: SQL Server vía Docker Compose (`mcr.microsoft.com/mssql/server:2022-latest`)

> ✏️ Si el proyecto usa otra combinación (Dapper en vez de EF Core, etc.) ajustalo acá — esta sección es la fuente de verdad del stack.

## 3. Comandos esenciales

Requiere el SDK/runtime de .NET 10 (el proyecto apunta a `net10.0`; si solo tenés un SDK más nuevo instalado, necesitás igual los runtime packs 10.x de `Microsoft.AspNetCore.App`/`Microsoft.NETCore.App` o `dotnet run`/`dotnet ef` van a fallar).

```bash
docker compose up -d          # levanta SQL Server en localhost:1433
dotnet restore
dotnet ef database update     # aplica migraciones + corre el seeder (requiere `dotnet tool install --global dotnet-ef`)
dotnet run                    # o `dotnet watch run`; Swagger en /swagger
dotnet build
dotnet test                   # si existe proyecto de tests
```

Crear una migración después de cambiar el modelo:

```bash
dotnet ef migrations add <Nombre>
```

**Siempre ejecutá `dotnet build` (y `dotnet test` si existe) antes de dar una tarea por terminada.**

### 3.1 Docker Compose (si no existe)

Si el repo todavía no tiene `docker-compose.yml`, crearlo con un servicio de **SQL Server** para desarrollo local, siguiendo esta convención:

- El **nombre del servicio y del contenedor** (`container_name`) deben ser el **nombre de la carpeta raíz del proyecto** (no un genérico como `sqlserver` o `db`), para que sea identificable si hay varios proyectos corriendo Docker en la misma máquina.
- Imagen `mcr.microsoft.com/mssql/server:2022-latest`, puerto `1433:1433`, variables `ACCEPT_EULA=Y` y `MSSQL_SA_PASSWORD` (o `SA_PASSWORD` según la versión de imagen).
- La contraseña de `SA` **nunca hardcodeada en texto plano en el `docker-compose.yml` committeado**: usar un archivo `.env` (agregado a `.gitignore`) con `MSSQL_SA_PASSWORD=...` y referenciarlo con `${MSSQL_SA_PASSWORD}`.
- Agregar un volumen nombrado para persistir los datos entre reinicios (`<nombre-carpeta>_data:/var/opt/mssql`).

Ejemplo (reemplazando `<nombre-carpeta>` por el nombre real de la carpeta del proyecto):

```yaml
services:
  <nombre-carpeta>:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: <nombre-carpeta>
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD}
    ports:
      - "1433:1433"
    volumes:
      - <nombre-carpeta>_data:/var/opt/mssql

volumes:
  <nombre-carpeta>_data:
```

## 4. Arquitectura

**Capas**: `Controllers → Repository interfaces (Repository/IRepository/*) → EF Core (ApplicationDbContext)`.
Los repositorios se registran como `Scoped` en `Program.cs`. **Los controllers nunca tocan `DbContext` directamente** — siempre pasan por su interfaz de repositorio inyectada.

Los errores de negocio/validación **no se devuelven como `BadRequest()`/`NotFound()` manuales** desde el controller ni desde el repositorio: se lanzan como excepciones tipadas (sección 9) que suben por la pila y las resuelve el middleware global (sección 8). Esto mantiene los controllers y repositorios limpios de lógica de formateo de respuesta.

No se usa CQRS/MediatR salvo que el proyecto ya lo tenga adoptado — si se pide agregar esa capa, señalarlo explícitamente en la respuesta antes de introducirla, porque cambia el patrón para todo el proyecto. Por esta razón, la validación (sección 10) se resuelve con un `IAsyncActionFilter`, no con un pipeline behavior de MediatR.

## 5. Mapeo de DTOs (Mapster)

- Todas las conversiones tipo↔tipo se centralizan en `Mapping/MapsterConfig.cs`, dentro de un único método `RegisterMappings()` llamado una vez al arrancar desde `Program.cs`.
- Los controllers llaman `.Adapt<T>()` directamente sobre la entidad/DTO — no crear clases `Profile` por feature.
- Al agregar un módulo nuevo, sumar sus mapeos (`Entity ↔ CreateDto`, `Entity ↔ UpdateDto`, `Entity ↔ ReadDto`) en ese mismo archivo, no en archivos sueltos.

## 6. Versionado de API

- `Asp.Versioning` con versionado por segmento de URL: `api/v{version:apiVersion}/[controller]`.
- Controllers con lógica que cambia entre versiones viven en `Controllers/V1/` y `Controllers/V2/` (ej. un endpoint que en V2 agrega ordenamiento/paginación que en V1 no existía). Marcar la versión vieja con `[Obsolete]` cuando corresponda, no borrarla.
- Controllers cuya lógica es idéntica en todas las versiones se marcan `[ApiVersionNeutral]` y viven en `Controllers/` a secas — no dupliques un controller entre V1/V2 si no hay diferencia real de comportamiento.
- Swagger se configura con un doc separado por versión (`v1`, `v2`, ...).

## 7. Autenticación y autorización

### 7.1 Identidad y roles

- Modelo de usuario: `ApplicationUser : IdentityUser` (no crear un segundo modelo `User` en paralelo — si el dominio necesita datos extra de usuario, extender `ApplicationUser`, no duplicar la tabla).
- Login/registro vía `UserManager`/`RoleManager` de Identity; el repositorio de usuarios (`UserRepository`, implementando `IUserRepository`) es el único lugar que emite y rota tokens (access + refresh).
- Autorización a **nivel de controller** con `[Authorize(Roles = "Admin")]` (o el rol que corresponda) como primera línea de defensa, con `[AllowAnonymous]` explícito en las acciones puntuales que deben ser públicas (login, registro, refresh, lecturas públicas). No dejar autorización "por accidente" sin declarar.

### 7.2 JWT de vida corta + refresh token

- **Access token**: JWT HMAC-SHA256, expiración corta configurable (`ApiSettings:AccessTokenMinutes`, default `15`). Claims mínimos: `id` (`ClaimTypes.NameIdentifier`), `username`, rol(es) (`ClaimTypes.Role`), y los claims de tipo `"permission"` del usuario (ver 7.3). No se persiste en BD.
- **Refresh token**: string aleatorio (`RandomNumberGenerator.GetBytes(64)` → Base64), expiración larga configurable (`ApiSettings:RefreshTokenDays`, default `7`), **sí se persiste** en la entidad `RefreshToken` (`Models/RefreshToken.cs`, mapeada por EF Core normal — sigue el flujo de la sección 13, no las tablas nativas de Identity).

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

- Endpoints en `AuthController` (`[ApiVersionNeutral]`, no cambia entre versiones):
  ```
  POST /api/auth/login    [AllowAnonymous]  -> { accessToken, refreshToken, expiresAt } (envuelto en el ApiResponse de la sección 8)
  POST /api/auth/refresh  [AllowAnonymous]  -> { accessToken, refreshToken, expiresAt }
  POST /api/auth/revoke   [Authorize]       -> 204 No Content
  ```
- **Rotación**: cada `refresh` marca el token usado como revocado (`RevokedAt`) y guarda `ReplacedByToken`, emitiendo un par nuevo. Un refresh token ya revocado o expirado lanza `UnauthorizedAccessException` (401, no pasa por la jerarquía de `AppException` porque es un caso de autenticación, no de negocio).
- Esta lógica vive en `UserRepository` (o un método dedicado ahí), no en un servicio nuevo — respeta el punto 7.1 de que el repositorio de usuarios es el único emisor de tokens.

### 7.3 Autorización por permisos (Claims)

Para endpoints que necesitan control más fino que un rol (ej. "solo quien puede cancelar ventas, aunque sea Admin"), se usa un segundo nivel de autorización basado en **Claims de tipo `"permission"`**, apoyado en las tablas nativas de Identity — **no se crean tablas `Permissions`/`RolePermissions` nuevas**:

- `AspNetRoleClaims` → permisos heredados por rol.
- `AspNetUserClaims` → permisos individuales de excepción (además de los del rol).

**Catálogo de permisos** en código (`Constants/Permissions.cs`), agrupado por módulo:

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
    // un bloque por módulo/entidad del dominio
}
```

**Seed de permisos por rol**: se hace dentro del mismo `DataSeeder.cs` (sección 11), usando `RoleManager.AddClaimAsync(role, new Claim("permission", Permissions.X.Y))`. Idempotente — no duplicar claims si ya existen.

**El "guard"**: políticas dinámicas nativas de ASP.NET Core (`Common/Authorization/`), para no tener que registrar una `AddPolicy` por cada permiso:

```csharp
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Claims.Any(c => c.Type == "permission" && c.Value == requirement.Permission))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) =>
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName) =>
        Task.FromResult(new AuthorizationPolicyBuilder().AddRequirements(new PermissionRequirement(policyName)).Build());

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();
}
```

Registro en `Program.cs`:

```csharp
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
```

**Atributo de uso** (`Common/Authorization/HasPermissionAttribute.cs`):

```csharp
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: permission) { }
}
```

**Cuándo usar `[Authorize(Roles = "...")]` vs `[HasPermission(...)]`**:

- `Roles` sigue siendo la primera línea, a nivel de controller (patrón ya establecido en 7.1) — se mantiene para todo controller nuevo.
- `[HasPermission(Permissions.X.Y)]` se agrega **a nivel de acción**, encima del `[Authorize]` del controller, únicamente en endpoints donde el rol solo no basta para decidir el acceso (ej. dentro de un mismo rol, solo ciertos usuarios pueden ejecutar esa acción puntual). No agregarlo "por si acaso" a todos los endpoints — si el rol del controller ya es suficiente, no sumar ruido.

**Secretos**: `ApiSettings:SecretKey`, `ApiSettings:AccessTokenMinutes`, `ApiSettings:RefreshTokenDays`, la cadena de conexión y cualquier credencial van por `dotnet user-secrets` en local y variables de entorno/Key Vault en el resto de entornos — **nunca committeados en texto plano en `appsettings.json`**, ni siquiera para desarrollo rápido.

## 8. Respuesta estandarizada (response envelope)

Todas las respuestas comparten la misma forma validada, para simplificar el consumo desde el front:

```json
// éxito
{ "success": true, "data": { }, "meta": { "page": 1, "total": 50 } } // meta es opcional
// error (armado por el middleware global de excepciones)
{ "success": false, "error": { "code": "string", "message": "string /* en español */" } }
```

- `ApiResponse<T>` (`Common/Responses/ApiResponse.cs`): `record ApiResponse<T>(bool Success, T? Data, ApiMeta? Meta = null)`, `record ApiMeta(int Page, int Total)`.
- **Éxito**: se envuelve automáticamente vía un `IAsyncResultFilter` global (`Common/Filters/ApiResponseWrapperFilter.cs`), registrado en `Program.cs` con `options.Filters.Add<ApiResponseWrapperFilter>()`. Los controllers devuelven la entidad/DTO tal cual (`return Ok(productDto)`); **no armar el envelope a mano en cada acción**.
- **Error**: lo arma el middleware de excepciones (sección 9), nunca un controller ni un repositorio.
- Los controllers **no devuelven `BadRequest()`/`NotFound()`/`Conflict()` manuales** — ver sección 9.

## 9. Excepciones custom

Todos los errores de negocio y validación se lanzan como excepciones tipadas y las resuelve el middleware global de excepciones (`Common/Middleware/ExceptionHandlingMiddleware.cs`), que arma la respuesta de error estandarizada de la sección 8.

Jerarquía base en `Common/Exceptions/`:

```csharp
public abstract class AppException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }
    protected AppException(string code, string message, int statusCode) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
```

| Excepción                              | Status | Code               | Uso                                                                                           |
| -------------------------------------- | ------ | ------------------ | --------------------------------------------------------------------------------------------- |
| `NotFoundException(entity, key)`       | 404    | `NOT_FOUND`        | entidad no encontrada por id                                                                  |
| `ValidationException(failures)`        | 400    | `VALIDATION_ERROR` | la lanza el filtro de FluentValidation (sección 10), no manualmente                           |
| `ConflictException(message)`           | 409    | `CONFLICT`         | ej. duplicados, restricción única, FK en uso (no se puede borrar un producto con ventas)      |
| `ForbiddenException(message)`          | 403    | `FORBIDDEN`        | regla de negocio prohibida, distinta del 401/403 que ya resuelve el guard de auth (sección 7) |
| `BusinessRuleException(code, message)` | 422    | el `code` recibido | reglas de dominio puntuales (ej. `INSUFFICIENT_STOCK`) que no encajan en las anteriores       |

`ValidationException` expone además `IDictionary<string, string[]> Errors` — el middleware lo agrega como `error.details` en la respuesta para que el front pinte los errores por campo.

Uso típico en repositorio/lógica de negocio:

```csharp
public async Task<Product> GetByIdAsync(int id, CancellationToken ct)
{
    var product = await _context.Products.FindAsync([id], ct);
    if (product is null)
        throw new NotFoundException(nameof(Product), id);
    return product;
}
```

**Regla**: si estás por escribir `return BadRequest(...)`, `return NotFound(...)` o un `if (...) return StatusCode(409, ...)` en un controller, es señal de que corresponde lanzar una `AppException` en su lugar y dejar que el middleware la traduzca.

## 10. Validación con FluentValidation

- Un `AbstractValidator<TDto>` por cada DTO de entrada, ubicado junto al DTO (`Models/Dtos/CreateProductDto.cs` + `Models/Dtos/CreateProductDtoValidator.cs`), no en una carpeta `Validators/` separada — mantiene el módulo autocontenido igual que el resto de los patrones del repo.
- Registro automático de todos los validadores: `builder.Services.AddValidatorsFromAssemblyContaining<Program>();` en `Program.cs` — **no registrar validadores uno por uno**.
- Como el proyecto no usa MediatR (sección 4), la validación se dispara con un `IAsyncActionFilter` global (`Common/Filters/ValidationFilter.cs`), no con un pipeline behavior:

  ```csharp
  public class ValidationFilter : IAsyncActionFilter
  {
      public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
      {
          foreach (var arg in context.ActionArguments.Values)
          {
              if (arg is null) continue;

              var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
              if (context.HttpContext.RequestServices.GetService(validatorType) is IValidator validator)
              {
                  var result = await validator.ValidateAsync(new ValidationContext<object>(arg));
                  if (!result.IsValid)
                      throw new ValidationException(result.Errors);
              }
          }

          await next();
      }
  }
  ```

- Registro en `Program.cs`, junto al `ApiResponseWrapperFilter` de la sección 8:
  ```csharp
  builder.Services.AddControllers(options =>
  {
      options.Filters.Add<ApiResponseWrapperFilter>();
      options.Filters.Add<ValidationFilter>();
  });
  ```
- Mensajes de validación **siempre en español** (`.WithMessage("El nombre es obligatorio.")`), consistente con `error.message` de la sección 8.
- No usar Data Annotations (`[Required]`, `[MaxLength]`, etc.) en los DTOs para reglas de negocio — solo FluentValidation. Data Annotations puede quedar para metadata que consume Swagger si hace falta, pero no duplicar la regla en los dos lados.

## 11. Seeding

- `Data/DataSeeder.cs` se conecta vía el hook `UseSeeding` de EF Core, cableado en `Program.cs` — corre con `dotnet ef database update` (y en paths tipo `EnsureCreated()`), **no en cada arranque de la app**.
- Responsabilidad típica del seeder: roles base (`Admin`, `User`), sus claims de permiso (sección 7.3), un usuario admin y uno estándar de prueba, y un puñado de datos de ejemplo por entidad principal.
- Al agregar un módulo nuevo, **preguntar antes de agregarle datos de seed** — no todos los módulos necesitan datos de ejemplo, y el seeder no debe crecer sin control. Sí corresponde sumar sus permisos al catálogo y al seed de roles sin preguntar, porque es parte de dejar el módulo funcional.

## 12. Manejo de archivos (uploads)

Patrón para entidades que aceptan una imagen u otro archivo:

- El DTO de creación/edición se bindea con `[FromForm]` (multipart) y expone un `IFormFile?` opcional.
- El controller guarda el archivo en `wwwroot/<Entidad>Images/<id><guid><ext>` y persiste dos campos en la entidad: la URL pública (`ImgUrl`) y la ruta local en disco (`ImgUrlLocal`).
- Si no se sube archivo, `ImgUrl` cae a una URL de placeholder (ej. `placehold.co`) — nunca dejar el campo nulo o vacío en la respuesta.

## 13. Caching y CORS

- Perfiles de cache con nombre en `Constants/CacheProfiles.cs` (ej. `Default10`, `Default20`, en segundos), aplicados con `[ResponseCache(CacheProfileName = "...")]` en endpoints de lectura que lo justifiquen (listados poco volátiles).
- Política de CORS nombrada en `Constants/PolicyNames.cs` (ej. `AllowSpecificOrigin`). **Para proyectos nuevos, configurar orígenes explícitos desde el inicio** (no `WithOrigins("*")` + cualquier método/header) — el wildcard solo se acepta temporalmente en desarrollo local y debe quedar señalado como deuda técnica si aparece.

## 14. Convenciones de código C#

- `Nullable` habilitado; evitar `!` (null-forgiving) salvo casos justificados y comentados.
- `file-scoped namespaces` y `primary constructors` para servicios/repositorios con inyección de dependencias.
- `records` para DTOs inmutables; `class` para entidades con identidad y ciclo de vida.
- Async/await en todo el pipeline de I/O, sufijo `Async` obligatorio, nunca `.Result`/`.Wait()`.
- `CancellationToken` propagado desde la acción del controller hasta las llamadas a EF Core.
- Nombrado: `PascalCase` para tipos/miembros públicos, `_camelCase` para campos privados, `camelCase` para locales/parámetros.
- No usar `var` cuando el tipo no es evidente por el lado derecho.

## 15. Testing

> ✏️ Ajustar según el estado real del proyecto.

- Si no hay proyecto de tests todavía y se agrega un módulo con lógica no trivial, proponer (no imponer) crear `/tests/<Proyecto>.UnitTests` con xUnit + FluentAssertions antes de escalar el proyecto.
- Si ya existe: unit tests para repositorios/lógica de negocio con mocks, integration tests contra SQL Server real (Testcontainers) para flujos completos de un endpoint.

## 16. Checklist: crear un módulo nuevo

Cuando se pida "creá el módulo de `<Entidad>`", el flujo esperado es:

1. **Modelo**: `Models/<Entidad>.cs` (entidad EF Core) + configuración Fluent API si hace falta (índices, tipos de columna, ej. `decimal(18,2)` para dinero).
2. **DTOs**: `Create<Entidad>Dto`, `Update<Entidad>Dto`, `<Entidad>Dto` (lectura) en `Models/Dtos/`.
3. **Validadores**: `Create<Entidad>DtoValidator`, `Update<Entidad>DtoValidator` junto a cada DTO (sección 10). No dejar un DTO de entrada sin validador.
4. **Mapeos**: agregar las conversiones correspondientes en `Mapping/MapsterConfig.cs`.
5. **Repositorio**: interfaz `I<Entidad>Repository` en `Repository/IRepository/` + implementación en `Repository/`, registrada `Scoped` en `Program.cs`. Los errores de "no encontrado"/"conflicto" se lanzan como `AppException` (sección 9), no se devuelven como `null`/`bool`.
6. **Migración**: `dotnet ef migrations add Add<Entidad>` y revisar el `.cs` generado antes de aplicarlo.
7. **Permisos**: sumar el módulo al catálogo `Constants/Permissions.cs` (`Create`, `Read`, `Update`, `Delete` u otras acciones propias) y decidir con el usuario si algún endpoint puntual necesita `[HasPermission(...)]` además del `[Authorize(Roles = "...")]` de controller (sección 7.3).
8. **Controller**: decidir si es `[ApiVersionNeutral]` o si necesita `V1`/`V2` según si ya se prevén cambios de contrato; aplicar `[Authorize(Roles = "...")]` a nivel controller con `[AllowAnonymous]` en las acciones públicas que correspondan. Las acciones devuelven la entidad/DTO tal cual — el envelope (sección 8) y los errores (sección 9) los resuelven los filtros globales, no el controller.
9. **Cache/CORS**: aplicar `[ResponseCache]` en los `GET` que lo justifiquen, usando los perfiles existentes.
10. **Uploads**: si la entidad maneja imágenes/archivos, seguir el patrón de la sección 12.
11. **Seed**: sumar los permisos del módulo al seed de roles sin preguntar; preguntar antes de sumar datos de ejemplo de la entidad al `DataSeeder` (sección 11).
12. **Build/test**: correr `dotnet build` (y `dotnet test` si aplica) y confirmar que Swagger documenta bien el nuevo controller (`[ProducesResponseType]` en cada acción, incluyendo el shape del `ApiResponse<T>`).

## 17. Qué NO debe hacer Claude

- No committear secretos (connection strings, JWT keys, tiempos de expiración sensibles) en `appsettings.json` — siempre `user-secrets`/variables de entorno.
- No aplicar migraciones automáticamente contra una base de datos que no sea la local de desarrollo.
- No crear un segundo modelo de usuario/entidad en paralelo al existente "para no tocar lo viejo" — si hay que migrar algo, migrarlo, no duplicarlo.
- No crear tablas `Permissions`/`RolePermissions` nuevas — los permisos van como Claims sobre las tablas nativas de Identity (sección 7.3).
- No introducir un patrón arquitectónico nuevo (CQRS, MediatR, Clean Architecture en capas separadas) sin señalarlo explícitamente y confirmar que es lo que se quiere.
- No devolver `BadRequest()`/`NotFound()`/envelopes armados a mano desde un controller o repositorio — lanzar la `AppException` correspondiente (sección 9) y dejar que los filtros globales (secciones 8-10) resuelvan la forma de la respuesta.
- No validar con Data Annotations ni con `if`s manuales en el controller — solo FluentValidation (sección 10).
- No dejar CORS en wildcard ni autorización sin declarar "por velocidad" en código que no es explícitamente un prototipo descartable.
- No reescribir archivos completos cuando alcanza con un diff quirúrgico.

---

> 💡 **Cómo adaptar esta plantilla a un proyecto nuevo**: reemplazá los placeholders de la sección 1, borrá/ajustá las secciones 5-13 según las librerías reales que uses (si no usás Mapster, versionado, permisos por Claims, o uploads, sacá esas secciones), y completá la sección 16 con el nombre real de tus entidades de ejemplo. Una vez pegado en el repo nuevo, con algo como "creá el módulo de Órdenes" alcanza para que se repliquen todos los patrones documentados acá.
