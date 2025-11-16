using Microsoft.EntityFrameworkCore;
using MagnetronTecnicalTest.Data;
using MagnetronTecnicalTest.Config;
using Microsoft.Extensions.Logging;

namespace MagnetronTecnicalTest.Tests.Helpers;

public class TestBase : IDisposable
{
    protected BillingDbContext Context { get; }
    protected JwtSettings JwtSettings { get; }

    public TestBase()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new BillingDbContext(options);
        
        JwtSettings = new JwtSettings
        {
            Secret = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("test-secret-key-for-jwt-token-generation-32-chars")),
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpMinutes = 60
        };
    }

    protected void SeedDatabase()
    {
        // Datos de prueba para Personas
        Context.Personas.AddRange(
            new Persona { Id = 1, Nombre = "Juan", Apellido = "Pérez", TipoDocumento = "DNI", Documento = "12345678" },
            new Persona { Id = 2, Nombre = "María", Apellido = "García", TipoDocumento = "DNI", Documento = "87654321" }
        );

        // Datos de prueba para Productos
        Context.Productos.AddRange(
            new Producto { Id = 1, Descripcion = "Producto A", UnidadMedida = "UN", Precio = 100.00m, Costo = 60.00m },
            new Producto { Id = 2, Descripcion = "Producto B", UnidadMedida = "KG", Precio = 50.00m, Costo = 30.00m }
        );

        // Datos de prueba para Usuarios
        Context.Usuarios.AddRange(
            new Usuario { Id = 1, Username = "testuser", PasswordHash = "hashedpassword", Role = "user" },
            new Usuario { Id = 2, Username = "admin", PasswordHash = "hashedpassword", Role = "admin" }
        );

        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context?.Dispose();
    }
}