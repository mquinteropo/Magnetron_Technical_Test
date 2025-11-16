using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MagnetronTecnicalTest.Data;
using System.Net.Http.Json;
using FluentAssertions;
using MagnetronTecnicalTest.Dtos;
using System.Net;

namespace MagnetronTecnicalTest.Tests.Integration;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private string? _bearerToken;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BillingDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<BillingDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestIntegrationDb");
                });
            });

            // Set minimal environment variables for the test
            builder.ConfigureAppConfiguration((context, config) =>
            {
                Environment.SetEnvironmentVariable("POSTGRES_HOST", "localhost");
                Environment.SetEnvironmentVariable("POSTGRES_PORT", "5432");
                Environment.SetEnvironmentVariable("POSTGRES_DB", "test_db");
                Environment.SetEnvironmentVariable("POSTGRES_USER", "test");
                Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "test");
                Environment.SetEnvironmentVariable("JWT_SECRET", "test-secret-key-for-jwt-token-generation-32-chars");
                Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
                Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");
                Environment.SetEnvironmentVariable("JWT_EXP_MINUTES", "60");
            });
        });

        _client = _factory.CreateClient();
    }

    private async Task EnsureAuthAsync()
    {
        if (!string.IsNullOrEmpty(_bearerToken)) return;
        var registerDto = new RegisterDto("it_user", "it_password");
        // Ignore conflict if user already exists
        var reg = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        if (reg.StatusCode == HttpStatusCode.Created || reg.StatusCode == HttpStatusCode.Conflict)
        {
            var loginDto = new LoginDto(registerDto.Username, registerDto.Password);
            var login = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            login.EnsureSuccessStatusCode();
            var auth = await login.Content.ReadFromJsonAsync<AuthResultDto>();
            _bearerToken = auth!.Token;
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_bearerToken}");
        }
        else
        {
            reg.EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task PersonasEndpoints_ShouldWorkCorrectly()
    {
        await EnsureAuthAsync();
        // Arrange - Seed data
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        
        context.Personas.Add(new Persona 
        { 
            Nombre = "Test", 
            Apellido = "User", 
            TipoDocumento = "DNI", 
            Documento = "12345678" 
        });
        await context.SaveChangesAsync();

        // Act & Assert - Get all personas
        var getResponse = await _client.GetAsync("/api/personas");
        getResponse.EnsureSuccessStatusCode();
        
        var personas = await getResponse.Content.ReadFromJsonAsync<List<PersonaDto>>();
        personas.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ProductosEndpoints_ShouldWorkCorrectly()
    {
        await EnsureAuthAsync();
        // Arrange - Seed data
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        
        context.Productos.Add(new Producto 
        { 
            Descripcion = "Test Product", 
            UnidadMedida = "UN", 
            Precio = 100.00m, 
            Costo = 60.00m 
        });
        await context.SaveChangesAsync();

        // Act & Assert - Get all productos
        var getResponse = await _client.GetAsync("/api/productos");
        getResponse.EnsureSuccessStatusCode();
        
        var productos = await getResponse.Content.ReadFromJsonAsync<List<ProductoDto>>();
        productos.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AuthEndpoints_ShouldWorkCorrectly()
    {
        // Arrange
        var registerDto = new RegisterDto("testuser", "testpassword");

        // Act - Register
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        
        // Assert
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResultDto>();
        authResult.Should().NotBeNull();
        authResult!.Token.Should().NotBeNullOrEmpty();

        // Act - Login
        var loginDto = new LoginDto("testuser", "testpassword");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        
        // Assert
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResultDto>();
        loginResult.Should().NotBeNull();
        loginResult!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ReportesEndpoints_ShouldReturnSuccess()
    {
        await EnsureAuthAsync();
        // Act & Assert - Test all report endpoints
        var endpoints = new[]
        {
            "/api/reportes/personas-total",
            "/api/reportes/persona-producto-mas-caro",
            "/api/reportes/productos-cantidad-desc",
            "/api/reportes/productos-utilidad-desc",
            "/api/reportes/productos-margen"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task FacturasEndpoints_ShouldReturnSuccess()
    {
        await EnsureAuthAsync();
        // Act
        var response = await _client.GetAsync("/api/facturas");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task InvalidEndpoint_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/api/nonexistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}