using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

using FluentValidation;

using Inventario.Application.Common;
using Inventario.Application.DTOs.Requests;
using Inventario.Application.Interfaces.Queries;
using Inventario.Application.Interfaces.Repositories;
using Inventario.Application.Interfaces.Security;
using Inventario.Application.Services;

using Inventario.Application.Validators.Auth;
using Inventario.Application.Validators.Inventario;
using Inventario.Application.Validators.Movimientos;
using Inventario.Application.Validators.Productos;

using Inventario.Infrastructure.Persistence;
using Inventario.Infrastructure.Repositories;
using Inventario.Infrastructure.Security;

using Inventario.Presentation.Errors;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// 1. CONFIGURACIÓN
// ======================================================

// Cadena de conexión de PostgreSQL.
var connectionString = builder.Configuration
    .GetConnectionString("Inventario")
    ?? throw new InvalidOperationException(
        "Falta ConnectionStrings:Inventario");

// Configuración JWT.
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException(
        "Falta Jwt:Secret");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Falta Jwt:Issuer");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Falta Jwt:Audience");

var accessMinutes = builder.Configuration.GetValue(
    "Jwt:AccessMinutes",
    15);

if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Secret debe tener al menos 32 bytes.");
}

if (accessMinutes <= 0)
{
    throw new InvalidOperationException(
        "Jwt:AccessMinutes debe ser positivo.");
}

// ======================================================
// 2. CONTROLADORES Y SWAGGER
// ======================================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description =
                "Introduce el access token obtenido en /api/auth/login"
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference(
                    "Bearer",
                    document)
            ] = new List<string>()
        });
});

// ======================================================
// 3. MANEJO GLOBAL DE ERRORES
// ======================================================

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// ======================================================
// 4. BASE DE DATOS — ENTITY FRAMEWORK CORE
// ======================================================

builder.Services.AddDbContext<InventarioDbContext>(
    options =>
    {
        options.UseNpgsql(connectionString);
    });

// ======================================================
// 5. PERSISTENCIA — UNIT OF WORK
// ======================================================

builder.Services.AddScoped<UnitOfWork>();

// ======================================================
// 6. REPOSITORIOS DE INFRASTRUCTURE
// ======================================================

builder.Services.AddScoped<
    IProductoRepository,
    ProductoRepository>();

builder.Services.AddScoped<
    IMovimientoRepository,
    MovimientoRepository>();

builder.Services.AddScoped<
    IUsuarioRepository,
    UsuarioRepository>();

builder.Services.AddScoped<
    ISesionRepository,
    SesionRepository>();

// ======================================================
// 7. CONSULTAS DE INFRASTRUCTURE
// ======================================================

builder.Services.AddScoped<
    ICategoriaQueries,
    CategoriaQueries>();

builder.Services.AddScoped<
    IInventarioQueries,
    InventarioQueriesRepository>();

// ======================================================
// 8. SERVICIOS DE SEGURIDAD
// ======================================================

builder.Services.AddSingleton<
    IPasswordHasher,
    PasswordHasher>();

builder.Services.AddSingleton<
    ITokenService,
    TokenService>();

builder.Services.AddSingleton(
    new JwtOptions(
        jwtIssuer,
        jwtAudience,
        jwtSecret,
        accessMinutes));

// ======================================================
// 9. COMPONENTES COMPARTIDOS DE APPLICATION
// ======================================================

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSingleton(AuthPolicy.Default);

// ======================================================
// 10. VALIDADORES — FLUENTVALIDATION
// ======================================================

builder.Services.AddScoped<
    IValidator<LoginRequest>,
    LoginValidator>();

builder.Services.AddScoped<
    IValidator<RenovarSesionRequest>,
    RenovarSesionValidator>();

builder.Services.AddScoped<
    IValidator<DatosProductoRequest>,
    DatosProductoValidator>();

builder.Services.AddScoped<
    IValidator<FiltroProductosRequest>,
    FiltroProductosValidator>();

builder.Services.AddScoped<
    IValidator<RegistrarEntradaRequest>,
    RegistrarEntradaValidator>();

builder.Services.AddScoped<
    IValidator<RegistrarSalidaRequest>,
    RegistrarSalidaValidator>();

builder.Services.AddScoped<
    IValidator<FiltroInventarioRequest>,
    FiltroInventarioValidator>();

builder.Services.AddScoped<
    IValidator<FiltroKardexRequest>,
    FiltroKardexValidator>();

// ======================================================
// 11. SERVICIOS DE APPLICATION
// ======================================================

builder.Services.AddScoped<AuthCommands>();

builder.Services.AddScoped<ProductoCommands>();

builder.Services.AddScoped<ProductoQueries>();

builder.Services.AddScoped<MovimientoCommands>();

builder.Services.AddScoped<InventarioQueries>();

// ======================================================
// 12. AUTENTICACIÓN JWT
// ======================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecret)),

                ValidateLifetime = true,

                RequireExpirationTime = true,

                ClockSkew = TimeSpan.Zero,

                NameClaimType = ClaimTypes.Name,

                RoleClaimType = ClaimTypes.Role
            };
    });

// ======================================================
// 13. AUTORIZACIÓN
// ======================================================

builder.Services.AddAuthorization();

// ======================================================
// 14. LIMITACIÓN DE PETICIONES DE AUTENTICACIÓN
// ======================================================

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy(
        "auth",
        context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?
                    .ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
});

// ======================================================
// 15. CORS — FRONTEND ANGULAR
// ======================================================

var origins = builder.Configuration
    .GetSection("Cors:Origins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        if (origins.Length > 0)
        {
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

// ======================================================
// 16. CONSTRUIR LA APLICACIÓN
// ======================================================

var app = builder.Build();

// ======================================================
// 17. MIDDLEWARE DE ERRORES
// ======================================================

app.UseExceptionHandler();

// ======================================================
// 18. SWAGGER EN DESARROLLO
// ======================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// ======================================================
// 19. PIPELINE HTTP
// ======================================================

app.UseRouting();

app.UseCors("frontend");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

// ======================================================
// 20. RUTAS DE CONTROLADORES
// ======================================================

app.MapControllers();

// ======================================================
// 21. INICIAR API
// ======================================================

app.Run();