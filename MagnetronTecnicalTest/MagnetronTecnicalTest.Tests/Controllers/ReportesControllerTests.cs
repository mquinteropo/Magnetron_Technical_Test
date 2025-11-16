using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MagnetronTecnicalTest.Controllers;
using MagnetronTecnicalTest.Tests.Helpers;

namespace MagnetronTecnicalTest.Tests.Controllers;

public class ReportesControllerTests : TestBase
{
    private ReportesController _controller;

    public ReportesControllerTests()
    {
        _controller = new ReportesController(Context);
        SeedDatabase();
        SeedReportesData();
    }

    private void SeedReportesData()
    {
        // Crear algunas facturas para que las vistas tengan datos
        var factura1 = new FacturaEncabezado
        {
            Id = 1,
            Numero = "FAC-001",
            Fecha = DateTime.UtcNow.AddDays(-5),
            PersonaId = 1,
            Detalles = new List<FacturaDetalle>
            {
                new FacturaDetalle
                {
                    Id = 1,
                    Linea = 1,
                    Cantidad = 2,
                    ProductoId = 1,
                    UnitPrice = 100.00m,
                    LineTotal = 200.00m,
                    FacturaId = 1
                },
                new FacturaDetalle
                {
                    Id = 2,
                    Linea = 2,
                    Cantidad = 1,
                    ProductoId = 2,
                    UnitPrice = 50.00m,
                    LineTotal = 50.00m,
                    FacturaId = 1
                }
            }
        };

        var factura2 = new FacturaEncabezado
        {
            Id = 2,
            Numero = "FAC-002",
            Fecha = DateTime.UtcNow.AddDays(-3),
            PersonaId = 2,
            Detalles = new List<FacturaDetalle>
            {
                new FacturaDetalle
                {
                    Id = 3,
                    Linea = 1,
                    Cantidad = 3,
                    ProductoId = 1,
                    UnitPrice = 100.00m,
                    LineTotal = 300.00m,
                    FacturaId = 2
                }
            }
        };

        Context.Facturas.AddRange(factura1, factura2);
        Context.SaveChanges();
    }

    [Fact]
    public async Task PersonasTotal_ShouldReturnOkResult()
    {
        // Act
        var result = await _controller.PersonasTotal();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task PersonaProductoMasCaro_ShouldReturnOkResult()
    {
        // Act
        var result = await _controller.PersonaProductoMasCaro();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task ProductosPorCantidadDesc_ShouldReturnOkResult()
    {
        // Act
        var result = await _controller.ProductosPorCantidadDesc();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task ProductosPorUtilidadDesc_ShouldReturnOkResult()
    {
        // Act
        var result = await _controller.ProductosPorUtilidadDesc();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task ProductosMargen_ShouldReturnOkResult()
    {
        // Act
        var result = await _controller.ProductosMargen();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task AllReportEndpoints_ShouldReturnOkResults()
    {
        // Act & Assert para todos los endpoints
        var personasTotalResult = await _controller.PersonasTotal();
        var personaProductoResult = await _controller.PersonaProductoMasCaro();
        var productosCantidadResult = await _controller.ProductosPorCantidadDesc();
        var productosUtilidadResult = await _controller.ProductosPorUtilidadDesc();
        var productosMargenResult = await _controller.ProductosMargen();

        // Assert
        var results = new IActionResult[]
        {
            personasTotalResult,
            personaProductoResult,
            productosCantidadResult,
            productosUtilidadResult,
            productosMargenResult
        };

        foreach (var result in results)
        {
            result.Should().BeOfType<OkObjectResult>();
        }
    }

    [Fact]
    public async Task ReportEndpoints_ShouldHandleEmptyData()
    {
        // Arrange - Limpiar datos existentes
        Context.FacturaDetalles.RemoveRange(Context.FacturaDetalles);
        Context.Facturas.RemoveRange(Context.Facturas);
        Context.Productos.RemoveRange(Context.Productos);
        Context.Personas.RemoveRange(Context.Personas);
        await Context.SaveChangesAsync();

        // Act
        var personasTotalResult = await _controller.PersonasTotal();
        var personaProductoResult = await _controller.PersonaProductoMasCaro();
        var productosCantidadResult = await _controller.ProductosPorCantidadDesc();
        var productosUtilidadResult = await _controller.ProductosPorUtilidadDesc();
        var productosMargenResult = await _controller.ProductosMargen();

        // Assert - Todos deberían retornar OK incluso con datos vacíos
        personasTotalResult.Should().BeOfType<OkObjectResult>();
        personaProductoResult.Should().BeOfType<OkObjectResult>();
        productosCantidadResult.Should().BeOfType<OkObjectResult>();
        productosUtilidadResult.Should().BeOfType<OkObjectResult>();
        productosMargenResult.Should().BeOfType<OkObjectResult>();
    }
}