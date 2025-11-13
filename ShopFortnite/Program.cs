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

// Configure Kestrel to listen on specific IP
builder.WebHost.UseUrls("http://192.168.100.9:5106");

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
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("http://192.168.100.9:5106/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<ShopFortnite.Services.IApiClientService, ShopFortnite.Services.ApiClientService>();

// Background Service
builder.Services.AddHostedService<FortniteSyncService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
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

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

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

// Map API controllers
app.MapControllers();

// Map Razor Pages (frontend)
app.MapRazorPages();

// Map MVC controllers (mantidos para compatibilidade)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
