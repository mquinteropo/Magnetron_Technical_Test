using Microsoft.EntityFrameworkCore;
using MagnetronTecnicalTest.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MagnetronTecnicalTest.Config;
using DotNetEnv;
using System.Security.Cryptography;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
// Cargar .env si existe; si no, confiar en variables de entorno del proceso (Docker/host)
if (File.Exists(envPath)) Env.Load(envPath);

string RequireEnv(string name) {
    var value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException($"Environment variable '{name}' is required.");
    return value;
}

var host = RequireEnv("POSTGRES_HOST");
var port = RequireEnv("POSTGRES_PORT");  
var database = RequireEnv("POSTGRES_DB");
var dbUser = RequireEnv("POSTGRES_USER");
var dbPwd = RequireEnv("POSTGRES_PASSWORD");
var connectionString = $"Host={host};Port={port};Database={database};Username={dbUser};Password={dbPwd}";

builder.Services.AddDbContext<BillingDbContext>(opt => opt.UseNpgsql(connectionString));

var jwtSecretRaw = RequireEnv("JWT_SECRET");
byte[] jwtKeyBytes = Encoding.UTF8.GetByteCount(jwtSecretRaw) < 32 
    ? SHA256.HashData(Encoding.UTF8.GetBytes(jwtSecretRaw))
    : Encoding.UTF8.GetBytes(jwtSecretRaw);

var jwtSecret = Convert.ToBase64String(jwtKeyBytes);
var jwtIssuer = RequireEnv("JWT_ISSUER");
var jwtAudience = RequireEnv("JWT_AUDIENCE");
if (!int.TryParse(RequireEnv("JWT_EXP_MINUTES"), out var jwtExpMinutes)) 
    throw new InvalidOperationException("JWT_EXP_MINUTES debe ser numérico");

var jwtSettings = new JwtSettings { Secret = jwtSecret, Issuer = jwtIssuer, Audience = jwtAudience, ExpMinutes = jwtExpMinutes };
builder.Services.AddSingleton(jwtSettings);
var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSettings.Secret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer, ValidAudience = jwtSettings.Audience, IssuerSigningKey = signingKey, ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Magnetron Billing API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.Http,
        Scheme = "bearer", BearerFormat = "JWT", Description = "JWT Authorization header"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }},
        Array.Empty<string>()
    }});
});

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try {
        var ctx = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        var conn = ctx.Database.GetDbConnection();
        conn.Open();
        logger.LogInformation("PostgreSQL OK. Version: {Version}", conn.ServerVersion);
        conn.Close();
    } catch (Exception ex) {
        logger.LogError(ex, "Error conectando a PostgreSQL");
    }
}

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Magnetron API v1"); c.RoutePrefix = string.Empty; });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Make the Program class accessible for integration tests
public partial class Program { }