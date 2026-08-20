using System.Text;
using Asp.Versioning;
using DulceAtardecer.Common.Authorization;
using DulceAtardecer.Common.Filters;
using DulceAtardecer.Common.Middleware;
using DulceAtardecer.Constants;
using DulceAtardecer.Data;
using DulceAtardecer.Mapping;
using DulceAtardecer.Models;
using DulceAtardecer.Repository;
using DulceAtardecer.Repository.IRepository;
using DulceAtardecer.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
string secretKey = builder.Configuration["ApiSettings:SecretKey"]
    ?? throw new InvalidOperationException("ApiSettings:SecretKey no está configurado.");

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseSeeding((context, _) =>
    {
        DataSeeder.SeedAsync((ApplicationDbContext)context).GetAwaiter().GetResult();
    });
    options.UseAsyncSeeding(async (context, _, cancellationToken) =>
    {
        await DataSeeder.SeedAsync((ApplicationDbContext)context, cancellationToken);
    });
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddAuthorization();

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddControllers(options =>
{
    // Mantiene el sufijo "Async" en los nombres de acción para que nameof(...Async)
    // usado en CreatedAtAction siga resolviendo la ruta correctamente.
    options.SuppressAsyncSuffixInActionNames = false;
    options.CacheProfiles.Add(CacheProfiles.Default10, new Microsoft.AspNetCore.Mvc.CacheProfile { Duration = 10 });
    options.CacheProfiles.Add(CacheProfiles.Default20, new Microsoft.AspNetCore.Mvc.CacheProfile { Duration = 20 });
    options.Filters.Add<ApiResponseWrapperFilter>();
    options.Filters.Add<ValidationFilter>();
})
    // El 400 automático de [ApiController] se apaga para que toda la validación
    // (y su forma de respuesta) pase siempre por FluentValidation + ExceptionHandlingMiddleware.
    .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddResponseCaching();

builder.Services.AddCors(options =>
{
    options.AddPolicy(PolicyNames.AllowSpecificOrigin, policy =>
        policy.WithOrigins("http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddOpenApi("v1");

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ISubCategoriaRepository, SubCategoriaRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IExtraRepository, ExtraRepository>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();
builder.Services.AddScoped<IReportesService, ReportesService>();

MapsterConfig.RegisterMappings();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "DulceAtardecer v1");
        options.RoutePrefix = "swagger";
    });

    // ASP.NET Core solo loguea "Now listening on: ..."; esto imprime el link directo a Swagger
    // una vez que Kestrel ya resolvió las URLs reales (launchSettings, --urls, ASPNETCORE_URLS, etc.).
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        IServerAddressesFeature? addressesFeature = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>();

        foreach (string address in addressesFeature?.Addresses ?? [])
        {
            app.Logger.LogInformation("Swagger UI: {Url}/swagger", address);
        }
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors(PolicyNames.AllowSpecificOrigin);

app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
