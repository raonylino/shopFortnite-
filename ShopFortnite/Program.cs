using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using ShopFortnite.Infrastructure.Data;
using ShopFortnite.Domain.Interfaces;
using ShopFortnite.Infrastructure.Repositories;
using ShopFortnite.Application.UseCases;
using ShopFortnite.Application.Mappings;
using ShopFortnite.Infrastructure.ExternalServices;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on all interfaces and Railway's PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "5106";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Configure Kestrel options
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add API Controllers
builder.Services.AddControllers();

// Add Razor Pages
builder.Services.AddRazorPages();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString, sqliteOptions =>
    {
        sqliteOptions.CommandTimeout(30); // 30 segundos timeout
    });
    options.EnableSensitiveDataLogging(false); // Desabilita logs sensíveis em produção
    options.EnableDetailedErrors(false); // Menos logs
});

// Unit of Work and Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICosmeticService, CosmeticService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IUserService, UserService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// HttpClient for Fortnite API
builder.Services.AddHttpClient("FortniteApi", client =>
{
    client.BaseAddress = new Uri("https://fortnite-api.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// HttpClient for internal API (Razor Pages)
// builder.Services.AddHttpClient("ApiClient", client =>
// {
//     client.BaseAddress = new Uri("http://192.168.100.9:5106/");
//     client.DefaultRequestHeaders.Add("Accept", "application/json");
// });

builder.Services.AddHttpClient("ApiClient", client =>
{
    // Railway fornece a URL pública via variável de ambiente
    var baseUrl = Environment.GetEnvironmentVariable("RAILWAY_PUBLIC_DOMAIN");
    if (!string.IsNullOrEmpty(baseUrl))
    {
        client.BaseAddress = new Uri($"https://{baseUrl}");
    }
    else
    {
        // Fallback para configuração local
        baseUrl = builder.Configuration["AppBaseUrl"] ?? "http://localhost:5106";
        client.BaseAddress = new Uri(baseUrl);
    }
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<ShopFortnite.Services.IApiClientService, ShopFortnite.Services.ApiClientService>();

// Background Service
builder.Services.AddHostedService<FortniteSyncService>();

// JWT Authentication - pega da variável de ambiente ou do appsettings
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? "ShopFortniteSecretKey2024!@#MinimumLength32Characters!!"; // Fallback seguro

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? "ShopFortnite";

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? "ShopFortniteUsers";

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
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
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ShopFortnite API",
        Version = "v1",
        Description = "API para gerenciamento de cosméticos do Fortnite"
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Garante que o diretório do banco existe (Railway com volume)
var dbPath = "/app/data";
var startupLoggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var startupLogger = startupLoggerFactory.CreateLogger<Program>();

if (Directory.Exists(dbPath))
{
    startupLogger.LogWarning($"Diretório do banco encontrado: {dbPath}");
}
else if (!app.Environment.IsDevelopment())
{
    // Em produção, cria o diretório se não existir
    try
    {
        Directory.CreateDirectory(dbPath);
        startupLogger.LogWarning($"Diretório do banco criado: {dbPath}");
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, $"ERRO ao criar diretório {dbPath}");
    }
}

// Apply migrations in background (não bloqueia o startup)
_ = Task.Run(async () =>
{
    try
    {
        await Task.Delay(1000); // Aguarda 1 segundo para app iniciar
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Iniciando migração do banco de dados em background...");

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        logger.LogWarning("Migração do banco de dados concluída com sucesso");
    }
    catch (Exception ex)
    {
        // Log mas não quebra a aplicação
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "ERRO ao aplicar migrations - continuando sem banco");
    }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Enable Swagger in all environments for API testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopFortnite API v1");
    c.RoutePrefix = "swagger";
});

// Comentado porque estamos usando HTTP ao invés de HTTPS
// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint para o Railway
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Map API controllers
app.MapControllers();

// Map Razor Pages (frontend)
app.MapRazorPages();

// Map MVC controllers (mantidos para compatibilidade)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogWarning($"ShopFortnite está rodando na porta {port}");
logger.LogWarning($"Environment: {app.Environment.EnvironmentName}");

app.Run();
