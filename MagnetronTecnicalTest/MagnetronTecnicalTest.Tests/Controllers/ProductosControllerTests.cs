using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MagnetronTecnicalTest.Controllers;
using MagnetronTecnicalTest.Dtos;
using MagnetronTecnicalTest.Tests.Helpers;

namespace MagnetronTecnicalTest.Tests.Controllers;

public class ProductosControllerTests : TestBase
{
    private ProductosController _controller;

    public ProductosControllerTests()
    {
        _controller = new ProductosController(Context);
        SeedDatabase();
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllProductos()
    {
        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var productos = okResult!.Value as IEnumerable<ProductoDto>;
        productos.Should().HaveCount(2);
    }

    [Fact]
    public async Task Get_WithValidId_ShouldReturnProducto()
    {
        // Act
        var result = await _controller.Get(1);

        // Assert
        result.Result.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(1);
        result.Value.Descripcion.Should().Be("Producto A");
    }

    [Fact]
    public async Task Get_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.Get(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldCreateProducto()
    {
        // Arrange
        var dto = new CreateProductoDto("Producto C", "LT", 75.00m, 45.00m);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        var producto = createdResult!.Value as ProductoDto;
        producto.Should().NotBeNull();
        producto!.Descripcion.Should().Be("Producto C");
        producto.Precio.Should().Be(75.00m);
        producto.Costo.Should().Be(45.00m);
    }

    [Fact]
    public async Task UpdatePrecios_WithValidData_ShouldUpdatePrices()
    {
        // Arrange
        var dto = new UpdatePrecioProductoDto(120.00m, 70.00m);

        // Act
        var result = await _controller.UpdatePrecios(1, dto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        // Verify the update
        var updatedProducto = await Context.Productos.FindAsync(1L);
        updatedProducto!.Precio.Should().Be(120.00m);
        updatedProducto.Costo.Should().Be(70.00m);
    }

    [Fact]
    public async Task UpdatePrecios_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new UpdatePrecioProductoDto(120.00m, 70.00m);

        // Act
        var result = await _controller.UpdatePrecios(999, dto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldDeleteProducto()
    {
        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        // Verify deletion
        var producto = await Context.Productos.FindAsync(1L);
        producto.Should().BeNull();
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(0, 50)]  // Precio cero
    [InlineData(-10, 50)] // Precio negativo
    [InlineData(100, -20)] // Costo negativo
    public async Task Create_WithInvalidPrices_ShouldStillCreate(decimal precio, decimal costo)
    {
        // Arrange
        var dto = new CreateProductoDto("Producto Test", "UN", precio, costo);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        // Note: El controlador actual no valida precios negativos, 
        // pero las pruebas documentan el comportamiento actual
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }
}