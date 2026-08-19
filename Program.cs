using System.Text;
using Asp.Versioning;
using DulceAtardecer.Authorization;
using DulceAtardecer.Constants;
using DulceAtardecer.Data;
using DulceAtardecer.Mapping;
using DulceAtardecer.Models;
using DulceAtardecer.Repository;
using DulceAtardecer.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
});
builder.Services.AddResponseCaching();

builder.Services.AddCors(options =>
{
    // TODO: reemplazar por los orígenes reales del frontend cuando exista.
    options.AddPolicy(PolicyNames.AllowSpecificOrigin, policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddOpenApi("v1");

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

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
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors(PolicyNames.AllowSpecificOrigin);

app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
